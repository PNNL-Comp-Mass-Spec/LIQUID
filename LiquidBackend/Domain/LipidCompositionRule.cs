using InformedProteomics.Backend.Data.Composition;
using System.Text;

namespace LiquidBackend.Domain
{
    public class LipidCompositionRule
    {
        public string LipidClass { get; set; }
        public string LipidSubClass { get; set; }
        public string Category { get; set; }
        public string MainClass { get; set; }
        public string SubClass { get; set; }
        public CompositionFormula C { get; set; }
        public CompositionFormula H { get; set; }
        public CompositionFormula N { get; set; }
        public CompositionFormula O { get; set; }
        public CompositionFormula S { get; set; }
        public CompositionFormula P { get; set; }
        public string Example { get; set; }
        public string Formula { get; set; }
        public string IonizationMode { get; set; }
        public int NumChains { get; set; }
        public bool ContainsEther { get; set; }
        public bool ContainsDiether { get; set; }
        public bool ContainsPlasmalogen { get; set; }
        public bool ContainsLCB { get; set; }
        public bool ContainsLCBPlusOH { get; set; }
        public bool ContainsLCBMinusOH { get; set; }
        public bool IsOxoCHO { get; set; }
        public bool IsOxoCOOH { get; set; }
        public int NumOH { get; set; }
        public bool ContainsOOH { get; set; }
        public bool ContainsF2IsoP { get; set; }

        public override string ToString()
        {
            var compRule = new StringBuilder();
            compRule.AppendFormat("{0}\t", LipidClass);
            compRule.AppendFormat("{0}\t", LipidSubClass);
            compRule.AppendFormat("{0}\t", Category);
            compRule.AppendFormat("{0}\t", MainClass);
            compRule.AppendFormat("{0}\t", SubClass);
            compRule.AppendFormat("{0}\t", C.GetEquationString());
            compRule.AppendFormat("{0}\t", H.GetEquationString());
            compRule.AppendFormat("{0}\t", N.GetEquationString());
            compRule.AppendFormat("{0}\t", O.GetEquationString());
            compRule.AppendFormat("{0}\t", S.GetEquationString());
            compRule.AppendFormat("{0}\t", P.GetEquationString());
            compRule.AppendFormat("{0}\t", Example);
            compRule.AppendFormat("{0}\t", Formula);
            compRule.AppendFormat("{0}\t", IonizationMode);
            compRule.AppendFormat("{0}", NumChains);

            return compRule.ToString();
        }

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
