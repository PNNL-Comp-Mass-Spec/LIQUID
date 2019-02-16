using LiquidBackend.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.IO
{
    public class LipidCompositionRuleReader<T> : FileReader<T> where T : LipidCompositionRule, new()
    {
        private const string LIPID_CLASS = "LIPIDCLASS";
        private const string LIPID_SUBCLASS = "LIPIDSUBCLASS";
        private const string CATEGORY = "CATEGORY";
        private const string MAIN_CLASS = "MAINCLASS";
        private const string SUB_CLASS = "SUBCLASS";
        private const string CARBON = "C";
        private const string HYDROGEN = "H";
        private const string NITROGEN = "N";
        private const string OXYGEN = "O";
        private const string SULFUR = "S";
        private const string PHOSPHORUS = "P";
        private const string EXAMPLE = "EXAMPLE";
        private const string FORMULA_NA = "FORMULA-NOADDUCT";
        private const string IONIZATION_MODE = "IONIZATIONMODE";
        private const string NUM_CHAINS = "NUMCHAINS";
        private const string CONTAINS_ETHER = "CONTAINSETHER";
        private const string CONTAINS_DIETHER = "CONTAINSDIETHER";
        private const string CONTAINS_PLASMALOGEN = "CONTAINSPLASMALOGEN";
        private const string CONTAINS_LCB = "CONTAINSLCB";
        private const string CONTAINS_LCB_PLUS_OH = "CONTAINSLCB+OH";
        private const string CONTAINS_LCB_MINUS_OH = "CONTAINSLCB-OH";
        private const string IS_OXO_CHO = "ISOXOCHO";
        private const string IS_OXO_COOH = "ISOXOCOOH";
        private const string NUM_OH = "NUMOH";
        private const string CONTAINS_OOH = "CONTAINSOOH";
        private const string CONTAINS_F2ISOP = "CONTAINSF2ISOP";

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
                    case LIPID_SUBCLASS:
                        columnMap.Add(LIPID_SUBCLASS, i);
                        break;
                    case CATEGORY:
                        columnMap.Add(CATEGORY, i);
                        break;
                    case MAIN_CLASS:
                        columnMap.Add(MAIN_CLASS, i);
                        break;
                    case SUB_CLASS:
                        columnMap.Add(SUB_CLASS, i);
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
                    case EXAMPLE:
                        columnMap.Add(EXAMPLE, i);
                        break;
                    case FORMULA_NA:
                        columnMap.Add(FORMULA_NA, i);
                        break;
                    case IONIZATION_MODE:
                        columnMap.Add(IONIZATION_MODE, i);
                        break;
                    case NUM_CHAINS:
                        columnMap.Add(NUM_CHAINS, i);
                        break;
                    case CONTAINS_ETHER:
                        columnMap.Add(CONTAINS_ETHER, i);
                        break;
                    case CONTAINS_DIETHER:
                        columnMap.Add(CONTAINS_DIETHER, i);
                        break;
                    case CONTAINS_PLASMALOGEN:
                        columnMap.Add(CONTAINS_PLASMALOGEN, i);
                        break;
                    case CONTAINS_LCB:
                        columnMap.Add(CONTAINS_LCB, i);
                        break;
                    case CONTAINS_LCB_PLUS_OH:
                        columnMap.Add(CONTAINS_LCB_PLUS_OH, i);
                        break;
                    case CONTAINS_LCB_MINUS_OH:
                        columnMap.Add(CONTAINS_LCB_MINUS_OH, i);
                        break;
                    case IS_OXO_CHO:
                        columnMap.Add(IS_OXO_CHO, i);
                        break;
                    case IS_OXO_COOH:
                        columnMap.Add(IS_OXO_COOH, i);
                        break;
                    case NUM_OH:
                        columnMap.Add(NUM_OH, i);
                        break;
                    case CONTAINS_OOH:
                        columnMap.Add(CONTAINS_OOH, i);
                        break;
                    case CONTAINS_F2ISOP:
                        columnMap.Add(CONTAINS_F2ISOP, i);
                        break;
                }
            }

            return columnMap;
        }

        protected override T ParseLine(string line, IDictionary<string, int> columnMapping)
        {
            var columns = line.Split('\t', '\n');

            var lipid = new T();

            //Convert and populate object
            if (columnMapping.ContainsKey(LIPID_CLASS)) lipid.LipidClass = columns[columnMapping[LIPID_CLASS]].ToUpper();
            if (columnMapping.ContainsKey(LIPID_SUBCLASS)) lipid.LipidSubClass = columns[columnMapping[LIPID_SUBCLASS]];
            if (columnMapping.ContainsKey(CATEGORY)) lipid.Category = columns[columnMapping[CATEGORY]];
            if (columnMapping.ContainsKey(MAIN_CLASS)) lipid.MainClass = columns[columnMapping[MAIN_CLASS]];
            if (columnMapping.ContainsKey(SUB_CLASS)) lipid.SubClass = columns[columnMapping[SUB_CLASS]];
            if (columnMapping.ContainsKey(CARBON)) lipid.C = new CompositionFormula(columns[columnMapping[CARBON]]);
            if (columnMapping.ContainsKey(HYDROGEN)) lipid.H = new CompositionFormula(columns[columnMapping[HYDROGEN]]);
            if (columnMapping.ContainsKey(NITROGEN)) lipid.N = new CompositionFormula(columns[columnMapping[NITROGEN]]);
            if (columnMapping.ContainsKey(OXYGEN)) lipid.O = new CompositionFormula(columns[columnMapping[OXYGEN]]);
            if (columnMapping.ContainsKey(SULFUR)) lipid.S = new CompositionFormula(columns[columnMapping[SULFUR]]);
            if (columnMapping.ContainsKey(PHOSPHORUS)) lipid.P = new CompositionFormula(columns[columnMapping[PHOSPHORUS]]);
            if (columnMapping.ContainsKey(EXAMPLE)) lipid.Example = columns[columnMapping[EXAMPLE]];
            if (columnMapping.ContainsKey(FORMULA_NA)) lipid.Formula = columns[columnMapping[FORMULA_NA]];
            if (columnMapping.ContainsKey(IONIZATION_MODE)) lipid.IonizationMode = columns[columnMapping[IONIZATION_MODE]];
            if (columnMapping.ContainsKey(NUM_CHAINS))
            {
                int numChains = 0;
                int.TryParse(columns[columnMapping[NUM_CHAINS]], out numChains);
                lipid.NumChains = numChains;
            }
            if (columnMapping.ContainsKey(CONTAINS_ETHER))
            {
                int containsEther = 0;
                int.TryParse(columns[columnMapping[CONTAINS_ETHER]], out containsEther);
                lipid.ContainsEther = (containsEther == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_DIETHER))
            {
                int containsDiether = 0;
                int.TryParse(columns[columnMapping[CONTAINS_DIETHER]], out containsDiether);
                lipid.ContainsDiether = (containsDiether == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_PLASMALOGEN))
            {
                int containsPlasmalogen = 0;
                int.TryParse(columns[columnMapping[CONTAINS_PLASMALOGEN]], out containsPlasmalogen);
                lipid.ContainsPlasmalogen = (containsPlasmalogen == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_LCB))
            {
                int containsLcb = 0;
                int.TryParse(columns[columnMapping[CONTAINS_LCB]], out containsLcb);
                lipid.ContainsLCB = (containsLcb == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_LCB_PLUS_OH))
            {
                int containsLcbPlusOh = 0;
                int.TryParse(columns[columnMapping[CONTAINS_LCB_PLUS_OH]], out containsLcbPlusOh);
                lipid.ContainsLCBPlusOH = (containsLcbPlusOh == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_LCB_MINUS_OH))
            {
                int containsLcbMinusOh = 0;
                int.TryParse(columns[columnMapping[CONTAINS_LCB_MINUS_OH]], out containsLcbMinusOh);
                lipid.ContainsLCBMinusOH = (containsLcbMinusOh == 1);
            }
            if (columnMapping.ContainsKey(IS_OXO_CHO))
            {
                int isOxoCho = 0;
                int.TryParse(columns[columnMapping[IS_OXO_CHO]], out isOxoCho);
                lipid.IsOxoCHO = isOxoCho == 1 ? true : false;
            }
            if (columnMapping.ContainsKey(IS_OXO_COOH))
            {
                int isOxoCooh = 0;
                int.TryParse(columns[columnMapping[IS_OXO_COOH]], out isOxoCooh);
                lipid.IsOxoCOOH = isOxoCooh == 1 ? true : false;
            }
            if (columnMapping.ContainsKey(NUM_OH))
            {
                int numOH = 0;
                int.TryParse(columns[columnMapping[NUM_OH]], out numOH);
                lipid.NumOH = numOH;
            }
            if (columnMapping.ContainsKey(CONTAINS_OOH))
            {
                int containsOOH = 0;
                int.TryParse(columns[columnMapping[CONTAINS_OOH]], out containsOOH);
                lipid.ContainsOOH = (containsOOH == 1);
            }
            if (columnMapping.ContainsKey(CONTAINS_F2ISOP))
            {
                int containsF2IsoP = 0;
                int.TryParse(columns[columnMapping[CONTAINS_F2ISOP]], out containsF2IsoP);
                lipid.ContainsF2IsoP = (containsF2IsoP == 1);
            }

            return lipid;
        }
    }
}
