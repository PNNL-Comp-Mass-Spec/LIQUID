using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InformedProteomics.Backend.Data.Composition;
using LiquidBackend.Domain;

namespace LiquidBackend.Util
{
    using InformedProteomics.Backend.Data.Biology;
    using InformedProteomics.Backend.Data.Spectrometry;

    public class LipidUtil
    {
        public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, LipidClass lipidClass, FragmentationMode fragmentationMode, IEnumerable<AcylChain> acylChainList)
        {
            var composition = Composition.ParseFromPlainString(empiricalFormula);
            return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
        }

        public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, LipidClass lipidClass, FragmentationMode fragmentationMode)
        {
            var composition = Composition.ParseFromPlainString(empiricalFormula);
            var acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
            return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
        }

        public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, FragmentationMode fragmentationMode)
        {
            var composition = Composition.ParseFromPlainString(empiricalFormula);
            var acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
            var lipidClass = ParseLipidCommonNameIntoClass(commonName);
            return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
        }

        public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, string fragmentationMode)
        {
            var composition = Composition.ParseFromPlainString(empiricalFormula);
            var acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
            var lipidClass = ParseLipidCommonNameIntoClass(commonName);
            var fragmentationModeAsEnum = (FragmentationMode)Enum.Parse(typeof(FragmentationMode), fragmentationMode);
            return new LipidTarget(commonName, lipidClass, fragmentationModeAsEnum, composition, acylChainList);
        }

        public static LipidTarget CreateLipidTarget(string commonName, FragmentationMode fragmentationMode, Adduct adduct)
        {
            var composition = ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName);
            var compositionOfAdduct = GetCompositionOfAdduct(adduct);
            var charge = IonCharge(adduct);

            if (adduct == Adduct.Acetate) composition += compositionOfAdduct;
            else if (fragmentationMode == FragmentationMode.Negative) composition -= compositionOfAdduct;
            else if (fragmentationMode == FragmentationMode.Positive) composition += compositionOfAdduct;

            var acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
            var lipidClass = ParseLipidCommonNameIntoClass(commonName);

            return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList, adduct, charge);
        }

        public static LipidTarget CreateLipidTarget(double mz, FragmentationMode fragmentationMode, Adduct adduct)
        {
            var compositionOfAdduct = GetCompositionOfAdduct(adduct);
            var charge = IonCharge(adduct);

            return new LipidTarget(mz.ToString(), LipidClass.Unknown, fragmentationMode, null, null, adduct, charge);
        }


        public static Composition GetCompositionOfAdduct(Adduct adduct)
        {
            if (adduct == Adduct.Hydrogen)
            {
                return Composition.Hydrogen;
            }
            if (adduct == Adduct.Dihydrogen)
            {
                return new Composition(0, 2, 0, 0, 0);
            }
            if (adduct == Adduct.Ammonium)
            {
                return new Composition(0, 4, 1, 0, 0);
            }
            if (adduct == Adduct.Acetate)
            {
                return new Composition(2, 3, 0, 2, 0);
            }
            if (adduct == Adduct.Sodium)
            {
                return new Composition(0, 0, 0, 0, 0, 0, new Tuple<Atom, short>(Atom.Get("Na"), 1));
            }
            if (adduct == Adduct.Potassium)
            {
                return new Composition(0, 0, 0, 0, 0, 0, new Tuple<Atom, short>(Atom.Get("K"), 1));
            }

            throw new SystemException("Unrecognized Adduct: " + adduct);
        }

        public static LipidClass ParseLipidCommonNameIntoClass(string commonName)
        {
            //Special Cases with paretheses in name go at top
            if (commonName.Contains("M(IP)2C")) return LipidClass.MIP2C;


            var commonNameSplit = commonName.Split('(');
            var classAbbrev = commonNameSplit[0];

            if (classAbbrev.Length == 0)
            {
                if (commonNameSplit[1].Contains("sulf")) return LipidClass.Sulfatide;
            }

            LipidClass lipidClass;
            var classFound = Enum.TryParse(classAbbrev, true, out lipidClass);

            if (!classFound)
            {
                // Add in any extra search criteria for classes that may not have straight forward parsing
                if (classAbbrev.Contains("Cer-2H2O")) return LipidClass.Cer2H2O;
                if (classAbbrev.Contains("Cer-H2O")) return LipidClass.CerH2O;
                if (classAbbrev.Contains("PIP2")) return LipidClass.PIP2;
                if (classAbbrev.Contains("PIP3")) return LipidClass.PIP3;
                if (classAbbrev.Contains("PIP")) return LipidClass.PIP;
                if (classAbbrev.Contains("cholest")) return LipidClass.Cholesterol;
                if (classAbbrev.Contains("sulf")) return LipidClass.Sulfatide;
                if (classAbbrev.Contains("DGTS/A")) return LipidClass.DGTSA;
                if (classAbbrev.Contains("PE-Cer")) return LipidClass.PE_Cer;
                if (classAbbrev.Contains("PI-Cer")) return LipidClass.PI_Cer;
                if (classAbbrev.Contains("PE-NMe2")) return LipidClass.PE_NMe2;
                if (classAbbrev.Contains("PE-NMe")) return LipidClass.PE_NMe;
                if (classAbbrev.Contains("Coenzyme Q")) return LipidClass.Ubiquinone;
                if (classAbbrev.Contains("GM") || classAbbrev.Contains("GD") || classAbbrev.Contains("GT") || classAbbrev.Contains("GQ")) return LipidClass.Ganglioside;

                throw new SystemException("Unrecognized lipid class for " + commonName);
            }

            return lipidClass;
        }

        public static IEnumerable<AcylChain> ParseLipidCommonNameIntoAcylChains(string commonName)
        {
            var name = commonName;
            var stereoChem = new Regex(@"\(\d+(E|Z)(\,[^\)]+\)|\))");
            var steroMatch = Regex.Match(name, @"\(\d+(E|Z)(\,[^\)]+\)|\))");
            if (steroMatch.Success) name = stereoChem.Replace(name, "");
            name = Regex.Replace(name, @"\[\w\]", "");
            var matchCollection = Regex.Matches(name, "([mdtOP]-?)?\\d+:\\d+(\\(((\\d+)?(OH|\\(OH\\))|CHO|COOH)\\))?");

            var acylChains = from object match in matchCollection select new AcylChain(match.ToString());
            return acylChains;
        }

        public static Composition ParseLipidCommonNameIntoCompositionWithoutAdduct(string commonName)
        {
            var lipidClass = ParseLipidCommonNameIntoClass(commonName);
            var fattyAcylChains = ParseLipidCommonNameIntoAcylChains(commonName).ToList();

            var numCarbons = fattyAcylChains.Sum(x => x.NumCarbons);
            var numDoubleBonds = fattyAcylChains.Sum(x => x.NumDoubleBonds);
            var hydroxyCount = fattyAcylChains.Sum(x => x.HydroxyCount);

            var numChains = fattyAcylChains.Count(x => x.NumCarbons > 0);
            var containsEther = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Ether) == 1;
            var containsDiether = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Ether) > 1;
            var containsPlasmogen = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Plasmalogen) > 0;
            var containsOH = hydroxyCount > 0;
            var isOxoCHO = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.OxoCHO) > 0;
            var isOxoCOOH = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.OxoCOOH) > 0;
            var dihydro = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Dihydro) == 1;
            var trihydro = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Trihydro) == 1;
            var monohydro = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Monohydro) == 1;
            var tri = trihydro ? 1 : 0;
            var mono = monohydro ? 1 : 0;

            switch (lipidClass)
            {
                case LipidClass.PC:
                    if (numChains > 1)
                    {
                        if (containsEther && isOxoCOOH)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 0 - 2 * numDoubleBonds, 1, 9, 0, 1);
                        }
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 2 - 2 * numDoubleBonds, 1, 7, 0, 1);
                        }
                        if (containsDiether)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 4 - 2 * numDoubleBonds, 1, 6, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 0 - 2 * numDoubleBonds, 1, 7, 0, 1);
                        }
                        if (isOxoCHO)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) - 2 - 2 * numDoubleBonds, 1, 9, 0, 1);
                        }
                        if (isOxoCOOH)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) - 2 - 2 * numDoubleBonds, 1, 10, 0, 1);
                        }
                        if (containsOH)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 0 - 2 * numDoubleBonds, 1, 9 + hydroxyCount, 0, 1);
                        }
                        return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 0 - 2 * numDoubleBonds, 1, 8, 0, 1);
                    }
                    else
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 4 - 2 * numDoubleBonds, 1, 6, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 2 - 2 * numDoubleBonds, 1, 6, 0, 1);
                        }
                        return new Composition(numCarbons + 8, 2 * (numCarbons + 8) + 2 - 2 * numDoubleBonds, 1, 7, 0, 1);
                    }
                case LipidClass.PE:
                    if (numChains > 1)
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 2 - 2 * numDoubleBonds, 1, 7, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 0 - 2 * numDoubleBonds, 1, 7, 0, 1);
                        }
                        if (containsOH)
                        {
                            return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 0 - 2 * numDoubleBonds, 1, 8 + hydroxyCount, 0, 1);
                        }
                        return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 0 - 2 * numDoubleBonds, 1, 8, 0, 1);
                    }
                    else
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 4 - 2 * numDoubleBonds, 1, 6, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 2 - 2 * numDoubleBonds, 1, 6, 0, 1);
                        }
                        return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 2 - 2 * numDoubleBonds, 1, 7, 0, 1);
                    }
                case LipidClass.PE_Cer:
                    if (numChains > 1)
                    {
                        if (containsOH) return new Composition(numCarbons + 2, 2 * (numCarbons + 3) + 1 - 2 * numDoubleBonds, 2, 7 + tri, 0, 1);
                        return new Composition(numCarbons + 2, 2 * (numCarbons + 3) + 1 - 2 * numDoubleBonds, 2, 6 + tri - mono, 0, 1);
                    }
                    else
                    {
                        return new Composition(numCarbons + 2, 2 * (numCarbons + 4) + 1 - 2 * numDoubleBonds, 2, 5 + tri - mono, 0, 1);
                    }
                case LipidClass.PE_NMe:
                    if (numChains > 1)
                    {
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 0 - 2 * numDoubleBonds, 1, 8, 0, 1);
                    }
                    break;
                case LipidClass.PE_NMe2:
                    if (numChains > 1)
                    {
                        return new Composition(numCarbons + 7, 2 * (numCarbons + 7) + 0 - 2 * numDoubleBonds, 1, 8, 0, 1);
                    }
                    break;
                case LipidClass.PS:
                    if (numChains > 1)
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 0 - 2 * numDoubleBonds, 1, 9, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 2 - 2 * numDoubleBonds, 1, 9, 0, 1);
                        }
                        if (containsOH)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 2 - 2 * numDoubleBonds, 1, 11, 0, 1);
                        }
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 2 - 2 * numDoubleBonds, 1, 10, 0, 1);
                    }
                    else
                    {
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 0 - 2 * numDoubleBonds, 1, 9, 0, 1);
                    }
                case LipidClass.PG:
                    if (numChains > 1)
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 1 - 2 * numDoubleBonds, 0, 9, 0, 1);
                        }
                        if (containsDiether)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 3 - 2 * numDoubleBonds, 0, 8, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 0, 9, 0, 1);
                        }
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 0, 10, 0, 1);
                    }
                    else
                    {
                        if (containsEther)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 3 - 2 * numDoubleBonds, 0, 8, 0, 1);
                        }
                        if (containsPlasmogen)
                        {
                            return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 1 - 2 * numDoubleBonds, 0, 8, 0, 1);
                        }
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) + 1 - 2 * numDoubleBonds, 0, 9, 0, 1);
                    }
                case LipidClass.PI_Cer:

                    if (containsOH)
                    {
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - numDoubleBonds, 1, 11 + tri - mono + hydroxyCount, 0, 1);
                    }
                    else
                    {
                        return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - numDoubleBonds, 1, 11 + tri - mono, 0, 1);
                    }
                case LipidClass.Cer:
                    if (containsOH)
                    {
                        return new Composition(numCarbons, 2 * (numCarbons + 0) + 1 - 2 * numDoubleBonds, 1, 4 + tri - mono, 0, 0);
                    }
                    else
                    {
                        if (numChains > 1)
                        {
                            return new Composition(numCarbons, 2 * (numCarbons + 0) + 1 - 2 * numDoubleBonds, 1, 3 + tri - mono, 0, 0);
                        }
                        return new Composition(numCarbons, 2 * (numCarbons + 0) + 3 - 2 * numDoubleBonds, 1, 2 + tri - mono, 0, 0);
                    }
                case LipidClass.CerH2O:
                    return new Composition(numCarbons, 2 * (numCarbons + 0) + 1 - 2 * numDoubleBonds, 1, 3 + tri - mono, 0, 0) - Composition.H2O;
                case LipidClass.Cer2H2O:
                    return new Composition(numCarbons, 2 * (numCarbons + 0) + 1 - 2 * numDoubleBonds, 1, 3 + tri - mono, 0, 0) - Composition.H2O - Composition.H2O;
                case LipidClass.SM:
                    return new Composition(numCarbons + 5, 2 * (numCarbons + 5) + 3 - 2 * numDoubleBonds, 2, 6, 0, 1);
                case LipidClass.GalCer:
                    if (containsOH) return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 1, 9 + tri - mono, 0, 0);
                    return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 1, 8, 0, 0);
                case LipidClass.GlcCer:
                    if (containsOH) return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 1, 9 + tri - mono, 0, 0);
                    return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 1, 8, 0, 0);
                case LipidClass.LacCer:
                    if (containsOH) return new Composition(numCarbons + 12, 2 * (numCarbons + 12) - 1 - 2 * numDoubleBonds, 1, 14 + tri - mono, 0, 0);
                    return new Composition(numCarbons + 12, 2 * (numCarbons + 12) - 1 - 2 * numDoubleBonds, 1, 13, 0, 0);
                case LipidClass.CerP:
                    return new Composition(numCarbons, 2 * (numCarbons + 0) + 2 - 2 * numDoubleBonds, 1, 6 + tri - mono, 0, 1);
                case LipidClass.Cholesterol:
                    return new Composition(27, 46, 0, 1, 0, 0);
                case LipidClass.CE:
                    return new Composition(numCarbons + 27, 2 * (numCarbons + 27) - 10 - 2 * numDoubleBonds, 0, 2, 0, 0);
                case LipidClass.Ubiquinone:
                    if (commonName.EndsWith("Q10")) return new Composition(59, 90, 0, 4, 0, 0);
                    else if (commonName.EndsWith("Q4")) return new Composition(29, 42, 0, 4, 0, 0);
                    else if (commonName.EndsWith("Q6")) return new Composition(39, 58, 0, 4, 0, 0);
                    else if (commonName.EndsWith("Q8")) return new Composition(49, 74, 0, 4, 0, 0);
                    else if (commonName.EndsWith("Q9")) return new Composition(54, 82, 0, 4, 0, 0);
                    break;
                case LipidClass.MG:
                    return new Composition(numCarbons + 3, 2 * (numCarbons + 3) + 0 - 2 * numDoubleBonds, 0, 4, 0, 0);
                case LipidClass.DG:
                    return new Composition(numCarbons + 3, 2 * (numCarbons + 3) - 2 - 2 * numDoubleBonds, 0, 5, 0, 0);
                case LipidClass.TG:
                    return new Composition(numCarbons + 3, 2 * (numCarbons + 3) - 4 - 2 * numDoubleBonds, 0, 6, 0, 0);
                case LipidClass.MGDG:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 4 - 2 * numDoubleBonds, 0, 10, 0, 0);
                case LipidClass.SQDG:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 4 - 2 * numDoubleBonds, 0, 12, 1, 0);
                case LipidClass.DGDG:
                    return new Composition(numCarbons + 15, 2 * (numCarbons + 15) - 6 - 2 * numDoubleBonds, 0, 15, 0, 0);
                case LipidClass.PI:
                    if (numChains > 1)
                    {
                        return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 3 - 2 * numDoubleBonds, 0, 13, 0, 1);
                    }
                    else
                    {
                        return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 1 - 2 * numDoubleBonds, 0, 12, 0, 1);
                    }
                case LipidClass.DGTSA:
                    return new Composition(numCarbons + 10, 2 * numCarbons + 17 - 2 * numDoubleBonds, 1, 7, 0, 0);
                case LipidClass.PIP:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 2 - 2 * numDoubleBonds, 0, 16, 0, 2);
                case LipidClass.PIP2:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 1 - 2 * numDoubleBonds, 0, 19, 0, 3);
                case LipidClass.PIP3:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) + 0 - 2 * numDoubleBonds, 0, 22, 0, 4);
                case LipidClass.PA:
                    if (numChains > 1)
                    {
                        return new Composition(numCarbons + 3, 2 * (numCarbons + 3) - 1 - 2 * numDoubleBonds, 0, 8, 0, 1);
                    }
                    else
                    {
                        return new Composition(numCarbons + 3, 2 * (numCarbons + 3) + 1 - 2 * numDoubleBonds, 0, 7, 0, 1);
                    }
                case LipidClass.CL:
                    return new Composition(numCarbons + 9, 2 * (numCarbons + 9) - 4 - 2 * numDoubleBonds, 0, 17, 0, 2);
                case LipidClass.Sulfatide:
                    return new Composition(numCarbons + 6, 2 * (numCarbons + 6) - 1 - 2 * numDoubleBonds, 1, 11, 1, 0);
                case LipidClass.WE:
                    return new Composition(numCarbons, 2 * numCarbons - 2 * numDoubleBonds, 0, 2, 0);
                case LipidClass.Ganglioside:
                    var glycan = ParseGlycan(commonName);
                    var cer = new Composition(numCarbons, 2 * (numCarbons + 0) + 1 - 2 * numDoubleBonds, 1, 3 + tri - mono + hydroxyCount, 0, 0);
                    return cer + glycan - Composition.H2O;
                case LipidClass.MIPC:
                    return new Composition(numCarbons + 12, (2 * (numCarbons + 12) - 2 - numDoubleBonds), 1, 16 + tri - mono + hydroxyCount, 0, 1);
                case LipidClass.MIP2C:
                    return new Composition(numCarbons + 18, (2 * (numCarbons + 18) - 3 - numDoubleBonds), 1, 24 + tri - mono + hydroxyCount, 0, 2);
                case LipidClass.anandamide:
                    return new Composition(numCarbons + 2, 2 * (numCarbons + 2) + 1 - 2 * numDoubleBonds, 1, 2, 0, 0);
                case LipidClass.carnitine:
                    return new Composition(numCarbons + 7, 2 * (numCarbons + 7) - 1 - 2 * numDoubleBonds, 1, 4, 0, 0);
                case LipidClass.FAHFA:
                    return new Composition(numCarbons, 2 * numCarbons - 2 - 2 * numDoubleBonds, 0, 4, 0, 0);
            }

            throw new SystemException("No empirical formula calculator found for " + commonName);
        }

        private static Composition ParseGlycan(string commonName)
        {
            var ganglioName = commonName.Split('(')[0];
            int NeuAc;
            var NeuGc = 0;
            int Hex;
            var HexNAc = 0;
            var DeoxyHex = 0;

            switch (ganglioName[1])
            {
                case 'M':
                    NeuAc = 1;
                    break;
                case 'D':
                    NeuAc = 2;
                    break;
                case 'T':
                    NeuAc = 3;
                    break;
                case 'Q':
                    NeuAc = 4;
                    break;
                default:
                    throw new SystemException(ganglioName[1] + " not a valid indicator of sialic acid residues for ganglioside target " + commonName);
            }
            switch (ganglioName[2])
            {
                case '1':
                    Hex = 3;
                    HexNAc = 1;
                    break;
                case '2':
                    Hex = 2;
                    HexNAc = 1;
                    break;
                case '3':
                    Hex = 2;
                    break;
                case '4':
                    Hex = 1;
                    break;
                default:
                    throw new SystemException(ganglioName[2] + " not a valid indicator of glycan chain for ganglioside target " + commonName);
            }

            var sugarCount = Hex + HexNAc + DeoxyHex + NeuAc + NeuGc;
            var carbons = 6 * Hex + 8 * HexNAc + 6 * DeoxyHex + 11 * NeuAc + 11 * NeuGc;
            var hydrogens = 12 * Hex + 15 * HexNAc + 12 * DeoxyHex + 19 * NeuAc + 19 * NeuGc - 2 * (sugarCount - 1); //Subtract H2O for each glycosidic bond formed
            var nitrogens = 0 * Hex + 1 * HexNAc + 0 * DeoxyHex + 1 * NeuAc + 1 * NeuGc;
            var oxygens = 6 * Hex + 6 * HexNAc + 5 * DeoxyHex + 9 * NeuAc + 10 * NeuGc - 1 * (sugarCount - 1);

            return new Composition(carbons, hydrogens, nitrogens, oxygens, 0);
        }

        public static List<MsMsSearchUnit> CreateMsMsSearchUnits(
            string commonName,
            double precursorMz,
            LipidClass lipidClass,
            FragmentationMode fragmentationMode,
            List<AcylChain> acylChainList)
        {
            var msMsSearchUnitList = new List<MsMsSearchUnit>();

            if (fragmentationMode == FragmentationMode.Positive)
            {
                if (lipidClass == LipidClass.PC)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 13, 1, 3, 0, 1).Mass, "C5H15O4NP-H2O"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 15, 1, 4, 0, 1).Mass, "M-C5H15O4NP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 14, 1, 4, 0, 1).Mass, "M-C5H14O4NP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 14, 1, 1, 0, 0).Mass, "C5H14ON"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 6, 0, 4, 0, 1).Mass, "C2H6O4P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 19, 1, 5, 0, 1).Mass, "C8H19O5NP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 21, 1, 6, 0, 1).Mass, "C8H21O6NP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 12, 1, 0, 0, 0).Mass, "C5H12N"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 5, 0, 4, 0, 1, new Tuple<Atom, short>(Atom.Get("Na"), 1)).Mass, "C2H5O4P+Na"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 5, 0, 4, 0, 1, new Tuple<Atom, short>(Atom.Get("K"), 1)).Mass, "C2H5O4P+K"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 3 - 2 * doubleBonds, 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 1 - 2 * doubleBonds, 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) + 2 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPC(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) - 0 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPC(P-)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ether", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 1, 0, 0).Mass - new Composition(3, 9, 1, 0, 0).Mass, "M-Ether-C3H9N", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCO+58]", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "[RCO+58]-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    //msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 4 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(O-)", acylChain));
                                    //msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(O-)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) + 5 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPC(O-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) + 3 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPC(O-)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "[RCO+58]-O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "[RCO]-2H", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "[RCO-H2O]", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 8) - 4 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "loss of ketene-59", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 8) - 6 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "loss of ether chain-59", acylChain));

                                }
                                break;
                            case AcylChainType.OxoCHO:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 14, 1, 4, 0, 1).Mass, "M-C5H14O4NP"));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) - 1 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "loss of oxidized FA"));
                                break;
                            case AcylChainType.OxoCOOH:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 14, 1, 4, 0, 1).Mass, "M-C5H14O4NP"));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) - 1 - 2 * doubleBonds, 1, 8, 0, 1).Mass, "loss of oxidized FA"));
                                break;
                        }
                    }

                    if (countOfStandardAcylsChains == 2)
                    {
                        var carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
                        var doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 4, 0, 0).Mass, "DAG", combinedChain));
                    }
                }
                else if (lipidClass == LipidClass.PE || lipidClass == LipidClass.PE_NMe || lipidClass == LipidClass.PE_NMe2)
                {
                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    if (lipidClass == LipidClass.PE)
                    {
                        var displayC2H8NO4P = countOfChains > 1 ? "M-C2H8NO4P / DAG" : "M-C2H8NO4P";

                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 8, 1, 4, 0, 1).Mass,
                            displayC2H8NO4P, true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
                    }
                    else if (lipidClass == LipidClass.PE_NMe)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 10, 1, 4, 0, 1).Mass, "C3H10O4NP", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 11, 1, 4, 0, 1).Mass, "C3H10O4NP"));
                    }
                    else if (lipidClass == LipidClass.PE_NMe2)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(4, 12, 1, 4, 0, 1).Mass, "C3H10O4NP", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(4, 13, 1, 4, 0, 1).Mass, "C3H10O4NP"));
                    }

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 3 - 2 * doubleBonds, 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 1 - 2 * doubleBonds, 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) - 1 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPE(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) - 3 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPE(P-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 5 - 2 * doubleBonds, 1, 4, 0, 1).Mass, "ether", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 3 - 2 * doubleBonds, 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PS)
                {
                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    var displayC3H8NO6P = countOfChains > 1 ? "M-C3H8NO6P / DAG" : "M-C3H8NO6P";

                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 8, 1, 6, 0, 1).Mass, displayC3H8NO6P, true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 3 - 2 * doubleBonds, 1, 8, 0, 1).Mass, "LPS(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 5 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "LPS(P-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 1 - 2 * doubleBonds, 1, 8, 0, 1).Mass, "ether", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 1 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "ether-H2O", acylChain));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PG)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 12, 1, 6, 0, 1).Mass, "M-C3H12O6NP", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz + new Composition(0, 4, 1, 0, 0, 0).Mass, "M+NH4"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPG-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPG-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 3 - 2 * doubleBonds, 1, 8, 0, 1).Mass, "LPG(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 5 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "LPG(P-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 5 - 2 * doubleBonds, 1, 4, 0, 1).Mass, "ether", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) + 3 - 2 * doubleBonds, 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
                                }
                                break;
                        }
                    }

                    if (countOfStandardAcylsChains == 2)
                    {
                        var carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
                        var doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 4, 0, 0).Mass, "DAG", combinedChain));
                    }
                }
                else if (lipidClass == LipidClass.Cer || lipidClass == LipidClass.CerH2O || lipidClass == LipidClass.Cer2H2O || lipidClass == LipidClass.GlcCer
                    || lipidClass == LipidClass.GalCer || lipidClass == LipidClass.LacCer || lipidClass == LipidClass.CerP || lipidClass == LipidClass.SM
                    || lipidClass == LipidClass.PE_Cer || lipidClass == LipidClass.PI_Cer)
                {
                    if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer || lipidClass == LipidClass.LacCer)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));
                    }

                    if (lipidClass == LipidClass.LacCer) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 22, 0, 11, 0, 0).Mass, "M-2(sugar)"));

                    if (lipidClass == LipidClass.CerP)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "M-H3PO4"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 5, 0, 5, 0, 1).Mass, "M-H3PO4-H2O"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "M-HPO3"));
                    }

                    if (lipidClass == LipidClass.SM)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 14, 1, 1, 0, 0).Mass, "C5H14ON")); //CC addtion 1-16-2015
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 6, 0, 4, 0, 1).Mass, "C2H6O4P")); //CC addition 1-16-2015

                    }

                    if (lipidClass == LipidClass.PE_Cer)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 7, 1, 1, 0).Mass, "M-C2H7NO"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 8, 1, 4, 0, 1).Mass, "M-C2H8NO4P", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 10, 1, 5, 0, 1).Mass, "M-(C2H8NO4P + H2O)"));
                    }

                    if (lipidClass == LipidClass.PI_Cer)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "C6H10O8P"));


                    }

                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
                    if (lipidClass != LipidClass.CerH2O && lipidClass != LipidClass.Cer2H2O) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "M-2(H2O)"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) - 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "FA long", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "FA short", acylChain));
                                break;
                            case AcylChainType.Monohydro:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 2 - 2 * doubleBonds, 1, 0, 0, 0).Mass, "LCB", acylChain, true));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 4 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
                                break;
                            case AcylChainType.Dihydro:
                                if (lipidClass == LipidClass.SM)
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 0 - 2 * doubleBonds, 1, 0, 0, 0).Mass, "LCB", acylChain, false));
                                else
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 0 - 2 * doubleBonds, 1, 0, 0, 0).Mass, "LCB", acylChain, true));

                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * (carbons - 1) + 2 - 2 * doubleBonds, 1, 0, 0, 0).Mass, "LCB-CH2, acylChain"));
                                break;
                            case AcylChainType.Hydroxy:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA+OH", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "FA+NH2", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) - 2 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "FA long", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 2 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "FA short", acylChain));
                                break;
                            case AcylChainType.Trihydro:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 0 - 2 * doubleBonds, 1, 0, 0, 0).Mass, "LCB", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 4 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "LCB+2(H2O), acylChain"));
                                break;
                        }
                    }

                    if (countOfChains == 2)
                    {
                        var carbons = acylChainList.Sum(x => x.NumCarbons);
                        var doubleBonds = acylChainList.Sum(x => x.NumDoubleBonds);

                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);

                        if (lipidClass != LipidClass.Cer && lipidClass != LipidClass.CerH2O && lipidClass != LipidClass.Cer2H2O)
                        {
                            if (lipidClass == LipidClass.SM) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "both chains", combinedChain));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "both chains - H2O", combinedChain));
                        }
                    }
                    else if (countOfChains == 1)
                    {
                        var carbons = acylChainList.First().NumCarbons;
                        var doubleBonds = acylChainList.First().NumDoubleBonds;
                        if (lipidClass == LipidClass.PE_Cer || lipidClass == LipidClass.PI_Cer)
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 39, 1, 5, 0, 1).Mass,
                                "S-1-P (C18H39NO5P)"));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 37, 1, 4, 0, 1).Mass,
                                "S-1-P (-H2O)"));
                            msMsSearchUnitList.Add(
                                new MsMsSearchUnit(
                                    new Composition(carbons, 2 * carbons - 2 * doubleBonds - 3, 0, 0, 0, 0).Mass,
                                    "LCB-NH3"));
                            if (lipidClass == LipidClass.PE_Cer)
                            {
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 9, 1, 4, 0, 1).Mass,
                                    "C2H9NO4P"));
                            }
                        }
                        else if (acylChainList.Count(x => x.AcylChainType == AcylChainType.Monohydro) == 1)
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz, "M+H"));
                        }
                        else
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz, "M+H"));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 6, 0, 2, 0, 0).Mass, "M-H2O-H2CO-CH2"));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(1, 4, 0, 1, 0, 0).Mass, "M-H2O-CH2"));
                            msMsSearchUnitList = msMsSearchUnitList.Where(
                                x => !x.Description.Equals("M-2(H2O)") && !x.Description.Equals("M-H2O")).ToList();
                        }
                    }
                }
                else if (lipidClass == LipidClass.Cholesterol)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45", true));
                }
                else if (lipidClass == LipidClass.CE)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45", true));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.WE)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0).Mass, "M-H2O"));
                    var FA = acylChainList.Last(); //Second chain in WE name is the FA
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(FA.NumCarbons, 2 * FA.NumCarbons + 1 - 2 * FA.NumDoubleBonds, 0, 2, 0).Mass, "FA+H2O / Fatty alcohol"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(FA.NumCarbons, 2 * FA.NumCarbons - 1 - 2 * FA.NumDoubleBonds, 0, 1, 0).Mass, "FA", FA));
                }
                else if (lipidClass == LipidClass.Ubiquinone)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(10, 13, 0, 4, 0).Mass, "C10H13O4"));
                }
                else if (lipidClass == LipidClass.MG || lipidClass == LipidClass.DG || lipidClass == LipidClass.TG)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 1, 0, 0, 0).Mass, "M-NH3"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 5, 1, 1, 0, 0).Mass, "M-NH3-H2O"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));

                        if (lipidClass == LipidClass.DG)
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons + 3 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "M-RCOOH-NH3", acylChain));
                        }
                        else
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons + 3 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "M-RCOOH-NH3", acylChain));
                        }
                    }
                }
                else if (lipidClass == LipidClass.MGDG || lipidClass == LipidClass.SQDG || lipidClass == LipidClass.DGDG)
                {
                    if (lipidClass == LipidClass.MGDG)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 11, 0, 6, 0, 0).Mass, "M-C6H11O6", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 13, 0, 7, 0, 0).Mass, "M-C6H13O7 / DAG"));
                    }
                    else if (lipidClass == LipidClass.SQDG)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 15, 1, 8, 1, 0).Mass, "M-C6H11O8SNH4 / DAG", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 11, 0, 8, 1, 0).Mass, "M-C6H11O8S"));
                    }
                    else if (lipidClass == LipidClass.DGDG)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 21, 0, 11, 0, 0).Mass, "M-C12H21O11", true));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 25, 1, 11, 0, 0).Mass, "M-C12H21O11NH4 / DAG"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));
                    }

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
                    }
                }
                else if (lipidClass == LipidClass.DGTSA)
                {
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(7, 14, 1, 2, 0, 0).Mass, "C7H14O2N"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(7, 14, 1, 3, 0, 0).Mass, "C7H16O3N"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(10, 18, 1, 3, 0, 0).Mass, "C10H18O3N"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(10, 22, 1, 5, 0, 0).Mass, "C10H22O5N", true));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass - new Composition(1, 0, 0, 2, 0).Mass, "M-FA-CO2", acylChain));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass - new Composition(5, 13, 1, 0, 0).Mass, "M-FA-C5H13N", acylChain));
                        //msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * carbons) + 18 - (2 * doubleBonds), 1, 3, 0, 0).Mass, "M-FA-CO2", acylChain));
                        //msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * carbons) + 5 - (2 * doubleBonds), 0, 5, 0, 0).Mass, "M-FA-C5H13N", acylChain));
                    }
                    if (countOfStandardAcylsChains == 2)
                    {
                        var carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
                        var doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);
                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 3 - 2 * doubleBonds, 0, 4, 0, 0).Mass, "DAG", combinedChain));
                    }
                }
                else if (lipidClass == LipidClass.anandamide)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 1, 0).Mass, "M-OH"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0).Mass, "M-H2O"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 3 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-H2O", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.carnitine)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(4, 5, 0, 2, 0).Mass, "C4H5O2", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0).Mass, "M-C3H9N"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "FA", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.FAHFA)
                {
                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 3 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "FA+NH3", acylChain));
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 3 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "FA+NH3", acylChain));
                                break;
                        }
                    }
                }
            }
            else if (fragmentationMode == FragmentationMode.Negative)
            {
                if (lipidClass == LipidClass.PC)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "M-(acetate + methyl)", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(7, 16, 1, 5, 0, 1).Mass, "M-C7H16O5NP"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Hydroxy:
                                if (acylChain.NumCarbons == 20 && acylChain.NumDoubleBonds == 4)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(20, 31, 0, 3, 0, 0).Mass, "HETE", true));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 7, 2 * (carbons + 7) + 1 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "loss of HETE"));
                                    if (acylChain.HydroxyPosition == 5) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 7, 0, 3, 0, 0).Mass, "5-HETE"));
                                    if (acylChain.HydroxyPosition == 8) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 11, 0, 3, 0, 0).Mass, "8-HETE"));
                                    if (acylChain.HydroxyPosition == 9) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 11, 0, 3, 0, 0).Mass, "9-HETE"));
                                    if (acylChain.HydroxyPosition == 11) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(10, 15, 0, 3, 0, 0).Mass, "11-HETE"));
                                    if (acylChain.HydroxyPosition == 12) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(11, 15, 0, 3, 0, 0).Mass, "12-HETE"));
                                    if (acylChain.HydroxyPosition == 15) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(14, 19, 0, 3, 0, 0).Mass, "15-HETE"));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PE)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 11, 1, 5, 0, 1).Mass, "C5H11O5NP", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 13, 1, 6, 0, 1).Mass, "C5H13O6NP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 6, 1, 0, 0, 0).Mass, "M-C2H6N"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 12, 1, 5, 0, 1).Mass, "M-C5H12O5NP"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * carbons - 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-CO2", acylChain)); //CC addition 1-18-2015
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 5) - 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 5) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 1 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPE(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) - 1 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPE(P-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 3 - 2 * doubleBonds, 1, 6, 0, 1).Mass, "LPE(O-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 1 - 2 * doubleBonds, 1, 5, 0, 1).Mass, "LPE(O-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Hydroxy:
                                if (acylChain.NumCarbons == 20 && acylChain.NumDoubleBonds == 4)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(20, 31, 0, 3, 0, 0).Mass, "HETE", true));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, 2 * (carbons + 5) + 1 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "loss of HETE"));
                                    if (acylChain.HydroxyPosition == 5) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 7, 0, 3, 0, 0).Mass, "5-HETE"));
                                    if (acylChain.HydroxyPosition == 8) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 11, 0, 3, 0, 0).Mass, "8-HETE"));
                                    if (acylChain.HydroxyPosition == 9) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 11, 0, 3, 0, 0).Mass, "9-HETE"));
                                    if (acylChain.HydroxyPosition == 11) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(10, 15, 0, 3, 0, 0).Mass, "11-HETE"));
                                    if (acylChain.HydroxyPosition == 12) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(11, 15, 0, 3, 0, 0).Mass, "12-HETE"));
                                    if (acylChain.HydroxyPosition == 15) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(14, 19, 0, 3, 0, 0).Mass, "15-HETE"));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PI)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "C6H10O8P", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 18, 0, 11, 0, 1).Mass, "C9H18O11P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 16, 0, 10, 0, 1).Mass, "C9H16O10P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2H2O-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 0, 0, 3, 0, 1).Mass, "PO3"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    if (countOfChains > 1)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(9, 17, 0, 10, 0, 1).Mass, "M-C9H17O10P"));
                    }

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));

                                if (countOfChains == 1) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H / M-sugar", acylChain));
                                else msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));

                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));

                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 2 - 2 * doubleBonds, 0, 11, 0, 1).Mass, "LPI(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 4 - 2 * doubleBonds, 0, 10, 0, 1).Mass, "LPI(P-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));

                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) + 0 - 2 * doubleBonds, 0, 11, 0, 1).Mass, "LPI(O-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 2 - 2 * doubleBonds, 0, 10, 0, 1).Mass, "LPI(O-)-H2O", acylChain));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PIP || lipidClass == LipidClass.PIP2 || lipidClass == LipidClass.PIP3)
                {
                    if (lipidClass == LipidClass.PIP) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 11, 0, 2).Mass, "C6H11O11P2", true));
                    else if (lipidClass == LipidClass.PIP2) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 14, 0, 3).Mass, "C6H12O14P3", true));
                    else if (lipidClass == LipidClass.PIP3) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 17, 0, 4).Mass, "C6H14O17P4", true));

                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "M-2(H2O)"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "M-H3O4P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "M-HPO3"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 13, 0, 12, 0, 2).Mass, "IP2-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 11, 0, 2).Mass, "IP2-H2O-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 10, 0, 2).Mass, "IP2-2(H2O)-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "IP-H2O-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2(H2O)-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 3, 0, 7, 0, 2).Mass, "H3P2O7"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 1, 0, 6, 0, 2).Mass, "HP2O6"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));

                    if (lipidClass == LipidClass.PIP2 || lipidClass == LipidClass.PIP3)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 15, 0, 3).Mass, "IP3-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 14, 0, 3).Mass, "IP3-H2O-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 13, 0, 3).Mass, "IP3-2(H2O)-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 6, 0, 6, 0, 1).Mass, "IP-3(H2O)-H"));
                    }
                    if (lipidClass == LipidClass.PIP3)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 16, 0, 18, 0, 4).Mass, "IP4-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 17, 0, 4).Mass, "IP4-H2O-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 16, 0, 4).Mass, "IP4-2(H2O)-H"));
                    }

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * (carbons - 1) + 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-CO2", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 3 - 2 * doubleBonds, 0, 14, 0, 2).Mass, "LPIP-H2O-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 5 - 2 * doubleBonds, 0, 13, 0, 2).Mass, "LPIP-2H2O-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));

                                if (lipidClass == LipidClass.PIP)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 1 - 2 * doubleBonds, 0, 15, 0, 2).Mass, "LPI-H", acylChain));
                                }
                                if (lipidClass == LipidClass.PIP3)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 2 - 2 * doubleBonds, 0, 17, 0, 3).Mass, "LPIP2-H2O-H", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, 2 * (carbons + 9) - 4 - 2 * doubleBonds, 0, 16, 0, 3).Mass, "LPIP2-2H2O-H", acylChain));
                                }
                                break;
                        }
                    }

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);

                    if (countOfChains == 2)
                    {
                        var carbons = acylChainList.Sum(x => x.NumCarbons);
                        var doubleBonds = acylChainList.Sum(x => x.NumDoubleBonds);

                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 8, 0, 1).Mass, "PA", combinedChain));
                    }
                }
                else if (lipidClass == LipidClass.PG)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 6, 0, 1).Mass, "C6H10O6P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "M-C3H6O2"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 7, 0, 1).Mass, "C6H12O7P")); //CC addition 1-19-2015

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * carbons - 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-CO2", acylChain)); //CC addition 1-18-2015
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                }
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain)); //CC addition 1-27-2015
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * carbons - 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-CO2", acylChain)); //CC addition 1-27-2015
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 2 - 2 * doubleBonds, 0, 8, 0, 1).Mass, "LPG(O-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPG(O-)-H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain)); //CC addition 1-27-2015
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * carbons - 1 - 2 * doubleBonds, 0, 0, 0, 0).Mass, "FA-CO2", acylChain)); //CC addition 1-27-2015
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 0 - 2 * doubleBonds, 0, 8, 0, 1).Mass, "LPG(P-)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 2 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPG(P-)-H2O", acylChain));
                                }
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PA)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 2, 0, 4, 0, 1).Mass, "PO4H2"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 0, 0, 3, 0, 1).Mass, "PO3"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 8, 0, 6, 0, 1).Mass, "C3H8O6P"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.PS)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 5, 1, 2, 0, 0).Mass, "M-C3H5O2N", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 8, 0, 6, 0, 1).Mass, "C3H8O6P"));

                    var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
                    var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));

                                if (countOfChains == 1)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                }
                                else if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 5, 1, 2, 0, 0).Mass - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain)); //really M-C3H5O2N-Ketene
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 5, 1, 2, 0, 0).Mass - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain)); //really M-C3H5O2N-FA
                                }
                                break;
                            case AcylChainType.Plasmalogen:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
                                break;
                            case AcylChainType.Ether:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));

                                if (countOfChains == 2)
                                {
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 1 - 2 * doubleBonds, 1, 8, 0, 1).Mass, "LPS(O-", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 1 - 2 * doubleBonds, 1, 7, 0, 1).Mass, "LPS(O- - H2O", acylChain));
                                }
                                break;
                            case AcylChainType.Hydroxy:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 31, 0, 3, 0, 0).Mass, "HODE", true));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 29, 0, 2, 0, 0).Mass, "HODE"));
                                if (acylChain.HydroxyPosition == 9) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 15, 0, 3, 0, 0).Mass, "9-HODE"));
                                if (acylChain.HydroxyPosition == 13) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(12, 19, 0, 2, 0, 0).Mass, "13-HODE"));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.Cer)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(1, 2, 0, 1, 0, 0).Mass, "M-CH2O"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(1, 4, 0, 1, 0, 0).Mass, "M-CH3OH"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

                    var containsHydroxy = acylChainList.Count(x => x.AcylChainType == AcylChainType.Hydroxy) > 0;

                    if (containsHydroxy)
                    {
                        foreach (var acylChain in acylChainList)
                        {
                            var carbons = acylChain.NumCarbons;
                            var doubleBonds = acylChain.NumDoubleBonds;

                            // Ignore any 0:0 chains
                            if (carbons == 0 && doubleBonds == 0) continue;

                            switch (acylChain.AcylChainType)
                            {
                                case AcylChainType.Dihydro:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 2 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-LBC (256)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "LBC (237)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 3 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "LBC (263)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * (carbons - 1) + 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "LBC+amine", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-LBC (240)", acylChain));
                                    break;
                                case AcylChainType.Standard:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                    break;
                                case AcylChainType.Hydroxy:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "FA with OH", acylChain));
                                    break;
                                case AcylChainType.Trihydro:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 4 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "M-(LBC+H2O) (274)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 4 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-(LBC+H2O) (258)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) + 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (255)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (281)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * (carbons - 1) + 4 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "LBC+amine+H2O", acylChain));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var acylChain in acylChainList)
                        {
                            var carbons = acylChain.NumCarbons;
                            var doubleBonds = acylChain.NumDoubleBonds;

                            // Ignore any 0:0 chains
                            if (carbons == 0 && doubleBonds == 0) continue;

                            switch (acylChain.AcylChainType)
                            {
                                case AcylChainType.Dihydro:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 1, 2 * (carbons + 1) + 1 - 2 * doubleBonds, 1, 3, 0, 0).Mass, "M-LBC (327)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 2 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-LBC (256)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-LBC (240)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) + 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "LBC (239)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) - 1 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "LBC (237)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 3 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "LBC (263)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, 2 * (carbons - 1) + 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "LBC+amine", acylChain));
                                    break;
                                case AcylChainType.Standard:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                    break;
                                case AcylChainType.Trihydro:
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 1, 2 * (carbons + 1) + 1 - 2 * doubleBonds, 1, 3, 0, 0).Mass, "M-LBC (327)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 4 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "M-(LBC+H2O) (274)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, 2 * (carbons - 2) + 4 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-(LBC+H2O) (258)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) + 3 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "LBC+H2O (257)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, 2 * (carbons - 2) + 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (255)", acylChain));
                                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (281)", acylChain));
                                    break;
                            }
                        }
                    }
                }
                else if (lipidClass == LipidClass.PI_Cer || lipidClass == LipidClass.MIPC || lipidClass == LipidClass.MIP2C)
                {
                    var sumCarbon = acylChainList.Sum(x => x.NumCarbons);
                    var sumDB = acylChainList.Sum(x => x.NumDoubleBonds);

                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "C6H10O8P"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 9, 0, 1).Mass, "IP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2(H2O)-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 5, 0, 0).Mass, "CerP"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 5, 0, 0).Mass - Composition.H2O.Mass, "Cerp-H2O"));

                    if (lipidClass == LipidClass.MIPC || lipidClass == LipidClass.MIP2C)
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(12, 22, 0, 14, 0, 1).Mass, "MIP"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(12, 20, 0, 13, 0, 1).Mass, "MIP-H2O"));
                        if (lipidClass == LipidClass.MIP2C)
                        {
                            msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(18, 33, 0, 21, 0, 2).Mass, "Cer"));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 32, 0, 19, 0, 1).Mass, "MIP(2)-P"));
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(18, 32, 0, 18, 0, 2).Mass / 2, "[M(IP)2]2-")); //Charge = -2
                            msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(sumCarbon + 18, 2 * (sumCarbon + 18) - 3 - sumDB, 1, 26, 0, 2).Mass / 2, "[M-2H]2- - M(IP)2C")); //Charge = -2
                        }
                    }

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                break;
                            case AcylChainType.Trihydro:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons + 1 - doubleBonds, 1, 5, 0, 1).Mass, "LBCP-H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - doubleBonds, 1, 4, 0, 1).Mass, "LBCP-2(H2O)", acylChain));
                                break;
                            case AcylChainType.Hydroxy:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "FA with OH", acylChain));
                                break;
                        }

                    }
                }
                else if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                break;
                            case AcylChainType.Hydroxy:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 3, 0, 0).Mass, "FA with OH", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.Ganglioside)
                {
                    var carbons = (from chain in acylChainList select chain.NumCarbons).Sum();
                    var doubleBonds = (from chain in acylChainList select chain.NumDoubleBonds).Sum();
                    var acylChains = new AcylChain(string.Format("{0}:{1}", carbons, doubleBonds));
                    var sialic = commonName.Split('(')[0][1];
                    var sugar = commonName.Split('(')[0][0];

                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 2 * doubleBonds, 1, 3, 0).Mass, "Cer", acylChains));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 1, 2, 0).Mass, "Cer-H2O", acylChains));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 2 - 2 * doubleBonds, 1, 8, 0).Mass, "HexCer", acylChains));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 12, 2 * (carbons + 12) - 4 - 2 * doubleBonds, 1, 13, 0).Mass, "2(Hex)Cer", acylChains));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 4 - 2 * doubleBonds, 1, 7, 0).Mass, "HexCer-H2O", acylChains));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 4, 0, 2, 0).Mass, "M-C2H4O2 (cross ring cleavage)"));

                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(11, 16, 1, 8, 0).Mass, "NAc-H"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(11, 17, 1, 8, 0).Mass, "M-NAc-H")); //Adduct is M-H
                    msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(11, 16, 1, 8, 0).Mass, "M-NAc-H")); //Adduct is M-2H
                    if (sialic == 'D' || sialic == 'T' || sialic == 'Q')
                    {
                        msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(22, 33, 2, 16, 0).Mass, "M-2NAc-H"));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(22, 33, 2, 16, 0).Mass, "2NAc-H"));
                    }
                    if (sialic == 'T' || sialic == 'Q') msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(33, 50, 3, 24, 0).Mass, "M-3NAc-H"));
                    if (sialic == 'Q') msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(44, 67, 4, 32, 0).Mass, "M-4NAc-H"));
                }
                else if (lipidClass == LipidClass.Sulfatide)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 1, 0, 4, 1, 0).Mass, "HO4S", true));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 9, 1, 0).Mass, "sulfogalactosyl"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 9, 1, 0).Mass, "sulfogalactosyl-H2"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 8, 1, 0).Mass, "sulfogalactosyl-H20"));
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 14, 1, 9, 1, 0).Mass, "sulfogalactosyl+amine"));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) - 2 - 2 * doubleBonds, 1, 1, 0, 0).Mass, "FA with amide", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) - 2 - 2 * doubleBonds, 1, 10, 1, 0).Mass, "lysoSulfogalactosyl with FA", acylChain));
                                break;
                            case AcylChainType.Dihydro:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) + 0 - 2 * doubleBonds, 1, 10, 1, 0).Mass, "lysoSulfogalactosyl with LBC", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, 2 * (carbons + 6) - 2 - 2 * doubleBonds, 1, 9, 1, 0).Mass, "lysoSulfogalactosyl with LBC - H20", acylChain));
                                break;
                            case AcylChainType.Hydroxy:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, 2 * (carbons + 2) - 2 - 2 * doubleBonds, 1, 2, 0, 0).Mass, "FA with OH", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, 2 * (carbons + 8) - 2 - 2 * doubleBonds, 1, 11, 1, 0).Mass, "lysoSulfogalactosyl with OH", acylChain));
                                break;
                        }
                    }
                }
                else if (lipidClass == LipidClass.CL)
                {
                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 1 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "FA+C3H6PO4", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 1 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "FA+C3H6PO4+H2O", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons + 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                break;
                        }
                    }

                    var acylChainsAsList = acylChainList.ToList();

                    if (acylChainsAsList.Count == 4)
                    {
                        var carbonsOfFirstTwoAcylChains = acylChainsAsList[0].NumCarbons + acylChainsAsList[1].NumCarbons;
                        var doubleBondsOfFirstTwoAcylChains = acylChainsAsList[0].NumDoubleBonds + acylChainsAsList[1].NumDoubleBonds;
                        var carbonsOfSecondTwoAcylChains = acylChainsAsList[2].NumCarbons + acylChainsAsList[3].NumCarbons;
                        var doubleBondsOfSecondTwoAcylChains = acylChainsAsList[2].NumDoubleBonds + acylChainsAsList[3].NumDoubleBonds;

                        var firstTwoAcylChains = new AcylChain(carbonsOfFirstTwoAcylChains + ":" + doubleBondsOfFirstTwoAcylChains);
                        var secondTwoAcylChains = new AcylChain(carbonsOfSecondTwoAcylChains + ":" + doubleBondsOfSecondTwoAcylChains);

                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 3, 2 * (carbonsOfFirstTwoAcylChains + 3) - 2 - 2 * doubleBondsOfFirstTwoAcylChains, 0, 8, 0, 1).Mass, "PA", firstTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 3, 2 * (carbonsOfSecondTwoAcylChains + 3) - 2 - 2 * doubleBondsOfSecondTwoAcylChains, 0, 8, 0, 1).Mass, "PA", secondTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, 2 * (carbonsOfFirstTwoAcylChains + 6) - 2 - 2 * doubleBondsOfFirstTwoAcylChains, 0, 10, 0, 1).Mass, "PG", firstTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, 2 * (carbonsOfSecondTwoAcylChains + 6) - 2 - 2 * doubleBondsOfSecondTwoAcylChains, 0, 10, 0, 1).Mass, "PG", secondTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, 2 * (carbonsOfFirstTwoAcylChains + 6) - 4 - 2 * doubleBondsOfFirstTwoAcylChains, 0, 9, 0, 1).Mass, "PG-H2O", firstTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, 2 * (carbonsOfSecondTwoAcylChains + 6) - 4 - 2 * doubleBondsOfSecondTwoAcylChains, 0, 9, 0, 1).Mass, "PG-H2O", secondTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, 2 * (carbonsOfFirstTwoAcylChains + 6) - 1 - 2 * doubleBondsOfFirstTwoAcylChains, 0, 13, 0, 2).Mass, "PG+PO3", firstTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, 2 * (carbonsOfSecondTwoAcylChains + 6) - 1 - 2 * doubleBondsOfSecondTwoAcylChains, 0, 13, 0, 2).Mass, "PG+PO3", secondTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, 2 * (carbonsOfFirstTwoAcylChains + 6) - 1 - 2 * doubleBondsOfFirstTwoAcylChains, 0, 12, 0, 2).Mass, "PG+PO2", firstTwoAcylChains));
                        msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, 2 * (carbonsOfSecondTwoAcylChains + 6) - 1 - 2 * doubleBondsOfSecondTwoAcylChains, 0, 12, 0, 2).Mass, "PG+PO2", secondTwoAcylChains));
                    }
                }
                else if (lipidClass == LipidClass.SQDG)
                {
                    msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 7, 1, 0).Mass, "C6H9O7S", true));

                    foreach (var acylChain in acylChainList)
                    {
                        var carbons = acylChain.NumCarbons;
                        var doubleBonds = acylChain.NumDoubleBonds;

                        // Ignore any 0:0 chains
                        if (carbons == 0 && doubleBonds == 0) continue;

                        switch (acylChain.AcylChainType)
                        {
                            case AcylChainType.Standard:
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, 2 * carbons - 1 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 2 - 2 * doubleBonds, 0, 1, 0, 0).Mass, "M-Ketene", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, 2 * carbons - 0 - 2 * doubleBonds, 0, 2, 0, 0).Mass, "M-FA", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) + 0 - 2 * doubleBonds, 0, 7, 0, 1).Mass, "LPA-H", acylChain));
                                msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, 2 * (carbons + 3) - 2 - 2 * doubleBonds, 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));

                                break;
                        }
                    }
                }
            }

            return msMsSearchUnitList;
        }

        /// <summary>
        /// Finds all isotope peaks corresponding to theoretical profiles with relative intensity higher than the threshold
        /// </summary>
        /// <param name="spectrum">Observed spectrum.</param>
        /// <param name="composition">Composition to calculate theoretical isotopic profile for.</param>
        /// <param name="tolerance">Peak ppm tolerance.</param>
        /// <param name="relativeIntensityThreshold"></param>
        /// <returns>array of observed isotope peaks in the spectrum. null if no peak found.</returns>
        public static Peak[] GetAllIsotopePeaks(Spectrum spectrum, Composition composition, Tolerance tolerance, double relativeIntensityThreshold)
        {
            var peaks = spectrum.Peaks;
            var mostAbundantIsotopeIndex = 0;
            var isotopomerEnvelope = IsoProfilePredictor.GetIsotopomerEnvelop(
                                                            composition.C,
                                                            composition.H,
                                                            composition.N,
                                                            composition.O,
                                                            composition.S).Envolope;
            var mostAbundantIsotopeMz = composition.Mass;
            var mostAbundantIsotopeMatchedPeakIndex = spectrum.FindPeakIndex(mostAbundantIsotopeMz, tolerance);
            if (mostAbundantIsotopeMatchedPeakIndex < 0) return null;

            var observedPeaks = new Peak[isotopomerEnvelope.Length];
            observedPeaks[mostAbundantIsotopeIndex] = peaks[mostAbundantIsotopeMatchedPeakIndex];

            // go down
            var peakIndex = mostAbundantIsotopeMatchedPeakIndex - 1;
            for (var isotopeIndex = mostAbundantIsotopeIndex - 1; isotopeIndex >= 0; isotopeIndex--)
            {
                if (isotopomerEnvelope[isotopeIndex] < relativeIntensityThreshold) break;
                var isotopeMz = mostAbundantIsotopeMz - isotopeIndex * Constants.C13MinusC12;
                var tolTh = tolerance.GetToleranceAsTh(isotopeMz);
                var minMz = isotopeMz - tolTh;
                var maxMz = isotopeMz + tolTh;
                for (var i = peakIndex; i >= 0; i--)
                {
                    var peakMz = peaks[i].Mz;
                    if (peakMz < minMz)
                    {
                        peakIndex = i;
                        break;
                    }
                    if (peakMz <= maxMz)    // find match, move to prev isotope
                    {
                        var peak = peaks[i];
                        if (observedPeaks[isotopeIndex] == null ||
                            peak.Intensity > observedPeaks[isotopeIndex].Intensity)
                        {
                            observedPeaks[isotopeIndex] = peak;
                        }
                    }
                }
            }

            // go up
            peakIndex = mostAbundantIsotopeMatchedPeakIndex + 1;
            for (var isotopeIndex = mostAbundantIsotopeIndex + 1; isotopeIndex < isotopomerEnvelope.Length; isotopeIndex++)
            {
                if (isotopomerEnvelope[isotopeIndex] < relativeIntensityThreshold) break;
                var isotopeMz = mostAbundantIsotopeMz + isotopeIndex * Constants.C13MinusC12;
                var tolTh = tolerance.GetToleranceAsTh(isotopeMz);
                var minMz = isotopeMz - tolTh;
                var maxMz = isotopeMz + tolTh;
                for (var i = peakIndex; i < peaks.Length; i++)
                {
                    var peakMz = peaks[i].Mz;
                    if (peakMz > maxMz)
                    {
                        peakIndex = i;
                        break;
                    }
                    if (peakMz >= minMz)    // find match, move to prev isotope
                    {
                        var peak = peaks[i];
                        if (observedPeaks[isotopeIndex] == null ||
                            peak.Intensity > observedPeaks[isotopeIndex].Intensity)
                        {
                            observedPeaks[isotopeIndex] = peak;
                        }
                    }
                }
            }

            return observedPeaks;
        }

        /// <summary>
        /// Calculates the PPM error between two values.
        /// </summary>
        /// <param name="num1">Expected value.</param>
        /// <param name="num2">Observed value.</param>
        /// <returns>PPM error between expected and observed value.</returns>
        public static double PpmError(double num1, double num2)
        {
            // (X - Y) / X * 1,000,000
            return (num2 - num1) / num2 * 1000000;
        }

        public static int IonCharge(Adduct adduct)
        {
            if (adduct == Adduct.Acetate || adduct == Adduct.Ammonium || adduct == Adduct.Hydrogen || adduct == Adduct.Sodium || adduct == Adduct.Potassium)
            {
                return 1;
            }
            if (adduct == Adduct.Dihydrogen)
            {
                return 2;
            }
            return 0;
        }
    }
}
