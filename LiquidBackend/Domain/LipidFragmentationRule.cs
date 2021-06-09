using System;
using System.Collections.Generic;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRule
    {
        public string LipidClass { get; set; }
        public FragmentationMode FragmentationMode { get; set; }
        public bool IsNeutralLoss { get; set; }
        public string Description { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }
        public string AdditionalElement { get; set; }

        public ConditionForInteger ConditionForCountOfChains { get; set; }
        public ConditionForInteger ConditionForCountOfStandardAcylsChains { get; set; }
        public ConditionForInteger ConditionForContainsHydroxy { get; set; }

        public string Sialic { get; set; }
        public string AcylChainType { get; set; }
        public int AcylChainNumCarbons { get; set; }
        public int AcylChainNumDoubleBonds { get; set; }
        public int AcylChainHydroxyPosition { get; set; }

        public List<int> TargetAcylChainsIndices { get; set; }

        public bool Diagnostic { get; set; }

        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            if (AdditionalElement == null)
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
                                   new Tuple<Atom, short>(Atom.Get(AdditionalElement), 1));
        }

        public bool IsFromHeader { get; set; }

        public bool CheckFromHeader()
        {
            return AcylChainType == null
                && Sialic == null
                && TargetAcylChainsIndices == null
                && ConditionForCountOfStandardAcylsChains == null;
        }

        // public boolean checkSpecialCase()
        // {
        //     return (!this.isFromHeader && this.acylChainType == null);
        // }

        public bool CheckAcylChainConditions(string acylChainTypeCompare,
                                             int acylChainNumCarbonsCompare,
                                             int acylChainNumDoubleBondsCompare,
                                             int acylChainHydroxyPositionCompare,
                                             int countOfChains,
                                             int containsHydroxy,
                                             char sialicCompare)
        {
            if (AcylChainType == null) return false;
            if (!AcylChainType.Equals("All") && !AcylChainType.Equals(acylChainTypeCompare)) return false;
            if (AcylChainNumCarbons != 0 && AcylChainNumCarbons != acylChainNumCarbonsCompare) { return false; }
            if (AcylChainNumDoubleBonds != 0 && AcylChainNumDoubleBonds != acylChainNumDoubleBondsCompare) { return false; }
            if (AcylChainHydroxyPosition != 0 && AcylChainHydroxyPosition != acylChainHydroxyPositionCompare) { return false; }
            if (ConditionForCountOfChains?.Meet(countOfChains) == false) { return false; }
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.Meet(countOfStandardAcylsChains)) return false; }
            if (ConditionForContainsHydroxy?.Meet(containsHydroxy) == false) { return false; }
            if (Sialic?.IndexOf(sialicCompare) == -1) { return false; }

            return true;
        }

        public bool UseCountOfStandardAcylsChains(int countOfStandardAcylsChains)
        {
            //if (this.isFromHeader) return false;
            //if (this.acylChainType != null) return false;
            //if (this.targetAcylChainsIndices != null) return false;
            //if (this.conditionForCountOfStandardAcylsChains != null) { if (!this.conditionForCountOfStandardAcylsChains.Meet(countOfStandardAcylsChains)) return false; }
            //return true;
            if (ConditionForCountOfStandardAcylsChains == null) return false;
            return ConditionForCountOfStandardAcylsChains.Meet(countOfStandardAcylsChains);
        }

        public bool CheckCountOfChains(int countOfChains)
        {
            if (ConditionForCountOfChains == null) return true;
            return ConditionForCountOfChains.Meet(countOfChains);
        }

        public bool CheckTargetAcylsChains()
        {
            return TargetAcylChainsIndices.Count > 0;
        }

        public MsMsSearchUnit GetMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null)
        {
            if (IsNeutralLoss)
            {
                return new MsMsSearchUnit(precursorMz - GetComposition(numCarbons, numDoubleBonds).Mass, Description, acylChain, Diagnostic);
            }

            return new MsMsSearchUnit(GetComposition(numCarbons, numDoubleBonds).Mass, Description, acylChain, Diagnostic);
        }

        public override string ToString()
        {
            return "lipidClass: " + LipidClass + "\t"
                   + "isNeutralLoss:" + IsNeutralLoss + "\t"
                   + "fragmentationMode:" + FragmentationMode + "\t"
                   + "diagnostic:" + Diagnostic + "\t"
                   + "isFromHeader:" + IsFromHeader + "\t"
                   + "acylChainType:" + AcylChainType;
        }

        // Composition ILipidFragmentationRule.GetComposition(int numCarbons, int numDoubleBonds)
        // {
        //     throw new NotImplementedException();
        // }
    }
}
