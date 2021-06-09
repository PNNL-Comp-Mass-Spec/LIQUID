using InformedProteomics.Backend.Data.Composition;

namespace LiquidBackend.Domain
{
    public class LipidFragmentationRuleFromTable
    {
        public string LipidSubClass { get; set; }
        public FragmentationMode FragmentationMode { get; set; }
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
        public bool Diagnostic { get; set; }
        public bool HeaderGroup { get; set; }
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
            if (LossType.Equals("PI"))
            {
                return new MsMsSearchUnit(GetComposition(numCarbons, numDoubleBonds).Mass, Description1, acylChain, Diagnostic);
            }

            if (LossType.Equals("NL"))
            {
                return new MsMsSearchUnit(precursorMz - GetComposition(numCarbons, numDoubleBonds).Mass, "M-" + Description1, acylChain, Diagnostic);
            }

            return null;
        }

        public override string ToString()
        {
            return LipidSubClass + "\t" +
                   LossType + "\t" +
                   FragmentationMode + "\t" +
                   Diagnostic + "\t" +
                   HeaderGroup;
        }
    }
}