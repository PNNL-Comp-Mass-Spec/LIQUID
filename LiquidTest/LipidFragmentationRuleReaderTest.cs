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
    [TestFixture()]
    public class LipidFragmentationRuleReaderTest
    {
        [Test()]
        public void TestLipidFragmentationRuleReader()
        {
            const string targetsFilePath = "/Users/leej324/Documents/projects/Liquid/LIQUID_Fragments_February2018_all_rules.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRuleReader = new LipidFragmentationRuleReaderFromTable<LipidFragmentationRuleFromTable>();
            var lipidFragmentationRules = lipidFragmentationRuleReader.ReadFile(targetsFileInfo);

            foreach (var rule in lipidFragmentationRules)
            {
                Console.WriteLine(rule.GetComposition(3, 4).ToString());
            }
        }

        [Test()]
        public void TestGetFragmentationRulesForLipidSubClass()
        {
            const string targetsFilePath = "/Users/leej324/Documents/projects/Liquid/LIQUID_Fragments_February2018_all_rules.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRuleReader = new LipidFragmentationRuleReaderFromTable<LipidFragmentationRuleFromTable>();
            var lipidFragmentationRules = lipidFragmentationRuleReader.ReadFile(targetsFileInfo);

            var fragmentationRules = LipidUtil.GetFragmentationRulesForLipidSubClass("PC", FragmentationMode.Positive, lipidFragmentationRules);
            foreach (var rule in fragmentationRules)
            {
                Console.WriteLine(rule.ToString());
                Console.WriteLine(rule.GetComposition(3, 4).ToString());
            }
        }

        /**
        public static List<LipidFragmentationRule> GetFragmentationRulesForLipidClass(
            string lipidClass,
            FragmentationMode fragmentationMode,
            List<LipidFragmentationRule> lipidFragmentationRules)
        {
            var lipidFragmentationRulesList = new List<LipidFragmentationRule>();

            foreach (var rule in lipidFragmentationRules)
            {
                if (rule.fragmentationMode.Equals(fragmentationMode) &&
                    rule.lpidClass.Equals(lipidClass))
                {
                    lipidFragmentationRulesList.Add(rule);
                }
            }

            return lipidFragmentationRulesList;
        }
        

        public static List<MsMsSearchUnit> CreateMsMsSearchUnitsFromFragmentationRules(
            string commonName,
            double precursorMz,
            string lipidClass,
            FragmentationMode fragmentationMode,
            List<AcylChain> acylChainList,
            List<LipidFragmentationRule> lipidFragmentationRules)
        {
            var msMsSearchUnitList = new List<MsMsSearchUnit>();

            var countOfChains = acylChainList.Count(x => x.NumCarbons > 0);
            var countOfStandardAcylsChains = acylChainList.Count(x => x.AcylChainType == AcylChainType.Standard && x.NumCarbons > 0);
            var containsHydroxy = acylChainList.Count(x => x.AcylChainType == AcylChainType.Hydroxy) > 0 ? 1 : 0;

            //var carbons = (from chain in acylChainList select chain.NumCarbons).Sum();
            //var doubleBonds = (from chain in acylChainList select chain.NumDoubleBonds).Sum();
            //var acylChains = new AcylChain(string.Format("{0}:{1}", carbons, doubleBonds));

            var sialic = commonName.Split('(')[0][1];
            var sugar = commonName.Split('(')[0][0];

            foreach (var rule in lipidFragmentationRules)
            {
                if (rule.isFromHeader && rule.checkCountOfChains(countOfChains))
                {
                    msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz));
                }
                else if (rule.checkCountOfChains(countOfChains) && rule.targetAcylChainsIndices != null && rule.targetAcylChainsIndices.Count > 0)
                {
                    var carbons = 0;
                    var doubleBonds = 0;

                    if (rule.targetAcylChainsIndices.Count == 1)
                    {
                        var acylChain = acylChainList[rule.targetAcylChainsIndices[0] - 1];  // _idx should be from 1
                        msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz, acylChain.NumCarbons, acylChain.NumDoubleBonds, acylChain));
                    }
                    else
                    {
                        foreach (var _idx in rule.targetAcylChainsIndices)
                        {
                            var idx = _idx - 1;
                            if (acylChainList.Count > idx)
                            {
                                carbons += acylChainList[idx].NumCarbons;
                                doubleBonds += acylChainList[idx].NumDoubleBonds;
                            }
                        }
                        var combinedChain = new AcylChain(carbons + ":" + doubleBonds);
                        msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz, carbons, doubleBonds, combinedChain));
                    }
                }

                if (rule.useCountOfStandardAcylsChains(countOfStandardAcylsChains))
                {
                    var carbons = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumCarbons);
                    var doubleBonds = acylChainList.Where(x => x.AcylChainType == AcylChainType.Standard).Sum(x => x.NumDoubleBonds);
                    var combinedChain = new AcylChain(carbons + ":" + doubleBonds);
                    msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz, carbons, doubleBonds, combinedChain));
                }
                if (rule.sialic != null && rule.sialic.IndexOf(sialic) >= 0)
                {
                    msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz));
                }

            }

            foreach (var acylChain in acylChainList)
            {
                var numCarbons = acylChain.NumCarbons;
                var numDoubleBonds = acylChain.NumDoubleBonds;

                // Ignore any 0:0 chains
                if (numCarbons == 0 && numDoubleBonds == 0) continue;

                foreach (var rule in lipidFragmentationRules)
                {
                    if (rule.checkAcylChainConditions(acylChain.AcylChainType.ToString(),
                                                      numCarbons,
                                                      numDoubleBonds,
                                                      acylChain.HydroxyPosition,
                                                      countOfChains,
                                                      containsHydroxy,
                                                      sialic))
                    {
                        msMsSearchUnitList.Add(rule.GetMsMsSearchUnit(precursorMz, numCarbons, numDoubleBonds, acylChain));
                    }
                }
            }

            return msMsSearchUnitList;
        }
        **/

        public bool CheckFragmentaionRules(string commonName,
                                            string empiricalFormula,
                                            FragmentationMode fragmentationMode,
                                            List<LipidFragmentationRule> lipidFragmentationRules)
        {
            var lipidTarget = LipidUtil.CreateLipidTarget(commonName, empiricalFormula, fragmentationMode);

            Console.WriteLine(commonName + "\t" + empiricalFormula);

            var msMsSearchUnitListNew = LipidUtil.CreateMsMsSearchUnitsFromFragmentationRules(commonName, lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList, lipidFragmentationRules);

            var newResults = "";
            foreach (var msMsSearchUnit in msMsSearchUnitListNew.OrderBy(x => x.Mz))
            {
                newResults += msMsSearchUnit.ToString() + "\n";
            }

            var msMsSearchUnitListOld = LipidUtil.CreateMsMsSearchUnits(lipidTarget.CommonName, lipidTarget.Composition.Mass, lipidTarget.LipidClass, fragmentationMode, lipidTarget.AcylChainList);

            var oldResults = "";
            foreach (var msMsSearchUnit in msMsSearchUnitListOld.OrderBy(x => x.Mz))
            {
                oldResults += msMsSearchUnit.ToString() + "\n";
            }

            if (!oldResults.Equals(newResults))
            {
                Console.WriteLine("============ DIFF NEW ================");
                Console.WriteLine(newResults);
                Console.WriteLine("============ DIFF OLD ================");
                Console.WriteLine(oldResults);
            }

            Console.WriteLine("==================================");

            return oldResults.Equals(newResults);
        }

        [Test()]
        public void TestGetFragmentationRules()
        {
            const string targetsFilePath = @"C:\Users\leej324\Downloads\LIQUID_UnitTest\extract_rules_from_code_equations_fixed.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRulesReader = new LipidFragmentationRuleReader<LipidFragmentationRule>();
            var lipidFragmentationRules = lipidFragmentationRulesReader.ReadFile(targetsFileInfo);

            //string commonName = "WE(23:0/16:1(9Z))";
            //string empiricalFormula = "C39H76O2";
            //string commonName = "PC(16:0/11:0(CHO))";
            //string empiricalFormula = "C35H68N1O9P1";
            const string commonName = "GM3(d14:1/24:1)";
            const string empiricalFormula = "C61H110N2O21";
            const FragmentationMode fragmentationMode = FragmentationMode.Negative;

            CheckFragmentaionRules(commonName, empiricalFormula, fragmentationMode, lipidFragmentationRules);
        }

        [Test()]
        public void TestGetFragmentationRulesForLipidClass()
        {
            const string targetsFilePath = @"C:\Users\leej324\Downloads\LIQUID_UnitTest\extract_rules_from_code_equations_fixed.txt";
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidFragmentationRulesReader = new LipidFragmentationRuleReader<LipidFragmentationRule>();
            var lipidFragmentationRules = lipidFragmentationRulesReader.ReadFile(targetsFileInfo);

            Console.WriteLine("================ POSITIVE ================");
            var fragmentationMode = FragmentationMode.Positive;
            var lines = File.ReadAllLines(@"C:\Users\leej324\Downloads\LIQUID_UnitTest\Global_April2018_all_POS.txt");
            var numPosTargets = 0;
            var numCorrectPosTargets = 0;
            foreach (var line in lines)
            {
                try
                {
                    var tokens = line.Split('\t');
                    var commonName = tokens[1];
                    var empiricalFormula = tokens[7];
                    if (empiricalFormula.Equals("")) empiricalFormula = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName).ToPlainString();
                    var correct = CheckFragmentaionRules(commonName, empiricalFormula, fragmentationMode, lipidFragmentationRules);
                    numPosTargets++;
                    if (correct) numCorrectPosTargets++;
                }
                catch
                {
                    Console.WriteLine(line);
                }
            }

            Console.WriteLine("================ NEGATIVE ================");
            fragmentationMode = FragmentationMode.Negative;
            lines = File.ReadAllLines(@"C:\Users\leej324\Downloads\LIQUID_UnitTest\Global_April2018_all_NEG.txt");
            var numNegTargets = 0;
            var numCorrectNegTargets = 0;

            foreach (var line in lines)
            {
                try
                {
                    var tokens = line.Split('\t');
                    var commonName = tokens[1];
                    var empiricalFormula = tokens[7];
                    if (empiricalFormula.Equals("")) empiricalFormula = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName).ToPlainString();
                    var correct = CheckFragmentaionRules(commonName, empiricalFormula, fragmentationMode, lipidFragmentationRules);
                    numNegTargets++;
                    if (correct) numCorrectNegTargets++;
                }
                catch
                {
                    Console.WriteLine(line);
                }
            }

            Console.WriteLine("================ FINAL ================");
            Console.WriteLine("Positive: {0}/{1} ({2}%)", numCorrectPosTargets, numPosTargets, 100.0 * numCorrectPosTargets/numPosTargets);
            Console.WriteLine("Negative: {0}/{1} ({2}%)", numCorrectNegTargets, numNegTargets, 100.0 * numCorrectNegTargets/numNegTargets);
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
