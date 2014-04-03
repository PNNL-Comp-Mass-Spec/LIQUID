using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Composition;
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

		public static LipidTarget CreateLipidTarget(string commonName, FragmentationMode fragmentationMode, Adduct adduct)
		{
			Composition composition = ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName);
			Composition compositionOfAdduct = GetCompositionOfAdduct(adduct);

			if (adduct == Adduct.Acetate) composition += compositionOfAdduct;
			else if (fragmentationMode == FragmentationMode.Negative) composition -= compositionOfAdduct;
			else if (fragmentationMode == FragmentationMode.Positive) composition += compositionOfAdduct;

			IEnumerable<AcylChain> acylChainList = ParseLipidCommonNameIntoAcylChains(commonName);
			LipidClass lipidClass = ParseLipidCommonNameIntoClass(commonName);

			return new LipidTarget(commonName, lipidClass, fragmentationMode, composition, acylChainList);
		}

		public static Composition GetCompositionOfAdduct(Adduct adduct)
		{
			if (adduct == Adduct.Hydrogen)
			{
				return Composition.Hydrogen;
			}
			if (adduct == Adduct.Ammonium)
			{
				return new Composition(0, 4, 1, 0, 0);
			}
			if (adduct == Adduct.Acetate)
			{
				return new Composition(2, 3, 0, 2, 0);
			}

			throw new SystemException("Unrecognized Adduct: " + adduct);
		}

		public static LipidClass ParseLipidCommonNameIntoClass(string commonName)
		{
			string[] commonNameSplit = commonName.Split('(');
			string classAbbrev = commonNameSplit[0];

			if (classAbbrev.Length == 0)
			{
				if(commonNameSplit[1].Contains("sulf")) return LipidClass.Sulfatide;
			}

			LipidClass lipidClass;
			bool classFound = Enum.TryParse(classAbbrev, true, out lipidClass);

			if (!classFound)
			{
				// Add in any extra search criteria for classes that may not have straight forward parsing
				if (classAbbrev.Contains("Cer-2H2O")) return LipidClass.Cer2H2O;
				if (classAbbrev.Contains("Cer-H2O")) return LipidClass.CerH2O;
				if (classAbbrev.Contains("PIP2")) return LipidClass.PIP2;
				if (classAbbrev.Contains("PIP3")) return LipidClass.PIP3;
				if (classAbbrev.Contains("PIP")) return LipidClass.PIP;
				if (classAbbrev.Contains("cholest")) return LipidClass.Cholesterol;
				if (classAbbrev.Contains("sulf")) return LipidClass.Sulfatide;

				throw new SystemException("Unrecognized lipid class for " + commonName);
			}

			return lipidClass;
		}

		public static IEnumerable<AcylChain> ParseLipidCommonNameIntoAcylChains(string commonName)
		{
			MatchCollection matchCollection = Regex.Matches(commonName, "([mdtOP]-?)?\\d+:\\d+(\\(2OH\\))?");

			IEnumerable<AcylChain> acylChains = (from object match in matchCollection select new AcylChain(match.ToString()));
			return acylChains;
		}

		public static Composition ParseLipidCommonNameIntoCompositionWithoutAdduct(string commonName)
		{
			LipidClass lipidClass = ParseLipidCommonNameIntoClass(commonName);
			IEnumerable<AcylChain> fattyAcylChains = ParseLipidCommonNameIntoAcylChains(commonName);

			int numCarbons = fattyAcylChains.Sum(x => x.NumCarbons);
			int numDoubleBonds = fattyAcylChains.Sum(x => x.NumDoubleBonds);

			int numChains = fattyAcylChains.Count(x => x.NumCarbons > 0);
			bool containsEther = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Ether) > 0;
			bool containsPlasmogen = fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Plasmalogen) > 0;

			switch (lipidClass)
			{
				case LipidClass.PC:
					if (numChains > 1)
					{
						if (containsEther)
						{
							return new Composition(numCarbons + 8, (2 * (numCarbons + 8)) + 2 - (2 * numDoubleBonds), 1, 7, 0, 1);
						}
						else if (containsPlasmogen)
						{
							return new Composition(numCarbons + 8, (2*(numCarbons + 8)) + 0 - (2*numDoubleBonds), 1, 7, 0, 1);
						}
						else
						{
							return new Composition(numCarbons + 8, (2 * (numCarbons + 8)) + 0 - (2 * numDoubleBonds), 1, 8, 0, 1);
						}
					}
					else
					{
						if (containsEther)
						{
							return new Composition(numCarbons + 8, (2 * (numCarbons + 8)) + 4 - (2 * numDoubleBonds), 1, 6, 0, 1);
						}
						else if (containsPlasmogen)
						{
							return new Composition(numCarbons + 8, (2*(numCarbons + 8)) + 2 - (2*numDoubleBonds), 1, 6, 0, 1);
						}
						else
						{
							return new Composition(numCarbons + 8, (2 * (numCarbons + 8)) + 2 - (2 * numDoubleBonds), 1, 7, 0, 1);
						}
					}
					break;
				case LipidClass.PE:
					if (numChains > 1)
					{
						if (containsEther)
						{
							return new Composition(numCarbons + 5, (2 * (numCarbons + 5)) + 2 - (2 * numDoubleBonds), 1, 7, 0, 1);
						}
						else if (containsPlasmogen)
						{
							return new Composition(numCarbons + 5, (2*(numCarbons + 5)) + 0 - (2*numDoubleBonds), 1, 7, 0, 1);
						}
						else
						{
							return new Composition(numCarbons + 5, (2 * (numCarbons + 5)) + 0 - (2 * numDoubleBonds), 1, 8, 0, 1);
						}
					}
					else
					{
						if (containsEther)
						{
							return new Composition(numCarbons + 5, (2 * (numCarbons + 5)) + 4 - (2 * numDoubleBonds), 1, 6, 0, 1);
						}
						else if (containsPlasmogen)
						{
							return new Composition(numCarbons + 5, (2*(numCarbons + 5)) + 2 - (2*numDoubleBonds), 1, 6, 0, 1);
						}
						else
						{
							return new Composition(numCarbons + 5, (2 * (numCarbons + 5)) + 2 - (2 * numDoubleBonds), 1, 7, 0, 1);
						}
					}
					break;
				case LipidClass.PS:
					if (numChains > 1)
					{
						if (containsEther)
						{
							return new Composition(numCarbons + 6, (2*(numCarbons + 6)) + 0 - (2*numDoubleBonds), 1, 9, 0, 1);
						}
						else if (containsPlasmogen)
						{
							return new Composition(numCarbons + 6, (2*(numCarbons + 6)) - 2 - (2*numDoubleBonds), 1, 9, 0, 1);
						}
						else
						{
							return new Composition(numCarbons + 6, (2*(numCarbons + 6)) - 2 - (2*numDoubleBonds), 1, 10, 0, 1);
						}
					}
					else
					{
						return new Composition(numCarbons + 6, (2 * (numCarbons + 6)) + 0 - (2 * numDoubleBonds), 1, 9, 0, 1);
					}
					break;
				case LipidClass.PG:
					if (numChains > 1)
					{
						return new Composition(numCarbons + 6, (2*(numCarbons + 6)) - 1 - (2*numDoubleBonds), 0, 10, 0, 1);
					}
					else
					{
						return new Composition(numCarbons + 6, (2 * (numCarbons + 6)) + 1 - (2 * numDoubleBonds), 0, 9, 0, 1);
					}
					break;
				case LipidClass.Cer:
					if (fattyAcylChains.Count(x => x.AcylChainType == AcylChainType.Dihydroxy) > 0)
					{
						return new Composition(numCarbons, (2 * (numCarbons + 0)) + 1 - (2 * numDoubleBonds), 1, 4, 0, 0);	
					}
					else
					{
						return new Composition(numCarbons, (2 * (numCarbons + 0)) + 1 - (2 * numDoubleBonds), 1, 3, 0, 0);	
					}
					break;
				case LipidClass.CerH2O:
					return new Composition(numCarbons, (2 * (numCarbons + 0)) + 1 - (2 * numDoubleBonds), 1, 3, 0, 0) - Composition.H2O;
					break;
				case LipidClass.Cer2H2O:
					return new Composition(numCarbons, (2 * (numCarbons + 0)) + 1 - (2 * numDoubleBonds), 1, 3, 0, 0) - Composition.H2O - Composition.H2O;
					break;
				case LipidClass.SM:
					return new Composition(numCarbons + 5, (2 * (numCarbons + 5)) + 3 - (2 * numDoubleBonds), 2, 6, 0, 1);
					break;
				case LipidClass.GalCer:
				case LipidClass.GlcCer:
					return new Composition(numCarbons + 6, (2 * (numCarbons + 6)) - 1 - (2 * numDoubleBonds), 1, 8, 0, 0);
					break;
				case LipidClass.LacCer:
					return new Composition(numCarbons + 12, (2 * (numCarbons + 12)) - 1 - (2 * numDoubleBonds), 1, 13, 0, 0);
					break;
				case LipidClass.CerP:
					return new Composition(numCarbons, (2 * (numCarbons + 0)) + 2 - (2 * numDoubleBonds), 1, 6, 0, 1);
					break;
				case LipidClass.Cholesterol:
					return new Composition(27, 46, 0, 1, 0, 0);
					break;
				case LipidClass.CE:
					return new Composition(numCarbons + 27, (2 * (numCarbons + 27)) - 10 - (2 * numDoubleBonds), 0, 2, 0, 0);
					break;
				case LipidClass.Ubiquitones:
					break;
				case LipidClass.MG:
					return new Composition(numCarbons + 3, (2 * (numCarbons + 3)) + 0 - (2 * numDoubleBonds), 0, 4, 0, 0);
					break;
				case LipidClass.DG:
					return new Composition(numCarbons + 3, (2 * (numCarbons + 3)) - 2 - (2 * numDoubleBonds), 0, 5, 0, 0);
					break;
				case LipidClass.TG:
					return new Composition(numCarbons + 3, (2 * (numCarbons + 3)) - 4 - (2 * numDoubleBonds), 0, 6, 0, 0);
					break;
				case LipidClass.MGDG:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 4 - (2 * numDoubleBonds), 0, 10, 0, 0);
					break;
				case LipidClass.SQDG:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 4 - (2 * numDoubleBonds), 0, 12, 1, 0);
					break;
				case LipidClass.DGDG:
					return new Composition(numCarbons + 15, (2 * (numCarbons + 15)) - 6 - (2 * numDoubleBonds), 0, 15, 0, 0);
					break;
				case LipidClass.PI:
					if (numChains > 1)
					{
						return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 3 - (2 * numDoubleBonds), 0, 13, 0, 1);
					}
					else
					{
						return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 1 - (2 * numDoubleBonds), 0, 12, 0, 1);
					}
					break;
				case LipidClass.PIP:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 2 - (2 * numDoubleBonds), 0, 16, 0, 2);
					break;
				case LipidClass.PIP2:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 1 - (2 * numDoubleBonds), 0, 19, 0, 3);
					break;
				case LipidClass.PIP3:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) + 0 - (2 * numDoubleBonds), 0, 22, 0, 4);
					break;
				case LipidClass.PA:
					if (numChains > 1)
					{
						return new Composition(numCarbons + 3, (2 * (numCarbons + 3)) - 1 - (2 * numDoubleBonds), 0, 8, 0, 1);
					}
					else
					{
						return new Composition(numCarbons + 3, (2 * (numCarbons + 3)) + 1 - (2 * numDoubleBonds), 0, 7, 0, 1);
					}
					break;
				case LipidClass.CL:
					return new Composition(numCarbons + 9, (2 * (numCarbons + 9)) - 4 - (2 * numDoubleBonds), 0, 17, 0, 2);
					break;
				case LipidClass.Sulfatide:
					return new Composition(numCarbons + 6, (2 * (numCarbons + 6)) - 1 - (2 * numDoubleBonds), 1, 11, 1, 0);
					break;
			}

			throw new SystemException("No empirical formula calculator found for " + commonName);
		}

		public static List<MsMsSearchUnit> CreateMsMsSearchUnits(double precursorMz, LipidClass lipidClass, FragmentationMode fragmentationMode, IEnumerable<AcylChain> acylChainList)
		{
			List<MsMsSearchUnit> msMsSearchUnitList = new List<MsMsSearchUnit>();

			if (fragmentationMode == FragmentationMode.Positive)
			{
				if (lipidClass == LipidClass.PC)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 14, 1, 1, 0, 0).Mass, "C5H14ON"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 6, 0, 4, 0, 1).Mass, "C2H6O4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 2 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 0 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 4 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPC(O-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPC(O-)-H2O", acylChain));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						AcylChain combinedChain = new AcylChain(carbons + ":" + doubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG", combinedChain));
					}
				}
				else if (lipidClass == LipidClass.PE)
				{
					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					string displayC2H8NO4P = countOfChains > 1 ? "M-C2H8NO4P / DAG" : "M-C2H8NO4P";

					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 8, 1, 4, 0, 1).Mass, displayC2H8NO4P, true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPE(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPE(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).Mass, "ether", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).Mass, "ether-H2O", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PS)
				{
					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					string displayC3H8NO6P = countOfChains > 1 ? "M-C3H8NO6P / DAG" : "M-C3H8NO6P";

					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 8, 1, 6, 0, 1).Mass, displayC3H8NO6P, true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 9, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPS(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPS(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "ether", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "ether-H2O", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PG)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 12, 1, 6, 0, 1).Mass, "M-C3H12O6NP", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz + new Composition(0, 4, 1, 0, 0, 0).Mass, "M+NH4"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPG-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPG-H2O-H", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 2 - (2 * doubleBonds), 0, 9, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 0 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "plasmalogen (rearranged)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "plasmalogen (rearranged)-H2O", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPG(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPG(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
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

						AcylChain combinedChain = new AcylChain(carbons + ":" + doubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).Mass, "DAG", combinedChain));
					}
				}
				else if (lipidClass == LipidClass.Cer || lipidClass == LipidClass.CerH2O || lipidClass == LipidClass.Cer2H2O || lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer || lipidClass == LipidClass.LacCer || lipidClass == LipidClass.CerP || lipidClass == LipidClass.SM)
				{
					if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer || lipidClass == LipidClass.LacCer)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));
					}

					if (lipidClass == LipidClass.LacCer) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 22, 0, 11, 0, 0).Mass, "M-2(sugar)"));

					if (lipidClass == LipidClass.CerP)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "M-H3PO4"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 5, 0, 5, 0, 1).Mass, "M-H3PO4-H2O"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "M-HPO3"));
					}

					if (lipidClass == LipidClass.SM)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).Mass, "C5H15O4NP", true));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).Mass, "M-(CH2)3NH3"));
					}
					
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
					if (lipidClass != LipidClass.CerH2O && lipidClass != LipidClass.Cer2H2O) msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "M-2(H2O)"));

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
								if (lipidClass == LipidClass.SM) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB", acylChain, false));
								else msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB", acylChain, true));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 2 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB-CH2, acylChain"));
								break;
							case AcylChainType.Dihydroxy:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA+OH", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA+NH2", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) - 2 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "FA long", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "FA short", acylChain));
								break;
							case AcylChainType.Trihydro:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 1, 0, 0, 0).Mass, "LCB", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LCB+H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) + 4 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "LCB+2(H2O), acylChain"));
								break;
						}
					}

					if (countOfChains == 2)
					{
						int carbons = acylChainList.Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Sum(x => x.NumDoubleBonds);

						AcylChain combinedChain = new AcylChain(carbons + ":" + doubleBonds);

						if (lipidClass != LipidClass.Cer && lipidClass != LipidClass.CerH2O && lipidClass != LipidClass.Cer2H2O)
						{
							if (lipidClass == LipidClass.SM) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 0 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "both chains", combinedChain));
							msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "both chains - H2O", combinedChain));
						}
					}
				}
				else if (lipidClass == LipidClass.Cholesterol)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45", true));
				}
				else if (lipidClass == LipidClass.CE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(27, 45, 0, 0, 0, 0).Mass, "C27H45", true));

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
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 1, 0, 0, 0).Mass, "M-NH3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 5, 1, 1, 0, 0).Mass, "M-NH3-H2O"));

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;
						
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "FA", acylChain));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "[RCOO+58]-H2O", acylChain));

						if (lipidClass == LipidClass.DG)
						{
							msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, (2*carbons) + 3 - (2*doubleBonds), 1, 2, 0, 0).Mass, "M-RCOOH-NH3", acylChain));
						}
						else
						{
							msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "[RCOO+58]", acylChain));
							msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, (2 * carbons) + 3 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "M-RCOOH-NH3", acylChain));	
						}
					}
				}
				else if (lipidClass == LipidClass.MGDG || lipidClass == LipidClass.SQDG || lipidClass == LipidClass.DGDG)
				{
					if (lipidClass == LipidClass.MGDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 11, 0, 6, 0, 0).Mass, "M-C6H11O6", true));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 13, 0, 7, 0, 0).Mass, "M-C6H13O7 / DAG"));
					}
					else if (lipidClass == LipidClass.SQDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 15, 1, 8, 1, 0).Mass, "M-C6H11O8SNH4 / DAG", true));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 11, 0, 8, 1, 0).Mass, "M-C6H11O8S"));
					}
					else if (lipidClass == LipidClass.DGDG)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 21, 0, 11, 0, 0).Mass, "M-C12H21O11", true));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(12, 25, 1, 11, 0, 0).Mass, "M-C12H21O11NH4 / DAG"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));
					}

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
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "M-(acetate + methyl)", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(7, 16, 1, 5, 0, 1).Mass, "M-C7H16O5NP"));

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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 11, 1, 5, 0, 1).Mass, "C5H11O5NP", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 13, 1, 6, 0, 1).Mass, "C5H13O6NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(2, 6, 1, 0, 0, 0).Mass, "M-C2H6N"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(5, 12, 1, 5, 0, 1).Mass, "M-C5H12O5NP"));

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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 5)) - 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 5)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPE(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPE(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 6, 0, 1).Mass, "LPE(O-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 5, 0, 1).Mass, "LPE(O-)-H2O", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PI)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 8, 0, 1).Mass, "C6H10O8P", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 18, 0, 11, 0, 1).Mass, "C9H18O11P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(9, 16, 0, 10, 0, 1).Mass, "C9H16O10P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 8, 0, 7, 0, 1).Mass, "IP-2H2O-H"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 0, 0, 3, 0, 1).Mass, "PO3"));

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
					int countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);

					if (countOfChains > 1)
					{
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar"));
						msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(9, 17, 0, 10, 0, 1).Mass, "M-C9H17O10P"));
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								
								if (countOfChains == 1) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H / M-sugar", acylChain));
								else msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 12, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));

								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "LPI(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 10, 0, 1).Mass, "LPI(P-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));

								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) + 0 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "LPI(O-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 10, 0, 1).Mass, "LPI(O-)-H2O", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PIP || lipidClass == LipidClass.PIP2 || lipidClass == LipidClass.PIP3)
				{
					if (lipidClass == LipidClass.PIP) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 11, 0, 2).Mass, "C6H11O11P2", true));
					else if (lipidClass == LipidClass.PIP2) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 12, 0, 14, 0, 3).Mass, "C6H12O14P3", true));
					else if (lipidClass == LipidClass.PIP3) msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 14, 0, 17, 0, 4).Mass, "C6H14O17P4", true));
					
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 4, 0, 2, 0, 0).Mass, "M-2(H2O)"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 3, 0, 4, 0, 1).Mass, "M-H3O4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 1, 0, 3, 0, 1).Mass, "M-HPO3"));
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 1 - (2 * doubleBonds), 0, 0, 0, 0).Mass, "FA-CO2", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 3 - (2 * doubleBonds), 0, 14, 0, 2).Mass, "LPIP-H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 5 - (2 * doubleBonds), 0, 13, 0, 2).Mass, "LPIP-2H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 12, 0, 1).Mass, "M-Ketene", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 11, 0, 1).Mass, "M-FA", acylChain));

								if (lipidClass == LipidClass.PIP)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 1 - (2 * doubleBonds), 0, 15, 0, 2).Mass, "LPI-H", acylChain));
								}
								if (lipidClass == LipidClass.PIP3)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 2 - (2 * doubleBonds), 0, 17, 0, 3).Mass, "LPIP2-H2O-H", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 9, (2 * (carbons + 9)) - 4 - (2 * doubleBonds), 0, 16, 0, 3).Mass, "LPIP2-2H2O-H", acylChain));
								}
								break;
						}
					}

					int countOfChains = acylChainList.Count(x => x.NumCarbons > 0);

					if (countOfChains == 2)
					{
						int carbons = acylChainList.Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Sum(x => x.NumDoubleBonds);

						AcylChain combinedChain = new AcylChain(carbons + ":" + doubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "PA", combinedChain));
					}
				}
				else if (lipidClass == LipidClass.PG)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 10, 0, 6, 0, 1).Mass, "C6H10O6P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 6, 0, 5, 0, 1).Mass, "C3H6O5P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 6, 0, 2, 0, 0).Mass, "M-C3H6O2"));

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
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 0 - (2 * doubleBonds), 0, 9, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 2 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));

								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 2 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "LPG(O-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPG(O-)-H2O", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));

								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 0 - (2 * doubleBonds), 0, 8, 0, 1).Mass, "LPG(P-)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 2 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPG(P-)-H2O", acylChain));
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
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.PS)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 5, 1, 2, 0, 0).Mass, "M-C3H5O2N", true));
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
								
								if (countOfChains == 1)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));	
								}
								else if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "M-Ketene", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "M-FA", acylChain));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(P-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(P-)-H2O", acylChain));
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA(O-)", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).Mass, "LPA(O-)-H2O", acylChain));

								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 8, 0, 1).Mass, "LPS(O-", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 7, 0, 1).Mass, "LPS(O- - H2O", acylChain));
								}
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.Cer)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(1, 2, 0, 1, 0, 0).Mass, "M-CH2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(1, 4, 0, 1, 0, 0).Mass, "M-CH3OH"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).Mass, "M-H2O"));

					bool containsDiHydroxy = acylChainList.Count(x => x.AcylChainType == AcylChainType.Dihydroxy) > 0;

					if (containsDiHydroxy)
					{
						foreach (var acylChain in acylChainList)
						{
							int carbons = acylChain.NumCarbons;
							int doubleBonds = acylChain.NumDoubleBonds;

							// Ignore any 0:0 chains
							if (carbons == 0 && doubleBonds == 0) continue;

							switch (acylChain.AcylChainType)
							{
								case AcylChainType.Dihydro:
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 2 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "M-LBC (256)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "LBC (237)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 3 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "LBC (263)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LBC+amine", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 2 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "M-LBC (240)", acylChain));
									break;
								case AcylChainType.Standard:
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
									break;
								case AcylChainType.Dihydroxy:
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "FA with OH", acylChain));
									break;
								case AcylChainType.Trihydro:
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 4 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "M-(LBC+H2O) (274)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 4 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "M-(LBC+H2O) (258)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) + 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (255)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (281)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 4 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "LBC+amine+H2O", acylChain));
									break;
							}
						}
					}
					else
					{
						foreach (var acylChain in acylChainList)
						{
							int carbons = acylChain.NumCarbons;
							int doubleBonds = acylChain.NumDoubleBonds;

							// Ignore any 0:0 chains
							if (carbons == 0 && doubleBonds == 0) continue;

							switch (acylChain.AcylChainType)
							{
								case AcylChainType.Dihydro:
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 1, (2 * (carbons + 1)) + 1 - (2 * doubleBonds), 1, 3, 0, 0).Mass, "M-LBC (327)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 2 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "M-LBC (256)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 2 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "M-LBC (240)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) + 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "LBC (239)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) - 1 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "LBC (237)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 3 - (2 * doubleBonds), 0, 1, 0, 0).Mass, "LBC (263)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 1, (2 * (carbons - 1)) + 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "LBC+amine", acylChain));
									break;
								case AcylChainType.Standard:
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
									break;
								case AcylChainType.Trihydro:
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 1, (2 * (carbons + 1)) + 1 - (2 * doubleBonds), 1, 3, 0, 0).Mass, "M-LBC (327)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 4 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "M-(LBC+H2O) (274)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons - 2, (2 * (carbons - 2)) + 4 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "M-(LBC+H2O) (258)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) + 3 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "LBC+H2O (257)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons - 2, (2 * (carbons - 2)) + 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (255)", acylChain));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "LBC-(LBC+H2O) (281)", acylChain));
									break;
							}
						}
					}
				}
				else if (lipidClass == LipidClass.GlcCer || lipidClass == LipidClass.GalCer)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 10, 0, 5, 0, 0).Mass, "M-sugar"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(6, 12, 0, 6, 0, 0).Mass, "M-sugar-H2O"));

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2*carbons) - 1 - (2*doubleBonds), 0, 2, 0, 0).Mass, "FA", acylChain));
								break;
							case AcylChainType.Dihydroxy:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 3, 0, 0).Mass, "FA with OH", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.Sulfatide)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(0, 1, 0, 4, 1, 0).Mass, "HO4S", true));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 11, 0, 9, 1, 0).Mass, "sulfogalactosyl"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 9, 1, 0).Mass, "sulfogalactosyl-H2"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 8, 1, 0).Mass, "sulfogalactosyl-H20"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 14, 1, 9, 1, 0).Mass, "sulfogalactosyl+amine"));

					foreach (var acylChain in acylChainList)
					{
						int carbons = acylChain.NumCarbons;
						int doubleBonds = acylChain.NumDoubleBonds;

						// Ignore any 0:0 chains
						if (carbons == 0 && doubleBonds == 0) continue;

						switch (acylChain.AcylChainType)
						{
							case AcylChainType.Standard:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) - 2 - (2 * doubleBonds), 1, 1, 0, 0).Mass, "FA with amide", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 2 - (2 * doubleBonds), 1, 10, 1, 0).Mass, "lysoSulfogalactosyl with FA", acylChain));
								break;
							case AcylChainType.Dihydro:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 0 - (2 * doubleBonds), 1, 10, 1, 0).Mass, "lysoSulfogalactosyl with LBC", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 2 - (2 * doubleBonds), 1, 9, 1, 0).Mass, "lysoSulfogalactosyl with LBC - H20", acylChain));
								break;
							case AcylChainType.Dihydroxy:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) - 2 - (2 * doubleBonds), 1, 2, 0, 0).Mass, "FA with OH", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 2 - (2 * doubleBonds), 1, 11, 1, 0).Mass, "lysoSulfogalactosyl with OH", acylChain));
								break;
						}
					}
				}
				else if (lipidClass == LipidClass.CL)
				{
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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "FA+C3H6PO4", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 1 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "FA+C3H6PO4+H2O", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons, (2 * carbons) + 0 - (2 * doubleBonds), 0, 2, 0, 0).Mass, "M-FA", acylChain));
								break;
						}
					}

					List<AcylChain> acylChainsAsList = acylChainList.ToList();

					if (acylChainsAsList.Count == 4)
					{
						int carbonsOfFirstTwoAcylChains = acylChainsAsList[0].NumCarbons + acylChainsAsList[1].NumCarbons;
						int doubleBondsOfFirstTwoAcylChains = acylChainsAsList[0].NumDoubleBonds + acylChainsAsList[1].NumDoubleBonds;
						int carbonsOfSecondTwoAcylChains = acylChainsAsList[2].NumCarbons + acylChainsAsList[3].NumCarbons;
						int doubleBondsOfSecondTwoAcylChains = acylChainsAsList[2].NumDoubleBonds + acylChainsAsList[3].NumDoubleBonds;

						AcylChain firstTwoAcylChains = new AcylChain(carbonsOfFirstTwoAcylChains + ":" + doubleBondsOfFirstTwoAcylChains);
						AcylChain secondTwoAcylChains = new AcylChain(carbonsOfSecondTwoAcylChains + ":" + doubleBondsOfSecondTwoAcylChains);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 3, (2 * (carbonsOfFirstTwoAcylChains + 3)) - 2 - (2 * doubleBondsOfFirstTwoAcylChains), 0, 8, 0, 1).Mass, "PA", firstTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 3, (2 * (carbonsOfSecondTwoAcylChains + 3)) - 2 - (2 * doubleBondsOfSecondTwoAcylChains), 0, 8, 0, 1).Mass, "PA", secondTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, (2 * (carbonsOfFirstTwoAcylChains + 6)) - 2 - (2 * doubleBondsOfFirstTwoAcylChains), 0, 10, 0, 1).Mass, "PG", firstTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, (2 * (carbonsOfSecondTwoAcylChains + 6)) - 2 - (2 * doubleBondsOfSecondTwoAcylChains), 0, 10, 0, 1).Mass, "PG", secondTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, (2 * (carbonsOfFirstTwoAcylChains + 6)) - 4 - (2 * doubleBondsOfFirstTwoAcylChains), 0, 9, 0, 1).Mass, "PG-H2O", firstTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, (2 * (carbonsOfSecondTwoAcylChains + 6)) - 4 - (2 * doubleBondsOfSecondTwoAcylChains), 0, 9, 0, 1).Mass, "PG-H2O", secondTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, (2 * (carbonsOfFirstTwoAcylChains + 6)) - 1 - (2 * doubleBondsOfFirstTwoAcylChains), 0, 13, 0, 2).Mass, "PG+PO3", firstTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, (2 * (carbonsOfSecondTwoAcylChains + 6)) - 1 - (2 * doubleBondsOfSecondTwoAcylChains), 0, 13, 0, 2).Mass, "PG+PO3", secondTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfFirstTwoAcylChains + 6, (2 * (carbonsOfFirstTwoAcylChains + 6)) - 1 - (2 * doubleBondsOfFirstTwoAcylChains), 0, 12, 0, 2).Mass, "PG+PO2", firstTwoAcylChains));
						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbonsOfSecondTwoAcylChains + 6, (2 * (carbonsOfSecondTwoAcylChains + 6)) - 1 - (2 * doubleBondsOfSecondTwoAcylChains), 0, 12, 0, 2).Mass, "PG+PO2", secondTwoAcylChains));
					}
				}
				else if (lipidClass == LipidClass.SQDG)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(6, 9, 0, 7, 1, 0).Mass, "C6H9O7S", true));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 9, (2 * (carbons + 9)) - 3 - (2 * doubleBonds), 0, 11, 1, 0).Mass, "M-Ketene", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(carbons + 9, (2 * (carbons + 9)) - 1 - (2 * doubleBonds), 0, 10, 1, 0).Mass, "M-FA", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).Mass, "LPA-H", acylChain));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).Mass, "LPA-H2O-H", acylChain));
								
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
