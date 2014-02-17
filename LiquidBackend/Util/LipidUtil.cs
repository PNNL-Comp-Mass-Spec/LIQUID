using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Database;
using LiquidBackend.Domain;

namespace LiquidBackend.Util
{
	public class LipidUtil
	{
		public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, LipidClass lipidClass, FragmentationMode fragmentationMode, IEnumerable<AcylChain> acylChainList)
		{
			Composition composition = Composition.ParseFromPlainString(empiricalFormula);
			return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
		}

		public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, LipidClass lipidClass, FragmentationMode fragmentationMode)
		{
			Composition composition = Composition.ParseFromPlainString(empiricalFormula);
			IEnumerable<AcylChain> acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
			return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
		}

		public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, FragmentationMode fragmentationMode)
		{
			Composition composition = Composition.ParseFromPlainString(empiricalFormula);
			IEnumerable<AcylChain> acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
			LipidClass lipidClass = ParseLipidCommonNameIntoClass(commonName);
			return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
		}

		public static LipidTarget CreateLipidTarget(string commonName, string empiricalFormula, string fragmentationMode)
		{
			Composition composition = Composition.ParseFromPlainString(empiricalFormula);
			IEnumerable<AcylChain> acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
			LipidClass lipidClass = ParseLipidCommonNameIntoClass(commonName);
			FragmentationMode fragmentationModeAsEnum = (FragmentationMode)Enum.Parse(typeof (FragmentationMode), fragmentationMode);
			return new LipidTarget(commonName, lipidClass, fragmentationModeAsEnum, composition, acylChainList);
		}

		public static LipidClass ParseLipidCommonNameIntoClass(string commonName)
		{
			string classAbbrev = commonName.Split('(')[0];

			LipidClass lipidClass;
			bool classFound = Enum.TryParse(classAbbrev, true, out lipidClass);

			if (!classFound)
			{
				// Add in any extra search criteria for classes that may not have straight forward parsing
				if (classAbbrev.Contains("PIP2")) return LipidClass.PIP2;
				if (classAbbrev.Contains("PIP3")) return LipidClass.PIP3;
				if (classAbbrev.Contains("PIP")) return LipidClass.PIP;
				if (classAbbrev.Contains("cholest")) return LipidClass.Cholesterol;
			}

			return lipidClass;
		}

		public static IEnumerable<AcylChain> ParseLipidCommonNameIntoAcylChains(string commonName)
		{
			MatchCollection matchCollection = Regex.Matches(commonName, "([A-z]-?)?\\d+:\\d+");

			IEnumerable<AcylChain> acylChains = (from object match in matchCollection select new AcylChain(match.ToString()));
			return acylChains;
		}

		public static List<MsMsSearchUnit> CreateMsMsSearchUnits(double precursorMz, LipidClass lipidClass, FragmentationMode fragmentationMode, IEnumerable<AcylChain> acylChainList)
		{
			List<MsMsSearchUnit> msMsSearchUnitList = new List<MsMsSearchUnit>();

			if (fragmentationMode == FragmentationMode.Positive)
			{
				if (lipidClass == LipidClass.PC)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "Lipid-(CH2)3NH3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 14, 1, 1, 0, 0).Mass, "C5H14ON"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 6, 0, 4, 0, 1).Mass, "C2H6O4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 19, 1, 5, 0, 1).Mass, "C8H19O5NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 21, 1, 6, 0, 1).Mass, "C8H21O6NP"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						string fattyAcylDisplay = acylChain.ToString();
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 5 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(O-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(O-)-H2O", acylChain));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG"));
					}
				}
				else if (lipidClass == LipidClass.PE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 8, 1, 4, 0, 1).Mass, "Lipid-C2H8NO4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPE(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPE(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG"));
					}
				}
				else if (lipidClass == LipidClass.PS)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 8, 1, 6, 0, 1).Mass, "Lipid-C3H8NO6P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 9, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPS(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPS(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG"));
					}
				}
				else if (lipidClass == LipidClass.PG)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 12, 1, 6, 0, 1).Mass, "Lipid-C3H12O6NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz + new Composition(0, 4, 1, 0, 0, 0).Mass, "Lipid+NH4"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 2 - (2 * doubleBonds), 0, 9, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 0 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPS(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPS(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG"));
					}
				}
				else if (lipidClass == LipidClass.Cer || lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.LacCer || lipidClass == LipidClass.CerP || lipidClass == LipidClass.SM)
				{
					if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.LacCer)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "Lipid-sugar"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "Lipid-sugar-H2O"));
					}

					if (lipidClass == LipidClass.LacCer) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 22, 0, 11, 0, 0).Mass, "Lipid-2(sugar)"));

					if (lipidClass == LipidClass.CerP)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "Lipid-H3PO4"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 5, 0, 5, 0, 1).Mass, "Lipid-H3PO4-H2O"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "Lipid-HPO3"));
					}

					if (lipidClass == LipidClass.SM)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "Lipid-(CH2)3NH3"));
					}
					
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "Lipid-2(H2O)"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA long", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA short", acylChain));
								break;
							case AcylChainType.Dihydro:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 2 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB-C, acylChain"));
								break;
						}
					}

					if (countOfChains == 2)
					{
						int carbons = acylChainList.Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Sum(x => x.NumDoubleBonds);

						if (lipidClass != LipidClass.Cer) {
							msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 0 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "both chains"));
							msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "both chains - H2O"));
						}
					}
				}
				else if (lipidClass == LipidClass.Cholesterol)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45"));
				}
				else if (lipidClass == LipidClass.CE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45"));

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.MG || lipidClass == LipidClass.DG || lipidClass == LipidClass.TG)
				{
					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, (2 * carbons) + 3 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "Lipid-RCOOH-NH3", acylChain));	
					}
				}
				else if (lipidClass == LipidClass.MGDG || lipidClass == LipidClass.SQDG || lipidClass == LipidClass.DGDG)
				{
					if (lipidClass == LipidClass.MGDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 11, 0, 6, 0, 0).Mass, "Lipid-C6H11O6"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 13, 0, 7, 0, 0).Mass, "Lipid-C6H13O7"));
					}
					else if (lipidClass == LipidClass.SQDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 15, 1, 8, 1, 0).Mass, "Lipid-C6H11O8SNH4"));
					}
					else if (lipidClass == LipidClass.DGDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 21, 0, 11, 0, 0).Mass, "Lipid-C12H21O11"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 25, 1, 11, 0, 0).Mass, "Lipid-C12H21O11NH4"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "Lipid-sugar"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "Lipid-sugar-H2O"));
					}

					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));
					}
				}
			}
			else if (fragmentationMode == FragmentationMode.Negative)
			{
				if (lipidClass == LipidClass.PC)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "Lipid-(acetate + methyl)"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(7, 16, 1, 5, 0, 1).Mass, "Lipid-C7H16O5NP"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 11, 1, 5, 0, 1).Mass, "C5H11O5NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 13, 1, 6, 0, 1).Mass, "C5H13O6NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 6, 1, 0, 0, 0).Mass, "Lipid-C2H6N"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 12, 1, 5, 0, 1).Mass, "Lipid-C5H12O5NP"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen+H2O", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PI)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "C6H10O8P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 18, 0, 11, 0, 1).Mass, "C9H18O11P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 16, 0, 10, 0, 1).Mass, "C9H16O10P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "Lipid-sugar"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(9, 17, 0, 10, 0, 1).Mass, "Lipid-C9H17O10P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2H2O-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 0, 0, 3, 0, 1).Mass, "PO3"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 12, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PIP)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 11, 0, 2).Mass, "C6H11O11P2"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "Lipid-H2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "Lipid-2(H2O)"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "Lipid-H3O4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "Lipid-HPO3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 13, 0, 12, 0, 2).Mass, "IP2-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 11, 0, 2).Mass, "IP2-H2O-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 10, 0, 2).Mass, "IP2-2(H2O)-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "IP-H2O-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2(H2O)-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 3, 0, 7, 0, 2).Mass, "H3P2O7"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 1, 0, 6, 0, 2).Mass, "HP2O6"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));

					if (lipidClass == LipidClass.PIP2 || lipidClass == LipidClass.PIP3)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 15, 0, 3).Mass, "IP3-H"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 14, 0, 3).Mass, "IP3-H2O-H"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 13, 0, 3).Mass, "IP3-2(H2O)-H"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 6, 0, 6, 0, 1).Mass, "IP-3(H2O)-H"));
					}
					if (lipidClass == LipidClass.PIP3)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 16, 0, 18, 0, 4).Mass, "IP4-H"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 17, 0, 4).Mass, "IP4-H2O-H"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 16, 0, 4).Mass, "IP4-2(H2O)-H"));
					}

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "PA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA-CO2", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 3 - (2 * doubleBonds), 0, 14, 0, 2).Mass, "LPIP-H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 5 - (2 * doubleBonds), 0, 13, 0, 2).Mass, "LPIP-2H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 12, 0, 1).Mass, "Lipid-Ketene", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "Lipid-FA", acylChain));

								if (lipidClass == LipidClass.PIP3)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 17, 0, 3).Mass, "LPIP2-H2O-H", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 16, 0, 3).Mass, "LPIP2-2H2O-H", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PG)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 6, 0, 1).Mass, "C6H10O6P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "Lipid-C3H6O2"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 0 - (2 * doubleBonds), 0, 9, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 2 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PA)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 2, 0, 4, 0, 1).Mass, "PO4H2"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 0, 0, 3, 0, 1).Mass, "PO3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 8, 0, 6, 0, 1).Mass, "C3H8O6P"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PS)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 5, 1, 2, 0, 0).Mass, "Lipid-C3H5O2N"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 8, 0, 6, 0, 1).Mass, "C3H8O6P"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "Lipid-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "Lipid-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen-head", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen-head-H2O", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "Lipid-sugar"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "Lipid-sugar-H2O"));

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								break;
						}
					}
				}

			}

			return msMsSearchUnitList;
		}

		/// <summary>
		/// Calculates the PPM error between two values.
		/// </summary>
		/// <param name="num1">Expected value.</param>
		/// <param name="num2">Observed value.</param>
		/// <returns>PPM error between expected and observed value.</returns>
		public static double PpmError(double num1, double num2)
		{
			// (X - Y) / X * 1,000,000
			return (num2 - num1) / num2 * 1000000;
		}
	}
}
