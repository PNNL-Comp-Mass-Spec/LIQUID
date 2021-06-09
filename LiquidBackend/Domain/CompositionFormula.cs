using System.Linq;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
    public class CompositionFormula
    {
        private const string formulaRegex = @"[\-]?[0-9]+[YXyx]?";
        private const string singleVarRegex = @"[^\-]?[^0-9]?[XYxy]{1}";
        private const string numberRegex = @"[\-]?[0-9]+";

        public int CarbonMultiplier { get; }
        public int DoubleBondMultiplier { get; }
        public int Constant { get; }

        public CompositionFormula(string formula)
        {
            CarbonMultiplier = 0;
            DoubleBondMultiplier = 0;
            Constant = 0;

            var singleVars = Regex.Matches(formula, singleVarRegex);

            foreach (Match part in singleVars)
            {
                if (part.Success)
                {
                    var negative = part.Value.First() == '-';

                    if (part.Value.Contains('X') || part.Value.Contains('x'))
                        CarbonMultiplier = negative ? -1 : 1;
                    else if (part.Value.Contains('Y') || part.Value.Contains('y'))
                        DoubleBondMultiplier = negative ? -1 : 1;
                    else
                        Constant = negative ? -1 : 1;
                }
            }
            var formulaParts = Regex.Matches(formula, formulaRegex);
            foreach (Match part in formulaParts)
            {
                if (part.Success)
                {
                    var number = Regex.Match(part.Value, numberRegex);
                    if (number.Success)
                    {
                        if (part.Value.Contains('X') || part.Value.Contains('x'))
                            CarbonMultiplier = int.Parse(number.Value);
                        else if (part.Value.Contains('Y') || part.Value.Contains('y'))
                            DoubleBondMultiplier = int.Parse(number.Value);
                        else
                            Constant = int.Parse(number.Value);
                    }
                }
            }
        }

        public int Evaluate(int numCarbons, int numDoubleBonds)
        {
            return CarbonMultiplier * numCarbons + DoubleBondMultiplier * numDoubleBonds + Constant;
        }

        public string GetEquationString()
        {
            return CarbonMultiplier + "X + " + DoubleBondMultiplier + "Y + " + Constant;
        }
    }
}
