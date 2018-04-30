using InformedProteomics.Backend.Data.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public int ContainsEther { get; set; }
		public int ContainsDiether { get; set; }
		public int ContainsPlasmalogen { get; set; }
		public int ContainsLCB { get; set; }
		public int ContainsLCBOH { get; set; }
		public int ContainsOH { get; set; }
		public int ContainsDeoxy { get; set; }
		public bool IsOxoCHO { get; set; }
		public bool IsOxoCOOH { get; set; }

		public override string ToString()
		{
			StringBuilder compRule = new StringBuilder();
			compRule.Append(LipidClass + "\t");
			compRule.Append(LipidSubClass + "\t");
			compRule.Append(Category + "\t");
			compRule.Append(MainClass + "\t");
			compRule.Append(SubClass + "\t");
			compRule.Append(C.GetEquationString() + "\t");
			compRule.Append(H.GetEquationString() + "\t");
			compRule.Append(N.GetEquationString() + "\t");
			compRule.Append(O.GetEquationString() + "\t");
			compRule.Append(S.GetEquationString() + "\t");
			compRule.Append(P.GetEquationString() + "\t");
			compRule.Append(Example + "\t");
			compRule.Append(Formula + "\t");
			compRule.Append(IonizationMode + "\t");
			compRule.Append(NumChains);
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
