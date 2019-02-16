using System;
using System.Collections.Generic;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRule
    {
        public string lipidClass { get; set; }
        public FragmentationMode fragmentationMode { get; set; }
        public bool isNeutralLoss { get; set; }
        public string description { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }
        public string additionalElement { get; set; }

        public ConditionForInteger conditionForCountOfChains { get; set; }
        public ConditionForInteger conditionForCountOfStandardAcylsChains { get; set; }
        public ConditionForInteger conditionForContainsHydroxy { get; set; }

        public string sialic { get; set; }
        public string acylChainType { get; set; }
        public int acylChainNumCarbons { get; set; }
        public int acylChainNumDoubleBonds { get; set; }
        public int acylChainHydroxyPosition { get; set; }

        public List<Int32> targetAcylChainsIndices { get; set; }

        public bool diagnostic { get; set; }

        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            if (this.additionalElement == null)
            {
                return new Composition(this.C.Evaluate(numCarbons, numDoubleBonds),
                                       this.H.Evaluate(numCarbons, numDoubleBonds),
                                       this.N.Evaluate(numCarbons, numDoubleBonds),
                                       this.O.Evaluate(numCarbons, numDoubleBonds),
                                       this.S.Evaluate(numCarbons, numDoubleBonds),
                                       this.P.Evaluate(numCarbons, numDoubleBonds));
            }
            else
            {
                return new Composition(this.C.Evaluate(numCarbons, numDoubleBonds),
                                       this.H.Evaluate(numCarbons, numDoubleBonds),
                                       this.N.Evaluate(numCarbons, numDoubleBonds),
                                       this.O.Evaluate(numCarbons, numDoubleBonds),
                                       this.S.Evaluate(numCarbons, numDoubleBonds),
                                       this.P.Evaluate(numCarbons, numDoubleBonds),
                                       new Tuple<Atom, short>(Atom.Get(this.additionalElement), 1));
            }
        }

        public bool isFromHeader { get; set; }
        public bool isSpecialCase { get; set; }
        public bool checkFromHeader()
        {
            return (this.acylChainType == null)
                && (this.sialic == null) 
                && (this.targetAcylChainsIndices == null) 
                && (this.conditionForCountOfStandardAcylsChains == null);
        }
        //public bool checkSpecialCase()
        //{
        //    return (!this.isFromHeader && this.acylChainType == null);
        //}

        public bool checkAcylChainConditions(string acylChainType,
                                             int acylChainNumCarbons,
                                             int acylChainNumDoubleBonds,
                                             int acylChainHydroxyPosition,
                                             int countOfChains,
                                             int containsHydroxy,
                                             char sialic)
        {
            if (this.acylChainType == null) return false;
            if (this.acylChainType != null) { if (!this.acylChainType.Equals("All") && !this.acylChainType.Equals(acylChainType)) return false; }
            if (this.acylChainNumCarbons != 0) { if (this.acylChainNumCarbons != acylChainNumCarbons) return false; }
            if (this.acylChainNumDoubleBonds != 0) { if (this.acylChainNumDoubleBonds != acylChainNumDoubleBonds) return false; }
            if (this.acylChainHydroxyPosition != 0) { if (this.acylChainHydroxyPosition != acylChainHydroxyPosition) return false; }
            if (this.conditionForCountOfChains != null) { if (!this.conditionForCountOfChains.meet(countOfChains)) return false; }
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains)) return false; }
            if (this.conditionForContainsHydroxy != null) { if (!this.conditionForContainsHydroxy.meet(containsHydroxy)) return false; }
            if (this.sialic != null) { if (this.sialic.IndexOf(sialic) == -1) return false; }

            return true;
        }

        public bool useCountOfStandardAcylsChains(int countOfStandardAcylsChains)
        {
            //if (this.isFromHeader) return false;
            //if (this.acylChainType != null) return false;
            //if (this.targetAcylChainsIndices != null) return false;
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains)) return false; }
            //return true;
            if (this.conditionForCountOfStandardAcylsChains == null) return false;
            else if (this.conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains)) return true;
            return false;
        }

        public bool checkCountOfChains(int countOfChains)
        {
            if (this.conditionForCountOfChains == null) return true;
            else if (this.conditionForCountOfChains.meet(countOfChains)) return true; 
            return false;
        }

        public bool checkTargetAcylsChains()
        {
            if (this.targetAcylChainsIndices.Count > 0) return true;
            return false;
        }

        public MsMsSearchUnit GetMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null)
        {
            if (this.isNeutralLoss)
            {
                return new MsMsSearchUnit(precursorMz - GetComposition(numCarbons, numDoubleBonds).Mass, description, acylChain, diagnostic);

            }

            return new MsMsSearchUnit(GetComposition(numCarbons, numDoubleBonds).Mass, description, acylChain, diagnostic);
        }

        public override string ToString()
        {
            return "lipidClass: " + lipidClass + "\t"
                                      + "isNeutralLoss:" + isNeutralLoss + "\t"
                                      + "fragmentationMode:" + fragmentationMode + "\t"
                                      + "diagnostic:" + diagnostic + "\t"
                                      + "isFromHeader:" + isFromHeader + "\t"
                                      + "acylChainType:" + acylChainType;
        }

        //Composition ILipidFragmentationRule.GetComposition(int numCarbons, int numDoubleBonds)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
