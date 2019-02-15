using System;
using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRuleFromTable
    {
        public string lpidSubClass { get; set; }
        public FragmentationMode fragmentationMode { get; set; }
        public string lossType { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }
        public string other { get; set; }
        public bool diagnastic { get; set; }
        public bool headerGroup { get; set; }
        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            return new Composition(this.C.Evaluate(numCarbons, numDoubleBonds),
                                   this.H.Evaluate(numCarbons, numDoubleBonds),
                                   this.N.Evaluate(numCarbons, numDoubleBonds),
                                   this.O.Evaluate(numCarbons, numDoubleBonds),
                                   this.S.Evaluate(numCarbons, numDoubleBonds),
                                   this.P.Evaluate(numCarbons, numDoubleBonds));
        }

        public MsMsSearchUnit GetMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null)
        {
            if (this.lossType.Equals("PI"))
            {
                return new MsMsSearchUnit(this.GetComposition(numCarbons, numDoubleBonds).Mass, this.description1, acylChain, this.diagnastic);
            }
            else if (this.lossType.Equals("NL"))
            {
                return new MsMsSearchUnit(precursorMz - this.GetComposition(numCarbons, numDoubleBonds).Mass, "M-" + this.description1, acylChain, this.diagnastic);
            }
            else
            {
                return null;    
            }

        }

        public override string ToString()
        {
            return this.lpidSubClass + "\t" 
                       + this.lossType + "\t" 
                       + this.fragmentationMode + "\t"
                       + this.diagnastic + "\t"
                       + this.headerGroup;
        }
    }
}