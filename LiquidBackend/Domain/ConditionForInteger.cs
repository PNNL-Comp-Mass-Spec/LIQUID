using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
    public class ConditionForInteger
    {
        private readonly string op;
        private readonly int conditionValue;

        public ConditionForInteger(string conditionString)
        {
            var condition = conditionString.Replace(" ", "");
            if (condition.Contains("==")) op = "==";
            else if (condition.Contains("!=")) op = "!=";
            else if (condition.Contains(">=")) op = ">=";
            else if (condition.Contains(">")) op = ">";
            else if (condition.Contains("<=")) op = "<=";
            else if (condition.Contains("<")) op = "<";

            else op = "==";

            if (!condition.Contains("==") && op.Equals("==")) conditionValue = int.Parse(condition);
            else
            {
                var tokens = Regex.Split(condition, op);
                if (tokens[0].Equals("")) conditionValue = int.Parse(tokens[1]);
            }
        }

        public bool meet(int x)
        {
            switch (op)
            {
                case "==": return x == conditionValue;
                case "!=": return x != conditionValue;
                case ">": return x > conditionValue;
                case ">=": return x >= conditionValue;
                case "<": return x < conditionValue;
                case "<=": return x <= conditionValue;
            }
            return false;
        }
    }
}
