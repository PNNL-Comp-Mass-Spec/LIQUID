using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
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
                Console.WriteLine(rule.GetComposition(3, 4).ToString());
            }
        }

        [Test()]
        public void TestGetFragmentationRulesForLipidSubClass()
        {
            var targetsFilePath = @"/Users/leej324/Documents/projects/Liquid/LIQUID_Fragments_February2018_all_rules.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRuleReader = new LipidFragmentationRuleReader<LipidFragmentationRule>();
            var lipidFragmentationRules = lipidFragmentationRuleReader.ReadFile(targetsFileInfo);

            var fragmentationRules = LipidUtil.GetFragmentationRulesForLipidSubClass("PC", FragmentationMode.Positive, lipidFragmentationRules);
            foreach (var rule in fragmentationRules)
            {
                Console.WriteLine(rule.ToString());
                Console.WriteLine(rule.GetComposition(3, 4).ToString());
            }
        }

        //[Test]
        //public void TestCreateMsMsSearchUnits()
        //{
        //    const FragmentationMode fragmentationMode = FragmentationMode.Positive;

        //    var commonName = "PC(16:0/18:1(9Z))";
        //    var empiricalFormula = "C42H83NO8P";
        //    var lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);
        //    var msMsSearchUnitList = LipidUtil.CreateMsMsSearchUnits(lipidTarget.CommonName, lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);
        //    Console.WriteLine(commonName + "\t" + empiricalFormula);
        //    foreach (var msMsSearchUnit in msMsSearchUnitList.OrderBy(x => x.Mz))
        //    {
        //        Console.WriteLine(msMsSearchUnit);
        //    }
        //    Console.WriteLine("**************************************************************************************");
        //}
    }
}
