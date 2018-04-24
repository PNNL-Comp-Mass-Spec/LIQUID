using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
	public class CompositionFormula
	{
		private const string formulaRegex = @"[\-]?[0-9]+[YXyx]?";
		private const string singleVarRegex = @"[^\-]?[^0-9]?[XYxy]{1}";
		private const string numberRegex = @"[\-]?[0-9]+";

		public int CarbonMult { get; private set; }
		public int DoubleBondMult { get; private set; }
		public int Constant { get; private set; }

		public CompositionFormula(string formula)
		{
			CarbonMult = 0;
			DoubleBondMult = 0;
			Constant = 0;

			MatchCollection singleVars = Regex.Matches(formula, singleVarRegex);
			if (singleVars != null)
			{
				foreach (Match part in singleVars)
				{
					if (part.Success)
					{
						bool negative = false;
						if (part.Value.First() == '-')
							negative = true;

						if (part.Value.Contains('X') || part.Value.Contains('x'))
							CarbonMult = negative ? -1 : 1;
						else if (part.Value.Contains('Y') || part.Value.Contains('y'))
							DoubleBondMult = negative ? -1 : 1;
						else
							Constant = negative ? -1 : 1;
					}
				}
			}
			MatchCollection formulaParts = Regex.Matches(formula, formulaRegex);
			if (formulaParts != null)
			{
				foreach (Match part in formulaParts)
				{
					if (part.Success)
					{
						Match number = Regex.Match(part.Value, numberRegex);
						if (number.Success)
						{
							if (part.Value.Contains('X') || part.Value.Contains('x'))
								CarbonMult = int.Parse(number.Value);
							else if (part.Value.Contains('Y') || part.Value.Contains('y'))
								DoubleBondMult = int.Parse(number.Value);
							else
								Constant = int.Parse(number.Value);
						}
					}
				}
			}
		}

		public int Evaluate(int numCarbons, int numDoubleBonds)
		{
			return CarbonMult * numCarbons + DoubleBondMult * numDoubleBonds + Constant;
		}

		public string GetEquationString()
		{
			return CarbonMult.ToString() + "X + " + DoubleBondMult.ToString() + "Y + " + Constant;
		}
	}
}
