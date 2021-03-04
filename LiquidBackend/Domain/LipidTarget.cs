using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.Data.Composition;
using LiquidBackend.Util;

namespace LiquidBackend.Domain
{
    public class LipidTarget
    {
        public string CommonName { get; }
        public LipidClass LipidClass { get; }
        public FragmentationMode FragmentationMode { get; }
        public Composition Composition { get; }
        public List<AcylChain> AcylChainList { get; }
        public Adduct Adduct { get; }
        public LipidType LipidType { get; }

        public double MzRounded
        {
            get
            {
                if (Composition != null)
                    return Math.Round(Composition.Mass / Charge, 4);

                return double.Parse(CommonName); //If this is an unknown target i.e. composition is unknown
            }
        }

        public int Charge { get; }

        public List<MsMsSearchUnit> SortedMsMsSearchUnits
        {
            get
            {
                if (LipidClass != LipidClass.Unknown)
                    return GetMsMsSearchUnits().OrderBy(x => x.Mz).ToList();

                return null;
            }
        }

        public string EmpiricalFormula => Composition.ToPlainString();

        public string StrippedDisplay
        {
            get
            {
                var stringBuilder = new StringBuilder();
                if (LipidClass == LipidClass.Unknown)
                {
                    stringBuilder.Append(CommonName);
                    return stringBuilder.ToString();
                }
                if (LipidClass == LipidClass.Ganglioside)
                {
                    stringBuilder.Append(CommonName.Split('(')[0]);
                }
                else if (LipidClass == LipidClass.Ubiquinone)
                {
                    stringBuilder.AppendFormat("Co{0}", CommonName.Split(' ')[1]);
                }
                else
                {
                    stringBuilder.Append(LipidClass);
                }

                var acylChainList = AcylChainList;
                if (acylChainList.Count > 0)
                {
                    stringBuilder.Append("(");
                    //var carbons = 0;
                    //var db = 0;
                    for (var i = 0; i < acylChainList.Count; i++)
                    {
                        var acylChain = acylChainList[i];
                        /*if (LipidClass == LipidClass.Ganglioside)
                        {
                            carbons += acylChain.NumCarbons;
                            db += acylChain.NumDoubleBonds;
                            if (i == acylChainList.Count - 1)
                            {
                                if (LipidType == LipidType.TwoChainsDihidroxy) stringBuilder.Append(carbons + ":" + db + "(2OH)");
                                else stringBuilder.Append(carbons + ":" + db);
                            }
                        }*/
                        //else
                        //{
                        stringBuilder.Append(acylChain);
                        if (i < acylChainList.Count - 1)
                            stringBuilder.Append("/");
                        //}
                    }
                    stringBuilder.Append(")");
                }

                return stringBuilder.ToString();
            }
        }

        public string AdductString => Adduct.ToString();

        public LipidTarget(
            string commonName,
            LipidClass lipidClass,
            FragmentationMode fragmentationMode,
            Composition composition,
            IEnumerable<AcylChain> acylChainList,
            Adduct adduct = Adduct.Hydrogen, int charge = 1)
        {
            CommonName = commonName;
            LipidClass = lipidClass;
            FragmentationMode = fragmentationMode;
            Composition = composition;
            AcylChainList = acylChainList.ToList();
            Adduct = adduct;
            Charge = charge;

            LipidType = FigureOutLipidType();
        }

        public List<MsMsSearchUnit> GetMsMsSearchUnits()
        {
            // return LipidUtil.CreateMsMsSearchUnits(CommonName, Composition.Mass / Charge, LipidClass, FragmentationMode, AcylChainList);
            return LipidUtil.CreateMsMsSearchUnitsFromFragmentationRules(CommonName, Composition.Mass / Charge, LipidClass, FragmentationMode, AcylChainList, LipidRules.LipidFragmentationRules);
        }

        protected bool Equals(LipidTarget other)
        {
            if (LipidClass != LipidClass.Unknown && other.LipidClass != LipidClass.Unknown)
            {
                return LipidClass == other.LipidClass && FragmentationMode == other.FragmentationMode &&
                       Equals(Composition, other.Composition) &&
                       AcylChainList.OrderBy(x => x.NumCarbons)
                           .ThenBy(x => x.NumDoubleBonds)
                           .ThenBy(x => x.AcylChainType)
                           .SequenceEqual(
                               other.AcylChainList.OrderBy(x => x.NumCarbons)
                                   .ThenBy(x => x.NumDoubleBonds)
                                   .ThenBy(x => x.AcylChainType));
            }

            return LipidClass == other.LipidClass && FragmentationMode == other.FragmentationMode &&
                   CommonName.Equals(other.CommonName);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((LipidTarget)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)LipidClass;
                hashCode = (hashCode * 397) ^ (int)FragmentationMode;
                hashCode = (hashCode * 397) ^ (Composition?.GetHashCode() ?? 0);
                if (AcylChainList != null)
                    hashCode = AcylChainList.OrderBy(x => x.NumCarbons).ThenBy(x => x.NumDoubleBonds).ThenBy(x => x.AcylChainType).Aggregate(hashCode, (current, acylChain) => (current * 397) ^ acylChain.GetHashCode());
                return hashCode;
            }
        }

