using System;
using System.Collections.Generic;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
#pragma warning disable IDE1006 // Naming Styles

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

        public List<int> targetAcylChainsIndices { get; set; }

        public bool diagnostic { get; set; }

#pragma warning restore IDE1006 // Naming Styles

        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            if (additionalElement == null)
            {
                return new Composition(C.Evaluate(numCarbons, numDoubleBonds),
                                       H.Evaluate(numCarbons, numDoubleBonds),
                                       N.Evaluate(numCarbons, numDoubleBonds),
                                       O.Evaluate(numCarbons, numDoubleBonds),
                                       S.Evaluate(numCarbons, numDoubleBonds),
                                       P.Evaluate(numCarbons, numDoubleBonds));
            }

            return new Composition(C.Evaluate(numCarbons, numDoubleBonds),
                                   H.Evaluate(numCarbons, numDoubleBonds),
                                   N.Evaluate(numCarbons, numDoubleBonds),
                                   O.Evaluate(numCarbons, numDoubleBonds),
                                   S.Evaluate(numCarbons, numDoubleBonds),
                                   P.Evaluate(numCarbons, numDoubleBonds),
                                   new Tuple<Atom, short>(Atom.Get(additionalElement), 1));
        }

        public bool isFromHeader { get; set; }
        public bool checkFromHeader()
        {
            return (acylChainType == null)
                && (sialic == null)
                && (targetAcylChainsIndices == null)
                && (conditionForCountOfStandardAcylsChains == null);
        }
        //public bool checkSpecialCase()
        //{
        //    return (!this.isFromHeader && this.acylChainType == null);
        //}

        public bool checkAcylChainConditions(string acylChainTypeCompare,
                                             int acylChainNumCarbonsCompare,
                                             int acylChainNumDoubleBondsCompare,
                                             int acylChainHydroxyPositionCompare,
                                             int countOfChains,
                                             int containsHydroxy,
                                             char sialicCompare)
        {
            if (acylChainType == null) return false;
            if (!acylChainType.Equals("All") && !acylChainType.Equals(acylChainTypeCompare)) return false;
            if (acylChainNumCarbons != 0 && acylChainNumCarbons != acylChainNumCarbonsCompare) { return false; }
            if (acylChainNumDoubleBonds != 0 && acylChainNumDoubleBonds != acylChainNumDoubleBondsCompare) { return false; }
            if (acylChainHydroxyPosition != 0 && acylChainHydroxyPosition != acylChainHydroxyPositionCompare) { return false; }
            if (conditionForCountOfChains?.meet(countOfChains) == false) { return false; }
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains)) return false; }
            if (conditionForContainsHydroxy?.meet(containsHydroxy) == false) { return false; }
            if (sialic?.IndexOf(sialicCompare) == -1) { return false; }

            return true;
        }

        public bool useCountOfStandardAcylsChains(int countOfStandardAcylsChains)
        {
            //if (this.isFromHeader) return false;
            //if (this.acylChainType != null) return false;
            //if (this.targetAcylChainsIndices != null) return false;
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains)) return false; }
            //return true;
            if (conditionForCountOfStandardAcylsChains == null) return false;
            return conditionForCountOfStandardAcylsChains.meet(countOfStandardAcylsChains);
        }

        public bool checkCountOfChains(int countOfChains)
        {
            if (conditionForCountOfChains == null) return true;
            return conditionForCountOfChains.meet(countOfChains);
        }

        public bool checkTargetAcylsChains()
        {
            return targetAcylChainsIndices.Count > 0;
        }

        public MsMsSearchUnit GetMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null)
        {
            if (isNeutralLoss)
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
