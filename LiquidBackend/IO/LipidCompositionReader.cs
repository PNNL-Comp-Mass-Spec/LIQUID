using LiquidBackend.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.IO
{
	public class LipidCompositionReader<T> : FileReader<T> where T : LipidCompositionRule, new()
	{

		private const string COMMON_NAME = "COMMONNAME";
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
		private const string CONTAINS_LCBOH = "CONTAINSLCB+OH";
		private const string CONTAINS_OH = "CONTAINSOH";
		private const string CONTAINS_DEOXY = "CONTAINSDEOXY";
		private const string IS_OXO_CHO = "ISOXOCHO";
		private const string IS_OXO_COOH = "ISOXOCOOH";

		protected override Dictionary<string, int> CreateColumnMapping(string columnString)
		{
			var columnMap = new Dictionary<string, int>();
			var columnTitles = columnString.Split('\t', '\n');

			for (var i = 0; i < columnTitles.Length; i++)
			{
				var columnTitle = columnTitles[i].ToUpper();

				switch (columnTitle)
				{
					case COMMON_NAME:
						columnMap.Add(COMMON_NAME, i);
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
					case CONTAINS_LCBOH:
						columnMap.Add(CONTAINS_LCBOH, i);
						break;
					case CONTAINS_OH:
						columnMap.Add(CONTAINS_OH, i);
						break;
					case CONTAINS_DEOXY:
						columnMap.Add(CONTAINS_DEOXY, i);
						break;
					case IS_OXO_CHO:
						columnMap.Add(IS_OXO_CHO, i);
						break;
					case IS_OXO_COOH:
						columnMap.Add(IS_OXO_COOH, i);
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

			return lipid;
		}
	}
}
