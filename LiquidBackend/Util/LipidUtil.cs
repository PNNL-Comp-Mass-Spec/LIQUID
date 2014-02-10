using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Sequence;
using LiquidBackend.Domain;

namespace LiquidBackend.Util
{
	public class LipidUtil
	{
		public static List<MsMsSearchUnit> CreateMsMsMsSearchUnits(double precursorMz, LipidClass lipidClass, FragmentationMode fragmentationMode, IEnumerable<AcylChain> acylChainList)
		{
			List<MsMsSearchUnit> msMsSearchUnitList = new List<MsMsSearchUnit>();

			if (fragmentationMode == FragmentationMode.Positive)
			{
				if (lipidClass == LipidClass.PC)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 15, 1, 4, 0, 1).GetMass(), "C5H15O4NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(3, 9, 1, 0, 0, 0).GetMass(), "Lipid-(CH2)3NH3"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(5, 14, 1, 1, 0, 0).GetMass(), "C5H14ON"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 6, 0, 4, 0, 1).GetMass(), "C2H6O4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).GetMass(), "Lipid-H2O"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 19, 1, 5, 0, 1).GetMass(), "C8H19O5NP"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(8, 21, 1, 6, 0, 1).GetMass(), "C8H21O6NP"));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).GetMass(), "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).GetMass(), "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).GetMass(), "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).GetMass(), "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "LPA-H2O-H"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).GetMass(), "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).GetMass(), "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).GetMass(), "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "LPC(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).GetMass(), "LPC(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 5 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "LPC(O-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 8, (2 * (carbons + 8)) + 3 - (2 * doubleBonds), 1, 5, 0, 1).GetMass(), "LPC(O-)-H2O"));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).GetMass(), "DAG"));
					}
				}
				else if (lipidClass == LipidClass.PE)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(2, 8, 1, 4, 0, 1).GetMass(), "C2H8NO4P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).GetMass(), "Lipid-H2O"));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).GetMass(), "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).GetMass(), "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).GetMass(), "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).GetMass(), "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "LPA-H2O-H"));
								if (countOfStandardAcylsChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 3 - (2 * doubleBonds), 1, 7, 0, 1).GetMass(), "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) + 1 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 4, 0, 1).GetMass(), "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 1 - (2 * doubleBonds), 1, 3, 0, 1).GetMass(), "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 1 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "LPE(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 5, (2 * (carbons + 5)) - 3 - (2 * doubleBonds), 1, 5, 0, 1).GetMass(), "LPE(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).GetMass(), "ether"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).GetMass(), "ether-H2O"));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).GetMass(), "DAG"));
					}
				}
				else if (lipidClass == LipidClass.PS)
				{
					msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(3, 8, 1, 0, 0, 1).GetMass(), "C3H8NO6P"));
					msMsSearchUnitList.Add(new MsMsSearchUnit(precursorMz - new Composition(0, 2, 0, 1, 0, 0).GetMass(), "Lipid-H2O"));

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
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons, (2 * carbons) - 1 - (2 * doubleBonds), 0, 1, 0, 0).GetMass(), "FA"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 1 - (2 * doubleBonds), 0, 3, 0, 0).GetMass(), "[RCOO+58]"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 2, 0, 0).GetMass(), "[RCOO+58]-H2O"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 7, 0, 1).GetMass(), "LPA-H"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "LPA-H2O-H"));
								if (countOfStandardAcylsChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) + 1 - (2 * doubleBonds), 1, 9, 0, 1).GetMass(), "Lipid-Ketene"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 1 - (2 * doubleBonds), 1, 8, 0, 1).GetMass(), "Lipid-FA"));
								}
								break;
							case AcylChainType.Plasmalogen:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "plasmalogen (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "plasmalogen (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 1, 6, 0, 1).GetMass(), "plasmalogen (rearranged)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 2 - (2 * doubleBonds), 1, 5, 0, 1).GetMass(), "plasmalogen (rearranged)-H2O"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 3 - (2 * doubleBonds), 1, 8, 0, 1).GetMass(), "LPS(P-)"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 6, (2 * (carbons + 6)) - 5 - (2 * doubleBonds), 1, 7, 0, 1).GetMass(), "LPS(P-)-H2O"));
								}
								break;
							case AcylChainType.Ether:
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 2 - (2 * doubleBonds), 0, 6, 0, 1).GetMass(), "ether (no head)"));
								msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) + 0 - (2 * doubleBonds), 0, 5, 0, 1).GetMass(), "ether (no head)-H2O"));
								if (countOfChains == 2)
								{
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 5 - (2 * doubleBonds), 1, 4, 0, 1).GetMass(), "ether"));
									msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 2, (2 * (carbons + 2)) + 3 - (2 * doubleBonds), 1, 3, 0, 1).GetMass(), "ether-H2O"));
								}
								break;
						}
					}

					if (countOfStandardAcylsChains == 2)
					{
						int carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
						int doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);

						msMsSearchUnitList.Add(new MsMsSearchUnit(new Composition(carbons + 3, (2 * (carbons + 3)) - 3 - (2 * doubleBonds), 0, 4, 0, 0).GetMass(), "DAG"));
					}
				}
			}
			else if (fragmentationMode == FragmentationMode.Negative)
			{
				
			}

			return msMsSearchUnitList;
		}
	}
}
