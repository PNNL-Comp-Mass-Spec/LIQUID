using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRuleFromTable
    {
        public string lipidSubClass { get; set; }
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
        public bool diagnostic { get; set; }
        public bool headerGroup { get; set; }
        public Composition GetComposition(int numCarbons, int numDoubleBonds)
        {
            return new Composition(C.Evaluate(numCarbons, numDoubleBonds),
                                   H.Evaluate(numCarbons, numDoubleBonds),
                                   N.Evaluate(numCarbons, numDoubleBonds),
                                   O.Evaluate(numCarbons, numDoubleBonds),
                                   S.Evaluate(numCarbons, numDoubleBonds),
                                   P.Evaluate(numCarbons, numDoubleBonds));
        }

        public MsMsSearchUnit GetMsMsSearchUnit(double precursorMz, int numCarbons = 0, int numDoubleBonds = 0, AcylChain acylChain = null)
        {
            if (lossType.Equals("PI"))
            {
                return new MsMsSearchUnit(GetComposition(numCarbons, numDoubleBonds).Mass, description1, acylChain, diagnostic);
            }

            if (lossType.Equals("NL"))
            {
                return new MsMsSearchUnit(precursorMz - GetComposition(numCarbons, numDoubleBonds).Mass, "M-" + description1, acylChain, diagnostic);
            }

            return null;
        }

        public override string ToString()
        {
            return lipidSubClass + "\t" +
                   lossType + "\t" +
                   fragmentationMode + "\t" +
                   diagnostic + "\t" +
                   headerGroup;
        }
    }
}