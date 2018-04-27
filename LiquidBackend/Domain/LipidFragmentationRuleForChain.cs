using System;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRuleForChain : ILipidFragmentationRule
    {
        public string lpidClass { get; set; }
        public FragmentationMode fragmentationMode { get; set; }
        public bool isNeutralLoss { get; set; }
        public string description { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }

        public bool diagnastic { get; set; }

        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            return new Composition(this.C.Evaluate(numCarbons, numDoubleBonds),
                                   this.H.Evaluate(numCarbons, numDoubleBonds),
                                   this.N.Evaluate(numCarbons, numDoubleBonds),
                                   this.O.Evaluate(numCarbons, numDoubleBonds),
                                   this.S.Evaluate(numCarbons, numDoubleBonds),
                                   this.P.Evaluate(numCarbons, numDoubleBonds));
        }
    }
}
