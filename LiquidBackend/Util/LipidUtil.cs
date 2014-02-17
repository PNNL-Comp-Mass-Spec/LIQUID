﻿using System;
using System.Collections.Generic;
using System.Linq;
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
				// TODO: Add in any extra search criteria for classes that may not have straight forward parsing
			}

			return lipidClass;
		}

		public static IEnumerable<AcylChain> ParseLipidCommonNameIntoAcylChains(string commonName)
		{
			MatchCollection matchCollection = Regex.Matches(commonName, "\\d+:\\d+");

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
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 5 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(O-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(O-)-H2O"));
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
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 8, 1, 4, 0, 1).Mass, "C2H8NO4P"));
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPE(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPE(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O"));
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
				else if (lipidClass == LipidClass.PS || lipidClass == LipidClass.PG)
				{
					if (lipidClass == LipidClass.PS) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 8, 1, 0, 0, 1).Mass, "Lipid-C3H8NO6P"));
					if (lipidClass == LipidClass.PG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 12, 1, 6, 0, 1).Mass, "Lipid-C3H12O6NP"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz + new Composition(0, 4, 1, 0, 0, 0).Mass, "Lipid+NH4"));
					}

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 9, 0, 1).Mass, "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPS(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPS(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O"));
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA long"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA short"));
								break;
							case AcylChainType.Dihydro:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LCB+H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB-CH2"));
								break;
						}
					}

					if (countOfChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 0 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "both chains"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "both chains - H2O"));
					}
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
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

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, (2 * carbons) + 3 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "Lipid-RCOOH-NH3"));	
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
					}

					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O"));
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG"));
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA"));
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
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 12, 1, 5, 0, 1).Mass, "C5H12O5NP"));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 3)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen+H2O"));
								break;
						}
					}
				}
			}

			return msMsSearchUnitList;
		}
	}
}
