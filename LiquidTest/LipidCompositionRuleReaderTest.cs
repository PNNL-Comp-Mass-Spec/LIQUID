using InformedProteomics.Backend.Data.Composition;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

		[Test]
		public void TestGetCompositionRuleForAllLipids()
		{
			Console.WriteLine("COMPOSITION RULES FOR ALL LIPIDS");
			var lipidFilePath = @"C:\Users\fuji510\Desktop\LIQUID_REVISED\LIQUID_Subclass_chain_parsing_cleaned_updated.txt";
			var lipidFileInfo = new FileInfo(lipidFilePath);
			var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
			var lipidCompositionRules = lipidCompositionRuleReader.ReadFile(lipidFileInfo);

			bool printMatches = false;
			bool printMissing = true;
			bool printFailed = false;

			List<int> percentage = new List<int>();

			foreach (var rule in lipidCompositionRules)
			{
				try
				{
					string oldCommonName = rule.Example;

					Composition oldComp = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(oldCommonName);
					var fattyAcylChains = LipidUtil.ParseLipidCommonNameIntoAcylChains(oldCommonName).ToList();
					var carbons = fattyAcylChains.Sum(x => x.NumCarbons);
					var dBonds = fattyAcylChains.Sum(x => x.NumDoubleBonds);

					Composition newComp = rule.GetComposition(carbons, dBonds);

					if (newComp.Equals(oldComp))
					{
						if (printMatches)
						{
							Console.WriteLine("----------------------------------------------------------------------------------------------------");
							Console.WriteLine("OLD COMMON NAME: " + oldCommonName + " COMPOSITION: " + oldComp.ToPlainString());
							Console.WriteLine(rule.LipidClass + "\t" + rule.LipidSubClass + "\t" + rule.Example + "\t" + newComp.ToPlainString());
						}
						percentage.Add(1);
					}
					else
					{
						if (printMissing)
						{
							Console.WriteLine("----------------------------------------------------------------------------------------------------");
							Console.WriteLine("OLD COMMON NAME: " + oldCommonName + " COMPOSITION: " + oldComp.ToPlainString());
							Console.WriteLine("------->NO MATCH<------- " + newComp.ToPlainString());
							Console.WriteLine("NEW EQUATIONS: C: " + rule.C.GetEquationString() + " H: " + rule.H.GetEquationString() + " N: " + rule.N.GetEquationString() +
								" O: " + rule.O.GetEquationString() + " S: " + rule.S.GetEquationString() + " P: " + rule.P.GetEquationString());
						}
						LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(oldCommonName);
						rule.GetComposition(carbons, dBonds);
						percentage.Add(0);
					}
				}
				catch(Exception e)
				{
					percentage.Add(0);
					if (printFailed)
					{
						Console.WriteLine("----------------------------------------------------------------------------------------------------");
						Console.WriteLine(rule.Example + " ------------------>" + e.Message);
					}
					continue;
				}
			}

			int sum = 0;
			foreach (var entry in percentage)
			{
				sum += entry;
			}
			double result = (double)sum / (double)percentage.Count * 100;
			Console.WriteLine("Percent Correct: " + result + "%");
		}
	}
}
