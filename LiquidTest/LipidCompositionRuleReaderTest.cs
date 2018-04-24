using LiquidBackend.Domain;
using LiquidBackend.IO;
using NUnit.Framework;
using System;
using System.IO;

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
	}
}
