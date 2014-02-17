using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using NUnit.Framework;

namespace LiquidTest
{
	public class LipidUtilTests
	{
		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsTwoChains()
		{
			const string commonName = "PC(16:0/18:1)";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("16:0"), acylChains);
			Assert.Contains(new AcylChain("18:1"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsTwoChainsOnePlasmogen()
		{
			const string commonName = "PC(P-16:0/18:1)";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("P-16:0"), acylChains);
			Assert.Contains(new AcylChain("18:1"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsTwoChainsOneEther()
		{
			const string commonName = "PC(O-16:0/18:1)";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("O-16:0"), acylChains);
			Assert.Contains(new AcylChain("18:1"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsTwoChainsOneDihydro()
		{
			const string commonName = "SM(d18:1/25:0)";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("d18:1"), acylChains);
			Assert.Contains(new AcylChain("25:0"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsThreeChains()
		{
			const string commonName = "TG(12:0/16:0/18:0)";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("12:0"), acylChains);
			Assert.Contains(new AcylChain("16:0"), acylChains);
			Assert.Contains(new AcylChain("18:0"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoAcylChainsTwoChainsDoubleBondPositions()
		{
			const string commonName = "PE(18:3(6Z,9Z,12Z)/20:5(5Z,8Z,11Z,14Z,17Z))";
			List<AcylChain> acylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(commonName).ToList();

			Assert.Contains(new AcylChain("18:3"), acylChains);
			Assert.Contains(new AcylChain("20:5"), acylChains);
		}

		[Test]
		public void TestParseLipidCommonNameIntoClass()
		{
			const string commonName = "PE(18:3(6Z,9Z,12Z)/20:5(5Z,8Z,11Z,14Z,17Z))";
			LipidClass lipidClass = LipidUtil.ParseLipidCommonNameIntoClass(commonName);

			Assert.AreEqual(LipidClass.PE, lipidClass);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForAllPositive()
		{
			const FragmentationMode fragmentationMode = FragmentationMode.Positive;

			string commonName;
			string empiricalFormula;
			LipidTarget lipidTarget;
			List<MsMsSearchUnit> msMsSearchUnitList;

			commonName = "PC(16:0/18:1(9Z))";
			empiricalFormula = "C42H83NO8P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(O-16:0/18:1(9Z))";
			empiricalFormula = "C42H85NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(P-16:0/18:1(9Z))";
			empiricalFormula = "C42H83NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(16:0/0:0)";
			empiricalFormula = "C24H51NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(P-16:0/0:0)";
			empiricalFormula = "C24H51NO6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(O-16:0/0:0)";
			empiricalFormula = "C24H53NO6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(16:0/18:1)";
			empiricalFormula = "C39H77NO8P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(O-16:0/18:1(9Z))";
			empiricalFormula = "C39H79NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(P-16:0/18:1(9Z))";
			empiricalFormula = "C39H77NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(16:0/0:0)";
			empiricalFormula = "C21H45NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(O-16:0/0:0)";
			empiricalFormula = "C21H47NO6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(P-16:0/0:0)";
			empiricalFormula = "C21H45NO6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(16:0/18:1)";
			empiricalFormula = "C40H77NO10P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(16:0/0:0)";
			empiricalFormula = "C22H45NO9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(P-16:0/18:1(9Z))";
			empiricalFormula = "C40H77NO9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(O-16:0/18:1(9Z))";
			empiricalFormula = "C40H79NO9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PG(16:0/18:1)";
			empiricalFormula = "C40H78O10P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PG(16:0/0:0)";
			empiricalFormula = "C22H46O9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "Cer(d18:1/25:0)";
			empiricalFormula = "C43H86NO3";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "SM(d18:1/25:0)";
			empiricalFormula = "C48H98N2O6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "GlcCer(d18:1/12:0)";
			empiricalFormula = "C36H70NO8";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "LacCer(d18:1/12:0)";
			empiricalFormula = "C42H80NO13";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "CerP(d18:1/12:0)";
			empiricalFormula = "C30H61NO6P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "CE(18:1(9Z))";
			empiricalFormula = "C45H82NO2";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "MG(16:0/0:0/0:0)";
			empiricalFormula = "C19H42NO4";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "DG(16:0/18:0/0:0)";
			empiricalFormula = "C37H76NO5";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "TG(16:0/18:0/22:0)";
			empiricalFormula = "C59H118NO6";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "MGDG(20:5(5Z,8Z,11Z,14Z,17Z)/18:3(9Z,12Z,15Z))";
			empiricalFormula = "C47H78NO10";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "SQDG(16:0/14:0)";
			empiricalFormula = "C39H78NO12S";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "DGDG(18:5(3Z,6Z,9Z,12Z,15Z)/18:4(6Z,9Z,12Z,15Z))";
			empiricalFormula = "C51H82NO15";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForAllNegative()
		{
			const FragmentationMode fragmentationMode = FragmentationMode.Negative;

			string commonName;
			string empiricalFormula;
			LipidTarget lipidTarget;
			List<MsMsSearchUnit> msMsSearchUnitList;

			commonName = "PC(16:0/18:1(9Z))";
			empiricalFormula = "C42H81NO8P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PC(16:0/0:0)";
			empiricalFormula = "C24H49NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(16:0/18:1)";
			empiricalFormula = "C39H75NO8P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(P-16:0/18:1(9Z))";
			empiricalFormula = "C39H75NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PE(16:0/0:0)";
			empiricalFormula = "C21H43NO7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PI(16:0/18:1)";
			empiricalFormula = "C43H80O13P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PI(16:0/0:0)";
			empiricalFormula = "C25H48O12P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PIP[3'](17:0/20:4(5Z,8Z,11Z,14Z))";
			empiricalFormula = "C46H81O16P2";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PIP2[3',5'](17:0/20:4(5Z,8Z,11Z,14Z))";
			empiricalFormula = "C46H82O19P3";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PIP3[3',4',5'](17:0/20:4(5Z,8Z,11Z,14Z))";
			empiricalFormula = "C46H83O22P4";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PG(16:0/18:1)";
			empiricalFormula = "C40H76O10P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PG(16:0/0:0)";
			empiricalFormula = "C22H44O9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PA(16:0/18:1)";
			empiricalFormula = "C37H70O8P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PA(16:0/0:0)";
			empiricalFormula = "C19H38O7P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(16:0/18:1)";
			empiricalFormula = "C40H75NO10P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(P-16:0/18:1(9Z))";
			empiricalFormula = "C40H75NO9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");

			commonName = "PS(16:0/0:0)";
			empiricalFormula = "C22H43NO9P";
			lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
			msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
			Console.WriteLine(commonName + "\t" + empiricalFormula);
			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}
			Console.WriteLine("**************************************************************************************");
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPcPositive()
		{
			// Testing PC(16:0/18:1) +H
			double precursorMz = 760.5855988;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(239.237490715, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(295.263705485, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(313.274270185, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(391.22495104, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(409.23551574, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(478.329750495, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(496.340315195, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(504.345400565, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(522.355965265, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(577.519585935, "DAG"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(701.512099485, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(742.5750341, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPcEtherPositive()
		{
			// Testing PC(O-16:0/18:1) +H
			double precursorMz = 746.606333;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("O-16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(377.24568648, "ether (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(395.25625118, "ether (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(464.350485935, "LPC(O-)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(482.361050635, "LPC(O-)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(504.345400565, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(522.355965265, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(687.532833685, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(728.5957683, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPcPlasmalogenPositive()
		{
			// Testing PC(P-16:0/18:1) +H
			double precursorMz = 744.5906838;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("P-16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(375.23003641, "plasmalogen (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(388.298056395, "plasmalogen (rearranged)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(393.24060111, "plasmalogen (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(406.308621095, "plasmalogen (rearranged)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(458.303535725, "LPC(P-)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(476.314100425, "LPC(P-)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(504.345400565, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(522.355965265, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(685.517184485, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(726.5801191, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpcPositive()
		{
			// Testing PC(16:0/0:0) +H
			double precursorMz = 496.3402966;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(239.237490715, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(295.263705485, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(313.274270185, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(391.22495104, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(409.23551574, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(437.266797285, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(478.3297319, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpcPlasmalogenPositive()
		{
			// Testing PC(P-16:0/0:0) +H
			double precursorMz = 480.3453816;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("P-16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(375.23003641, "plasmalogen (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(393.24060111, "plasmalogen (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(421.271882285, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(462.3348169, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpcEtherPositive()
		{
			// Testing PC(O-16:0/0:0) +H
			double precursorMz = 482.3610308;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("O-16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PC, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(104.10753912, "C5H14ON"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(125.00037073, "C2H6O4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(184.073870045, "C5H15O4NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(240.100084815, "C8H19O5NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(258.110649515, "C8H21O6NP"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(377.24568648, "ether (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(395.25625118, "ether (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(423.287531485, "Lipid-(CH2)3NH3"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(464.3504661, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPePositive()
		{
			// Testing PE(16:0/18:1) +H
			double precursorMz = 718.5386512;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(239.237490715, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(295.263705485, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(313.274270185, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(391.22495104, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(409.23551574, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(436.282800285, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(454.293364985, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(462.298450355, "Lipid-FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(480.309015055, "Lipid-Ketene"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(577.519585935, "DAG"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(700.5280865, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPeEtherPositive()
		{
			// Testing PE(O-16:0/18:1) +H
			double precursorMz = 704.5593854;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("O-16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(348.266756255, "ether-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(377.24568648, "ether (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(366.277320955, "ether"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(395.25625118, "ether (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(686.5488207, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForPePlasmalogenPositive()
		{
			// Testing PE(P-16:0/18:1) +H
			double precursorMz = 702.5437362;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("P-16:0"), new AcylChain("18:1") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(265.253140785, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(321.279355555, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(339.289920255, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(346.251106185, "plasmalogen (rearranged)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(364.261670885, "plasmalogen (rearranged)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(375.23003641, "plasmalogen (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(393.24060111, "plasmalogen (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(416.256585515, "LPE(P-)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(417.24060111, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(434.267150215, "LPE(P-)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(435.25116581, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(684.5331715, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpePositive()
		{
			// Testing PE(16:0/0:0) +H
			double precursorMz = 454.293349;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(239.237490715, "FA"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(295.263705485, "[RCOO+58]-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(313.274270185, "[RCOO+58]"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(391.22495104, "LPA-H2O-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(409.23551574, "LPA-H"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(436.2827843, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpePlasmalogenPositive()
		{
			// Testing PE(P-16:0/0:0) +H
			double precursorMz = 438.298434;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("P-16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(375.23003641, "plasmalogen (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(393.24060111, "plasmalogen (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(420.2878693, "Lipid-H2O"), msMsSearchUnitList);
		}

		[Test]
		public void TestCreateMsMsMsSearchUnitsForLpeEtherPositive()
		{
			// Testing PE(O-16:0/0:0) +H
			double precursorMz = 440.3140832;
			FragmentationMode fragmentationMode = FragmentationMode.Positive;
			List<AcylChain> acylChainList = new List<AcylChain> { new AcylChain("O-16:0"), new AcylChain("0:0") };

			List<MsMsSearchUnit> msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(precursorMz, LipidClass.PE, fragmentationMode, acylChainList);

			foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
			{
				Console.WriteLine(msMsSearchUnit);
			}

			Assert.Contains(new MsMsSearchUnit(141.0190948, "C2H8NO4P"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(377.24568648, "ether (no head)-H2O"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(395.25625118, "ether (no head)"), msMsSearchUnitList);
			Assert.Contains(new MsMsSearchUnit(422.3035185, "Lipid-H2O"), msMsSearchUnitList);
		}
	}
}
