using System;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
    public class ConditionForInteger
    {
        private string conditionString;
        private string op;
        private int conditionValue;

        public ConditionForInteger(string conditionString)
        {
            this.conditionString = conditionString.Replace(" ", "");
            if (this.conditionString.Contains("==")) this.op = "==";
            else if (this.conditionString.Contains("!=")) this.op = "!=";
            else if (this.conditionString.Contains(">=")) this.op = ">=";
            else if (this.conditionString.Contains(">")) this.op = ">";
            else if (this.conditionString.Contains("<=")) this.op = "<=";
            else if (this.conditionString.Contains("<")) this.op = "<";

            else this.op = "==";

            if (!this.conditionString.Contains("==") && this.op.Equals("==")) this.conditionValue = Int32.Parse(this.conditionString);
            else
            {
                string[] tokens = Regex.Split(this.conditionString, this.op);
                if (tokens[0].Equals("")) this.conditionValue = Int32.Parse(tokens[1]);
            }
        }

        public bool meet(int x)
        {
            switch (this.op)
            {
                case "==": return x == this.conditionValue;
                case "!=": return x != this.conditionValue;
                case ">": return x > this.conditionValue;
                case ">=": return x >= this.conditionValue;
                case "<": return x < this.conditionValue;
                case "<=": return x <= this.conditionValue;
            }
            return false;
        }
    }
}
