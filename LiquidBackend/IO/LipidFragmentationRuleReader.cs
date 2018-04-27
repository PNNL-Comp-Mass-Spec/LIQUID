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
        private const string COUNT_OF_STANDARD_ACYLSCHAINS = "COUNTOFSTANDARDACYLSCHAINS";
        private const string CONTAINS_HYDROXY = "CONTAINSHYDROXY";
        private const string SIALIC = "SIALIC";
        private const string ACYLCHAINTYPE = "ACYLCHAIN.ACYLCHAINTYPE";
        private const string ACYLCHAIN_NUMCARBONS = "ACYLCHAIN.NUMCARBONS";
        private const string ACYLCHAIN_NUMDOUBLEBONDS = "ACYLCHAIN.NUMDOUBLEBONDS";
        private const string ACYLCHAIN_HYDROXYPOSITION = "ACYLCHAIN.HYDROXYPOSITION";
        private const string TARGET_ACYLCHAINS = "TARGET_ACYLCHAINS";

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
                    case COUNT_OF_STANDARD_ACYLSCHAINS:
                        columnMap.Add(COUNT_OF_STANDARD_ACYLSCHAINS, i);
                        break;
                    case CONTAINS_HYDROXY:
                        columnMap.Add(CONTAINS_HYDROXY, i);
                        break;
                    case ACYLCHAINTYPE:
                        columnMap.Add(ACYLCHAINTYPE, i);
                        break;
                    case ACYLCHAIN_NUMCARBONS:
                        columnMap.Add(ACYLCHAIN_NUMCARBONS, i);
                        break;
                    case ACYLCHAIN_NUMDOUBLEBONDS:
                        columnMap.Add(ACYLCHAIN_NUMDOUBLEBONDS, i);
                        break;
                    case ACYLCHAIN_HYDROXYPOSITION:
                        columnMap.Add(ACYLCHAIN_HYDROXYPOSITION, i);
                        break;
                    case SIALIC:
                        columnMap.Add(SIALIC, i);
                        break;
                    case TARGET_ACYLCHAINS:
                        columnMap.Add(TARGET_ACYLCHAINS, i);
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
                if (columnMapping.ContainsKey(LIPID_CLASS)) fragmentationRule.lpidClass = columns[columnMapping[LIPID_CLASS]];
                else throw new SystemException("LipidClass is required for importing fragmentation rules: " + line);

                if (columnMapping.ContainsKey(FRAGMENTATION_MODE))
                {
                    string fragmentationMode = columns[columnMapping[FRAGMENTATION_MODE]];
                    if (fragmentationMode.Equals("Positive")) fragmentationRule.fragmentationMode = FragmentationMode.Positive;
                    else if (fragmentationMode.Equals("Negative")) fragmentationRule.fragmentationMode = FragmentationMode.Negative;
                    else throw new SystemException("FragmentationMode is required to be Positive or Negative: " + line);
                }
                else throw new SystemException("FragmentationMode is required for importing fragmentation rules: " + line);

                if (columnMapping.ContainsKey(NEUTRAL_LOSS)) fragmentationRule.isNeutralLoss = columns[columnMapping[NEUTRAL_LOSS]] == "1";
                else throw new SystemException("NEUTRAL_LOSS is required for importing fragmentation rules: " + line);

                if (columnMapping.ContainsKey(DESCRIPTION)) fragmentationRule.description = columns[columnMapping[DESCRIPTION]];
                if (columnMapping.ContainsKey(DIAGNOSTIC)) fragmentationRule.diagnastic = columns[columnMapping[DIAGNOSTIC]] == "1";

                if (columnMapping.ContainsKey(CARBON)) fragmentationRule.C = new CompositionFormula(columns[columnMapping[CARBON]]);
                if (columnMapping.ContainsKey(HYDROGEN)) fragmentationRule.H = new CompositionFormula(columns[columnMapping[HYDROGEN]]);
                if (columnMapping.ContainsKey(NITROGEN)) fragmentationRule.N = new CompositionFormula(columns[columnMapping[NITROGEN]]);
                if (columnMapping.ContainsKey(OXYGEN)) fragmentationRule.O = new CompositionFormula(columns[columnMapping[OXYGEN]]);
                if (columnMapping.ContainsKey(SULFUR)) fragmentationRule.S = new CompositionFormula(columns[columnMapping[SULFUR]]);
                if (columnMapping.ContainsKey(PHOSPHORUS)) fragmentationRule.P = new CompositionFormula(columns[columnMapping[PHOSPHORUS]]);

                if (columnMapping.ContainsKey(ADDITIONAL_ELEMENT) && !columns[columnMapping[ADDITIONAL_ELEMENT]].Equals(""))
                    fragmentationRule.additionalElement = columns[columnMapping[ADDITIONAL_ELEMENT]];
                if (columnMapping.ContainsKey(COUNT_OF_CHAINS) && !columns[columnMapping[COUNT_OF_CHAINS]].Equals(""))
                    fragmentationRule.conditionForCountOfChains = new ConditionForInteger(columns[columnMapping[COUNT_OF_CHAINS]]);
                if (columnMapping.ContainsKey(COUNT_OF_STANDARD_ACYLSCHAINS) && !columns[columnMapping[COUNT_OF_STANDARD_ACYLSCHAINS]].Equals(""))
                    fragmentationRule.conditionForCountOfStandardAcylsChains = new ConditionForInteger(columns[columnMapping[COUNT_OF_STANDARD_ACYLSCHAINS]]);
                if (columnMapping.ContainsKey(CONTAINS_HYDROXY) && !columns[columnMapping[CONTAINS_HYDROXY]].Equals(""))
                    fragmentationRule.conditionForContainsHydroxy = new ConditionForInteger(columns[columnMapping[CONTAINS_HYDROXY]]);
                if (columnMapping.ContainsKey(SIALIC) && !columns[columnMapping[SIALIC]].Equals("")) 
                    fragmentationRule.sialic = columns[columnMapping[SIALIC]];
                if (columnMapping.ContainsKey(ACYLCHAINTYPE) && !columns[columnMapping[ACYLCHAINTYPE]].Equals("")) 
                    fragmentationRule.acylChainType = columns[columnMapping[ACYLCHAINTYPE]];
                if (columnMapping.ContainsKey(ACYLCHAIN_NUMCARBONS) && !columns[columnMapping[ACYLCHAIN_NUMCARBONS]].Equals("")) 
                    fragmentationRule.acylChainNumCarbons = Int32.Parse(columns[columnMapping[ACYLCHAIN_NUMCARBONS]]);
                if (columnMapping.ContainsKey(ACYLCHAIN_NUMDOUBLEBONDS) && !columns[columnMapping[ACYLCHAIN_NUMDOUBLEBONDS]].Equals("")) 
                    fragmentationRule.acylChainNumDoubleBonds = Int32.Parse(columns[columnMapping[ACYLCHAIN_NUMDOUBLEBONDS]]);
                if (columnMapping.ContainsKey(ACYLCHAIN_HYDROXYPOSITION) && !columns[columnMapping[ACYLCHAIN_HYDROXYPOSITION]].Equals(""))
                    fragmentationRule.acylChainHydroxyPosition = Int32.Parse(columns[columnMapping[ACYLCHAIN_HYDROXYPOSITION]]);
                if (columnMapping.ContainsKey(TARGET_ACYLCHAINS) && !columns[columnMapping[TARGET_ACYLCHAINS]].Equals(""))
                {
                    if (columns[columnMapping[TARGET_ACYLCHAINS]].Equals("All"))
                    {
                        fragmentationRule.targetAcylChainsIndices = Enumerable.Range(1, 100).ToList();  // TODO: for any number of chains
                    }
                    else fragmentationRule.targetAcylChainsIndices = columns[columnMapping[TARGET_ACYLCHAINS]].Split(',').Select(Int32.Parse).ToList();
                }
                    
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERR] on " + line);
                throw e;
            }

            fragmentationRule.isFromHeader = fragmentationRule.checkFromHeader();
            //fragmentationRule.isSpecialCase = fragmentationRule.checkSpecialCase();

            return fragmentationRule;
        }
    }
}
