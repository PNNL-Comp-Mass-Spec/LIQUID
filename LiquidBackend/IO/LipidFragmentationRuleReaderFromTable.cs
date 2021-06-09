using LiquidBackend.Domain;
using System;
using System.Collections.Generic;

namespace LiquidBackend.IO
{
    public class LipidFragmentationRuleReaderFromTable<T> : FileReader<T> where T : LipidFragmentationRuleFromTable, new()
    {
        private const string LIPID_SUBCLASS = "LIPIDSUBCLASS";
        private const string FRAGMENTATION_MODE = "FRAGMENTATIONMODE";
        private const string LOSS_TYPE = "LOSSTYPE";
        private const string DESC1 = "DESCRIPTION1";
        private const string DESC2 = "DESCRIPTION2";
        private const string CARBON = "C";
        private const string HYDROGEN = "H";
        private const string NITROGEN = "N";
        private const string OXYGEN = "O";
        private const string SULFUR = "S";
        private const string PHOSPHORUS = "P";
        //TODO: How to handle Na and K
        private const string OTHER = "OTHER";
        private const string DIAGNOSTIC = "DIAGNOSTIC";
        private const string HEADER_GROUP = "HEADERGROUP";

        protected override Dictionary<string, int> CreateColumnMapping(string columnString)
        {
            var columnMap = new Dictionary<string, int>();
            var columnTitles = columnString.Split('\t', '\n');

            for (var i = 0; i < columnTitles.Length; i++)
            {
                var columnTitle = columnTitles[i].ToUpper();

                switch (columnTitle)
                {
                    case LIPID_SUBCLASS:
                        columnMap.Add(LIPID_SUBCLASS, i);
                        break;
                    case FRAGMENTATION_MODE:
                        columnMap.Add(FRAGMENTATION_MODE, i);
                        break;
                   case LOSS_TYPE:
                        columnMap.Add(LOSS_TYPE, i);
                        break;
                    case DESC1:
                        columnMap.Add(DESC1, i);
                        break;
                    case DESC2:
                        columnMap.Add(DESC2, i);
                        break;
                    case CARBON:
                        columnMap.Add(CARBON, i);
                        break;
                    case HYDROGEN:
                        columnMap.Add(HYDROGEN, i);
                        break;
                    case NITROGEN:
                        columnMap.Add(NITROGEN, i);
                        break;
                    case OXYGEN:
                        columnMap.Add(OXYGEN, i);
                        break;
                    case SULFUR:
                        columnMap.Add(SULFUR, i);
                        break;
                    case PHOSPHORUS:
                        columnMap.Add(PHOSPHORUS, i);
                        break;
                    case OTHER:
                        columnMap.Add(OTHER, i);
                        break;
                    case DIAGNOSTIC:
                        columnMap.Add(DIAGNOSTIC, i);
                        break;
                    case HEADER_GROUP:
                        columnMap.Add(HEADER_GROUP, i);
                        break;
                    }
            }

            return columnMap;
        }

        protected override T ParseLine(string line, IDictionary<string, int> columnMapping)
        {
            var columns = line.Split('\t', '\n');

            var fragmentationRule = new T();

            if (columnMapping.ContainsKey(LIPID_SUBCLASS)) fragmentationRule.LipidSubClass = columns[columnMapping[LIPID_SUBCLASS]];
            else throw new SystemException("LipidClass is required for importing fragmentation rules: " + line);

            if (columnMapping.ContainsKey(FRAGMENTATION_MODE))
            {
                var fragmentationMode = columns[columnMapping[FRAGMENTATION_MODE]];
                if (fragmentationMode.Equals("Positive")) fragmentationRule.FragmentationMode = FragmentationMode.Positive;
                else if (fragmentationMode.Equals("Negative")) fragmentationRule.FragmentationMode = FragmentationMode.Negative;
                else throw new SystemException("FragmentationMode is required to be Positive or Negative: " + line);
            }
            else
            {
                throw new SystemException("FragmentationMode is required for importing fragmentation rules: " + line);
            }

            if (columnMapping.ContainsKey(LOSS_TYPE)) fragmentationRule.LossType = columns[columnMapping[LOSS_TYPE]];
            else throw new SystemException("LossType is required for importing fragmentation rules: " + line);

            if (!(fragmentationRule.LossType.Equals("PI") ||
                  fragmentationRule.LossType.Equals("NL")))
            {
                throw new SystemException("LossType should be PI or NL: " + line);
            }

            if (columnMapping.ContainsKey(DESC1)) fragmentationRule.Description1 = columns[columnMapping[DESC1]];
            if (columnMapping.ContainsKey(DESC1)) fragmentationRule.Description2 = columns[columnMapping[DESC2]];
            if (columnMapping.ContainsKey(CARBON)) fragmentationRule.C = new CompositionFormula(columns[columnMapping[CARBON]]);
            if (columnMapping.ContainsKey(HYDROGEN)) fragmentationRule.H = new CompositionFormula(columns[columnMapping[HYDROGEN]]);
            if (columnMapping.ContainsKey(NITROGEN)) fragmentationRule.N = new CompositionFormula(columns[columnMapping[NITROGEN]]);
            if (columnMapping.ContainsKey(OXYGEN)) fragmentationRule.O = new CompositionFormula(columns[columnMapping[OXYGEN]]);
            if (columnMapping.ContainsKey(SULFUR)) fragmentationRule.S = new CompositionFormula(columns[columnMapping[SULFUR]]);
            if (columnMapping.ContainsKey(PHOSPHORUS)) fragmentationRule.P = new CompositionFormula(columns[columnMapping[PHOSPHORUS]]);
            if (columnMapping.ContainsKey(OTHER)) fragmentationRule.Other = columns[columnMapping[OTHER]];
            if (columnMapping.ContainsKey(DIAGNOSTIC)) fragmentationRule.Diagnostic = columns[columnMapping[DIAGNOSTIC]] == "1";
            if (columnMapping.ContainsKey(HEADER_GROUP)) fragmentationRule.HeaderGroup = columns[columnMapping[HEADER_GROUP]] == "1";

            return fragmentationRule;
        }
    }
}
