using System;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRule
    {
        public string LipidClass { get; set; }
        public string FragmentationMode { get; set; }
        public string LossType { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }
        public string Other { get; set; }
        public Composition getComposition(int numCarbons, int numDoubleBonds)
        {
            return new Composition(this.C.Evaluate(numCarbons, numDoubleBonds),
                                   this.H.Evaluate(numCarbons, numDoubleBonds),
                                   this.N.Evaluate(numCarbons, numDoubleBonds),
                                   this.O.Evaluate(numCarbons, numDoubleBonds),
                                   this.S.Evaluate(numCarbons, numDoubleBonds),
                                   this.P.Evaluate(numCarbons, numDoubleBonds));
        }
        public MsMsSearchUnit getMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null, bool isDiagnostic = false)
        {
            if (this.LossType.Equals("PI"))
            {
                return new MsMsSearchUnit(this.getComposition(numCarbons, numDoubleBonds).Mass, this.Description1, acylChain, isDiagnostic);
            }
            else if (this.LossType.Equals("NL"))
            {
                return new MsMsSearchUnit(precursorMz - this.getComposition(numCarbons, numDoubleBonds).Mass, "M-" + this.Description1, acylChain, isDiagnostic);
            }
            else
            {
                return null;    
            }

        }
    }
}