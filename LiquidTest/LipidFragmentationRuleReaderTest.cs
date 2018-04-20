using LiquidBackend.Domain;
using LiquidBackend.IO;
using NUnit.Framework;
using System;
using System.IO;

namespace LiquidTest
{
    [TestFixture()]
    public class LipidFragmentationRuleReaderTest
    {
        [Test()]
        public void TestLipidFragmentationRuleReader()
        {
            var targetsFilePath = @"/Users/leej324/Documents/projects/Liquid/LIQUID_Fragments_February2018_all_rules.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRuleReader = new LipidFragmentationRuleReader<LipidFragmentationRule>();
            var lipidFragmentationRules = lipidFragmentationRuleReader.ReadFile(targetsFileInfo);

            foreach (var rule in lipidFragmentationRules)
            {
                Console.WriteLine(rule.getComposition(3, 4).ToString());
            }
        }
    }
}
