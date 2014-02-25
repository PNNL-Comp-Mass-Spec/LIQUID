using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using NUnit.Framework;

namespace LiquidTest
{
	public class LipidIOTests
	{
		[Test]
		public void TestReadLipidMaps()
		{
			string fileLocation = @"../../../testFiles/Global_LipidMaps_Pos.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);
			Console.WriteLine(lipidList.Count);
		}

		[Test]
		public void TestReadLipidMapsAndCreateTargets()
		{
			string fileLocation = @"../../../testFiles/Global_LipidMaps_Pos.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);
			Console.WriteLine(lipidList.Count);

			foreach (Lipid lipid in lipidList)
			{
				LipidTarget lipidTarget = lipid.LipidTarget;
			}
		}

		[Test]
		public void TestReadLipidMapsNegativeAndCreateTargets()
		{
			string fileLocation = @"../../../testFiles/Global_LipidMaps_Neg.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);
			Console.WriteLine(lipidList.Count);

			foreach (Lipid lipid in lipidList)
			{
				LipidTarget lipidTarget = lipid.LipidTarget;
			}
		}
	}
}
