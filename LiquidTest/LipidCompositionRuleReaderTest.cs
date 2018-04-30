using InformedProteomics.Backend.Data.Composition;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace LiquidTest
{
	[TestFixture]
	public class LipidCompositionRuleReaderTest
	{
		[Test]
		public void TestLipidCompositionRuleReader()
		{
			var lipidFilePath = @"C:\Users\fuji510\Desktop\LIQUID_REVISED\LIQUID_Subclass_chain_parsing_cleaned.txt";
			var lipidFileInfo = new FileInfo(lipidFilePath);
			var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
			var lipidCompositionRules = lipidCompositionRuleReader.ReadFile(lipidFileInfo);

			foreach (var rule in lipidCompositionRules)
			{
				Console.WriteLine(rule.ToString());
			}
		}

		[Test]
		public void TestGetCompositionRuleForLipid()
		{
			//Set lipid for testing
			string oldCommonName = "PC(16:0/0:0)";

			Console.WriteLine(oldCommonName);

			Composition oldComp = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(oldCommonName);
			var fattyAcylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(oldCommonName).ToList();
			var carbons = fattyAcylChains.Sum(x => x.NumCarbons);
			var dBonds = fattyAcylChains.Sum(x => x.NumDoubleBonds);
			Console.WriteLine("OLD COMPOSITION: " + oldComp.ToPlainString());

			var lipidFilePath = @"C:\Users\fuji510\Desktop\LIQUID_REVISED\LIQUID_Subclass_chain_parsing_cleaned.txt";
			var lipidFileInfo = new FileInfo(lipidFilePath);
			var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
			var lipidCompositionRules = lipidCompositionRuleReader.ReadFile(lipidFileInfo);

			foreach (var rule in lipidCompositionRules)
			{
				Composition newComp = rule.GetComposition(carbons, dBonds);
				if (newComp.Equals(oldComp))
				{
					Console.WriteLine(rule.ToString());
					Console.WriteLine(newComp.ToPlainString());
				}
			}
		}
	}
}