        private LipidType FigureOutLipidType()
        {
            if (LipidClass == LipidClass.Ubiquinone || LipidClass == LipidClass.Cholesterol || LipidClass == LipidClass.Unknown)
                return LipidType.Standard;

            var chainCount = 0;
            var standardChainCount = 0;
            var plasmogenChainCount = 0;
            var etherChainCount = 0;
            var OOHChainCount = 0;
            var F2IsoPChainCount = 0;
            var oxoCHOChainCount = 0;
            var oxoCOOHChainCount = 0;
            var dihydroxyChainCount = 0;
            var monohydroxyChainCount = 0;
            var trihydroChainCount = 0;

            foreach (var acylChain in AcylChainList)
            {
                if (acylChain.NumCarbons < 1)
                    continue;

                var chainType = acylChain.AcylChainType;
                if (chainType == AcylChainType.Standard)
                    standardChainCount++;
                else if (chainType == AcylChainType.Plasmalogen)
                    plasmogenChainCount++;
                else if (chainType == AcylChainType.Ether)
                    etherChainCount++;
                else if (chainType == AcylChainType.OxoCHO)
                    oxoCHOChainCount++;
                else if (chainType == AcylChainType.OxoCOOH)
                    oxoCOOHChainCount++;
                else if (chainType == AcylChainType.Monohydro)
                    monohydroxyChainCount++;
                else if (chainType == AcylChainType.Dihydro || chainType == AcylChainType.Hydroxy)
                    dihydroxyChainCount++;
                else if (chainType == AcylChainType.Trihydro)
                    trihydroChainCount++;
                else if (chainType == AcylChainType.OOH || chainType == AcylChainType.OOHOH)
                    OOHChainCount++;
                else if (chainType == AcylChainType.F2IsoP)
                    F2IsoPChainCount++;

                chainCount++;
            }

            if (chainCount == 1)
            {
                if (standardChainCount == 1)
                    return LipidType.SingleChain;
                if (plasmogenChainCount == 1)
                    return LipidType.SingleChainPlasmogen;
                if (etherChainCount == 1)
                    return LipidType.SingleChainEther;
                if (monohydroxyChainCount == 1)
                    return LipidType.SingleChainMonoHydroxy;
                if (dihydroxyChainCount == 1)
                    return LipidType.SingleChainDihydroxy;
            }
            if (chainCount == 2)
            {
                if (standardChainCount == 2)
                    return LipidType.TwoChains;
                if (plasmogenChainCount == 1)
                    return LipidType.TwoChainsPlasmogen;
                if (etherChainCount >= 1)
                    return LipidType.TwoChainsEther; //etherChainCount == 1
                if (oxoCHOChainCount == 1)
                    return LipidType.TwoChainsOxoCHO;
                if (oxoCOOHChainCount == 1)
                    return LipidType.TwoChainsOxoCOOH;
                if (OOHChainCount == 1)
                    return LipidType.TwoChainsOOH;
                if (F2IsoPChainCount == 1)
                    return LipidType.TwoChainsF2IsoP;
                if (trihydroChainCount == 1)
                {
                    if (dihydroxyChainCount == 1)
                        return LipidType.TwoChainsDihidroxyPhyto;

                    return LipidType.TwoChainsPhyto;
                }
                if (dihydroxyChainCount == 1)
                    return LipidType.TwoChainsDihidroxy;
                if (dihydroxyChainCount == 2)
                    return LipidType.TwoChainsTwoDihidroxy;
                if (monohydroxyChainCount == 1)
                    return LipidType.TwoChainsMonohydroxy;
            }
            if (chainCount == 3)
            {
                return LipidType.ThreeChains;
            }
            if (chainCount == 4)
            {
                return LipidType.FourChains;
            }

            throw new SystemException("Unable to determine LipidType for LipidTarget: " + ToString());
        }
    }
}
