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

        [Test()]
        public void TestGetCompositionRulesForCommonName()
        {
            var targetsFilePath = @"C:\Users\leej324\Downloads\LIQUID_UnitTest\LIQUID_composition_13Aug18_JK.txt";
            var lipidFileInfo = new FileInfo(targetsFilePath);
            var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
            var lipidCompositionRules = lipidCompositionRuleReader.ReadFile(lipidFileInfo);
            string[] commonNames = {
                "PC(18:0/F2IsoP-20:4)",
                "PE(18:0/F2IsoP-20:4)",
                "PE(18:0/20:4(OH))",
                "PE(18:0/20:4(OOH))",
                "PE(18:0/20:4(OOHOH))",
                "PC(18:0/20:4(OH))",
                "PC(18:0/20:4(OOH))",
                "PC(18:0/20:4(OOHOH))",
                "PI(18:0/20:4(OH))",
                "PI(18:0/20:4(OOH))",
                "PI(18:0/20:4(OOHOH))",
                "PI(18:0/F2IsoP-20:4)",
                "PS(18:0/20:4(OH))",
                "PS(18:0/20:4(OOH))",
                "PS(18:0/20:4(OOHOH))",
                "PS(18:0/F2IsoP-20:4)",
                "GlcCer(d18:1/12:0)",
                "GlcCer(d14:1(4E)/20:0(2OH))",
                "GalCer(d18:1/12:0)",
                "GalCer(d14:1(4E)/20:0(2OH))"};
            string[] formulae = {
                "C46H86NO11P",
                "C43H80NO11P",
                "C43H78NO9P",
                "C43H78NO10P",
                "C43H78NO11P",
                "C46H84NO9P",
                "C46H84NO10P",
                "C46H84NO11P",
                "C47H83O14P",
                "C47H83O15P",
                "C47H83O16P",
                "C47H85O16P",
                "C44H78NO11P",
                "C44H78NO12P",
                "C44H78NO13P",
                "C44H80NO13P",
                "C36H69NO8",
                "C40H77NO9",
                "C36H69NO8",
                "C40H77NO9"};
            for (var i = 0; i < commonNames.Length; i++)
            {
                string commonName = commonNames[i];
                string formula = formulae[i];

                string empiricalFormula1 = Composition.ParseFromPlainString(formula).ToPlainString();
                string empiricalFormula2 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName).ToPlainString();
                string empiricalFormula3 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdductUsingCompositionRules(commonName, lipidCompositionRules).ToPlainString();
                Console.WriteLine(commonName + "\t" + empiricalFormula1 + "\t" + empiricalFormula2 + "\t" + empiricalFormula3);
            }
        }

        [Test()]
        public void TestGetCompositionRulesForLipidClass()
        {
            var targetsFilePath = @"C:\Users\leej324\Downloads\LIQUID_UnitTest\LIQUID_composition_13Aug18_JK.txt";
            var lipidFileInfo = new FileInfo(targetsFilePath);
            var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
            var lipidCompositionRules = lipidCompositionRuleReader.ReadFile(lipidFileInfo);

            string commonName = "";
            string empiricalFormula1 = "";
            string empiricalFormula2 = "";
            string empiricalFormula3 = "";

            Console.WriteLine("================ POSITIVE ================");
            //FragmentationMode fragmentationMode = FragmentationMode.Positive;
            string[] lines = File.ReadAllLines(@"C:\Users\leej324\Downloads\LIQUID_UnitTest\Global_August2018_all_updated_ox_POS.txt");
            var numPosTargets = 0;
            var numCorrectPosTargets = 0;
            foreach (var line in lines)
            {
                try
                {
                    var tokens = line.Split('\t');
                    commonName = tokens[1];
                    if (!tokens[7].Equals("")) empiricalFormula1 = Composition.ParseFromPlainString(tokens[7]).ToPlainString();
                    else empiricalFormula1 = "";
                    empiricalFormula2 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName).ToPlainString();
                    empiricalFormula3 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdductUsingCompositionRules(commonName, lipidCompositionRules).ToPlainString();
                    var correct = false;
                    if (empiricalFormula1.Equals(""))
                    {
                        if (empiricalFormula2.Equals(empiricalFormula3)) correct = true;
                    }
                    else
                    {
                        //if (empiricalFormula1.Equals(empiricalFormula2) && empiricalFormula2.Equals(empiricalFormula3)) correct = true;
                        if (empiricalFormula1.Equals(empiricalFormula3)) correct = true;
                    }
                    numPosTargets += 1;
                    if (correct) numCorrectPosTargets += 1;
                    else Console.WriteLine(tokens[0] + "\t" +
                        tokens[1] + "\t" +
                        tokens[2] + "\t" +
                        tokens[7] + "\t" +
                        empiricalFormula2 + "\t" + empiricalFormula3);
                }
                catch
                {
                    Console.WriteLine(line);
                }
            }

            Console.WriteLine("================ NEGATIVE ================");
            //fragmentationMode = FragmentationMode.Negative;
            lines = File.ReadAllLines(@"C:\Users\leej324\Downloads\LIQUID_UnitTest\Global_August2018_all_updated_ox_NEG.txt");
            var numNegTargets = 0;
            var numCorrectNegTargets = 0;

            foreach (var line in lines)
            {
                try
                {
                    var tokens = line.Split('\t');
                    commonName = tokens[1];
                    if (!tokens[7].Equals("")) empiricalFormula1 = Composition.ParseFromPlainString(tokens[7]).ToPlainString();
                    else empiricalFormula1 = "";
                    empiricalFormula2 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdduct(commonName).ToPlainString();
                    empiricalFormula3 = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdductUsingCompositionRules(commonName, lipidCompositionRules).ToPlainString();
                    var correct = false;
                    if (empiricalFormula1.Equals(""))
                    {
                        if (empiricalFormula2.Equals(empiricalFormula3)) correct = true;
                    }
                    else
                    {
                        //if (empiricalFormula1.Equals(empiricalFormula2) && empiricalFormula2.Equals(empiricalFormula3)) correct = true;
                        if (empiricalFormula1.Equals(empiricalFormula3)) correct = true;
                    }
                    numNegTargets += 1;
                    if (correct) numCorrectNegTargets += 1;
                    else Console.WriteLine(tokens[0] + "\t" +
                        tokens[1] + "\t" +
                        tokens[2] + "\t" +
                        tokens[7] + "\t" +
                        empiricalFormula2 + "\t" + empiricalFormula3);
                }
                catch
                {
                    Console.WriteLine(line);
                }
            }

            Console.WriteLine("================ FINAL ================");
            Console.WriteLine("Positive: {0}/{1} ({2}%)", numCorrectPosTargets, numPosTargets, 100.0 * numCorrectPosTargets / numPosTargets);
            Console.WriteLine("Negative: {0}/{1} ({2}%)", numCorrectNegTargets, numNegTargets, 100.0 * numCorrectNegTargets / numNegTargets);
        }

        [Test]
        public void TestGetCompositionRule()
        {
            LipidRules.LoadLipidRules(@"C:\Users\leej324\source\repos\liquid\LiquidBackend\DefaultCompositionRules.txt", @"C:\Users\leej324\source\repos\liquid\LiquidBackend\DefaultFragmentationRules.txt");
            Console.WriteLine("Get Composition Rule");

            //var commonName = "DAT1(16:0/22:0(2Me[S],4Me[S]))";
            var commonName = "DAT2(18:0/23:0(2Me[S],3OH[S],4Me[S],6Me[S]))";

            string empiricalFormula = LipidUtil.ParseLipidCommonNameIntoCompositionWithoutAdductUsingCompositionRules(commonName, LipidRules.LipidCompositionRules).ToPlainString();
            Console.WriteLine(commonName + "\t" + empiricalFormula);
        }

        [Test]
        public void TestGetCompositionRuleForAllLipids()
        {
            Console.WriteLine("COMPOSITION RULES FOR ALL LIPIDS");
            var lipidFilePath = @"C:\Users\leej324\Downloads\LIQUID_UnitTest\LIQUID_Subclass_chain_parsing_clean.txt";
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
