using LiquidBackend.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiquidBackend.IO
{
    public class LipidFragmentationRuleReader<T> : FileReader<T> where T : LipidFragmentationRule, new()
    {
        private const string LIPID_CLASS = "LIPIDCLASS";
        private const string FRAGMENTATION_MODE = "FRAGMENTATIONMODE";
        private const string NEUTRAL_LOSS = "NEUTRAL_LOSS";
        private const string DESCRIPTION = "DESC";
        private const string DIAGNOSTIC = "DIAGNOSTIC";
        private const string CARBON = "C";
        private const string HYDROGEN = "H";
        private const string NITROGEN = "N";
        private const string OXYGEN = "O";
        private const string SULFUR = "S";
        private const string PHOSPHORUS = "P";
        private const string ADDITIONAL_ELEMENT = "ADDITIONAL_ELEMENT";
        private const string COUNT_OF_CHAINS = "COUNTOFCHAINS";
        private const string COUNT_OF_STANDARD_ACYL_CHAINS = "COUNTOFSTANDARDACYLSCHAINS";
        private const string CONTAINS_HYDROXY = "CONTAINSHYDROXY";
        private const string SIALIC = "SIALIC";
        private const string ACYL_CHAIN_TYPE = "ACYLCHAIN.ACYLCHAINTYPE";
        private const string ACYL_CHAIN_NUM_CARBONS = "ACYLCHAIN.NUMCARBONS";
        private const string ACYL_CHAIN_NUM_DOUBLE_BONDS = "ACYLCHAIN.NUMDOUBLEBONDS";
        private const string ACYL_CHAIN_HYDROXY_POSITION = "ACYLCHAIN.HYDROXYPOSITION";
        private const string TARGET_ACYL_CHAINS = "TARGET_ACYLCHAINS";

        protected override Dictionary<string, int> CreateColumnMapping(string columnString)
        {
            var columnMap = new Dictionary<string, int>();
            var columnTitles = columnString.Split('\t', '\n');

            for (var i = 0; i < columnTitles.Length; i++)
            {
                var columnTitle = columnTitles[i].ToUpper();

                switch (columnTitle)
                {
                    case LIPID_CLASS:
                        columnMap.Add(LIPID_CLASS, i);
                        break;
                    case FRAGMENTATION_MODE:
                        columnMap.Add(FRAGMENTATION_MODE, i);
                        break;
                    case NEUTRAL_LOSS:
                        columnMap.Add(NEUTRAL_LOSS, i);
                        break;
                    case DESCRIPTION:
                        columnMap.Add(DESCRIPTION, i);
                        break;
                    case DIAGNOSTIC:
                        columnMap.Add(DIAGNOSTIC, i);
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
                    case ADDITIONAL_ELEMENT:
                        columnMap.Add(ADDITIONAL_ELEMENT, i);
                        break;
                    case COUNT_OF_CHAINS:
                        columnMap.Add(COUNT_OF_CHAINS, i);
                        break;
                    case COUNT_OF_STANDARD_ACYL_CHAINS:
                        columnMap.Add(COUNT_OF_STANDARD_ACYL_CHAINS, i);
                        break;
                    case CONTAINS_HYDROXY:
                        columnMap.Add(CONTAINS_HYDROXY, i);
                        break;
                    case ACYL_CHAIN_TYPE:
                        columnMap.Add(ACYL_CHAIN_TYPE, i);
                        break;
                    case ACYL_CHAIN_NUM_CARBONS:
                        columnMap.Add(ACYL_CHAIN_NUM_CARBONS, i);
                        break;
                    case ACYL_CHAIN_NUM_DOUBLE_BONDS:
                        columnMap.Add(ACYL_CHAIN_NUM_DOUBLE_BONDS, i);
                        break;
                    case ACYL_CHAIN_HYDROXY_POSITION:
                        columnMap.Add(ACYL_CHAIN_HYDROXY_POSITION, i);
                        break;
                    case SIALIC:
                        columnMap.Add(SIALIC, i);
                        break;
                    case TARGET_ACYL_CHAINS:
                        columnMap.Add(TARGET_ACYL_CHAINS, i);
                        break;
                }
            }

            return columnMap;
        }

        protected override T ParseLine(string line, IDictionary<string, int> columnMapping)
        {
            var columns = line.Split('\t', '\n');

            var fragmentationRule = new T();

            try
            {
                if (columnMapping.ContainsKey(LIPID_CLASS)) fragmentationRule.LipidClass = columns[columnMapping[LIPID_CLASS]];
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

                if (columnMapping.ContainsKey(NEUTRAL_LOSS)) fragmentationRule.IsNeutralLoss = columns[columnMapping[NEUTRAL_LOSS]] == "1";
                else throw new SystemException("NEUTRAL_LOSS is required for importing fragmentation rules: " + line);

                if (columnMapping.ContainsKey(DESCRIPTION)) fragmentationRule.Description = columns[columnMapping[DESCRIPTION]];
                if (columnMapping.ContainsKey(DIAGNOSTIC)) fragmentationRule.Diagnostic = columns[columnMapping[DIAGNOSTIC]] == "1";

                if (columnMapping.ContainsKey(CARBON)) fragmentationRule.C = new CompositionFormula(columns[columnMapping[CARBON]]);
                if (columnMapping.ContainsKey(HYDROGEN)) fragmentationRule.H = new CompositionFormula(columns[columnMapping[HYDROGEN]]);
                if (columnMapping.ContainsKey(NITROGEN)) fragmentationRule.N = new CompositionFormula(columns[columnMapping[NITROGEN]]);
                if (columnMapping.ContainsKey(OXYGEN)) fragmentationRule.O = new CompositionFormula(columns[columnMapping[OXYGEN]]);
                if (columnMapping.ContainsKey(SULFUR)) fragmentationRule.S = new CompositionFormula(columns[columnMapping[SULFUR]]);
                if (columnMapping.ContainsKey(PHOSPHORUS)) fragmentationRule.P = new CompositionFormula(columns[columnMapping[PHOSPHORUS]]);

                if (columnMapping.ContainsKey(ADDITIONAL_ELEMENT) && !columns[columnMapping[ADDITIONAL_ELEMENT]].Equals(""))
                    fragmentationRule.AdditionalElement = columns[columnMapping[ADDITIONAL_ELEMENT]];
                if (columnMapping.ContainsKey(COUNT_OF_CHAINS) && !columns[columnMapping[COUNT_OF_CHAINS]].Equals(""))
                    fragmentationRule.ConditionForCountOfChains = new ConditionForInteger(columns[columnMapping[COUNT_OF_CHAINS]]);
                if (columnMapping.ContainsKey(COUNT_OF_STANDARD_ACYL_CHAINS) && !columns[columnMapping[COUNT_OF_STANDARD_ACYL_CHAINS]].Equals(""))
                    fragmentationRule.ConditionForCountOfStandardAcylsChains = new ConditionForInteger(columns[columnMapping[COUNT_OF_STANDARD_ACYL_CHAINS]]);
                if (columnMapping.ContainsKey(CONTAINS_HYDROXY) && !columns[columnMapping[CONTAINS_HYDROXY]].Equals(""))
                    fragmentationRule.ConditionForContainsHydroxy = new ConditionForInteger(columns[columnMapping[CONTAINS_HYDROXY]]);
                if (columnMapping.ContainsKey(SIALIC) && !columns[columnMapping[SIALIC]].Equals(""))
                    fragmentationRule.Sialic = columns[columnMapping[SIALIC]];
                if (columnMapping.ContainsKey(ACYL_CHAIN_TYPE) && !columns[columnMapping[ACYL_CHAIN_TYPE]].Equals(""))
                    fragmentationRule.AcylChainType = columns[columnMapping[ACYL_CHAIN_TYPE]];
                if (columnMapping.ContainsKey(ACYL_CHAIN_NUM_CARBONS) && !columns[columnMapping[ACYL_CHAIN_NUM_CARBONS]].Equals(""))
                    fragmentationRule.AcylChainNumCarbons = int.Parse(columns[columnMapping[ACYL_CHAIN_NUM_CARBONS]]);
                if (columnMapping.ContainsKey(ACYL_CHAIN_NUM_DOUBLE_BONDS) && !columns[columnMapping[ACYL_CHAIN_NUM_DOUBLE_BONDS]].Equals(""))
                    fragmentationRule.AcylChainNumDoubleBonds = int.Parse(columns[columnMapping[ACYL_CHAIN_NUM_DOUBLE_BONDS]]);
                if (columnMapping.ContainsKey(ACYL_CHAIN_HYDROXY_POSITION) && !columns[columnMapping[ACYL_CHAIN_HYDROXY_POSITION]].Equals(""))
                    fragmentationRule.AcylChainHydroxyPosition = int.Parse(columns[columnMapping[ACYL_CHAIN_HYDROXY_POSITION]]);
                if (columnMapping.ContainsKey(TARGET_ACYL_CHAINS) && !columns[columnMapping[TARGET_ACYL_CHAINS]].Equals(""))
                {
                    if (columns[columnMapping[TARGET_ACYL_CHAINS]].Equals("All"))
                    {
                        fragmentationRule.TargetAcylChainsIndices = Enumerable.Range(1, 100).ToList();  // TODO: for any number of chains
                    }
                    else
                    {
                        fragmentationRule.TargetAcylChainsIndices = columns[columnMapping[TARGET_ACYL_CHAINS]].Split(',').Select(int.Parse).ToList();
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[ERR] on " + line);
                throw;
            }

            fragmentationRule.IsFromHeader = fragmentationRule.CheckFromHeader();
            //fragmentationRule.isSpecialCase = fragmentationRule.checkSpecialCase();

            return fragmentationRule;
        }
    }
}
