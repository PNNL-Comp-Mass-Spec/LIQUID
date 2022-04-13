using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
using NUnit.Framework;

namespace LiquidTest
{
    public class GlobalWorkflowTests
    {
        // Ignore Spelling: Pos, workflow

        [Test]
        public void TestGlobalWorkflowPositive()
        {
            const string rawFileLocation = "../../../testFiles/Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05.raw";
            var globalWorkflow = new GlobalWorkflow(rawFileLocation);

            const string fileLocation = "../../../testFiles/Global_LipidMaps_Pos.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);

            var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

            var filteredLipidGroupSearchResults = new List<LipidGroupSearchResult>();

            // Group results of same scan together
            var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

            // Grab the result(s) with the best score
            foreach (var group in resultsGroupedByScan)
            {
                var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

                for (var i = 0; i < 1 && i < groupOrdered.Count; i++)
                {
                    var resultToAdd = groupOrdered[i];

                    if (resultToAdd.LipidTarget.LipidClass == LipidClass.PC &&
                        resultToAdd.LipidTarget.AcylChainList.Count(x => x.NumCarbons > 0) == 2 &&
                        resultToAdd.LipidTarget.AcylChainList.Count(x => x.AcylChainType == AcylChainType.Standard) == 2)
                    {
                        filteredLipidGroupSearchResults.Add(resultToAdd);
                    }
                }
            }

            if (File.Exists("fragmentOutput.csv")) File.Delete("fragmentOutput.csv");
            TextWriter textWriter = new StreamWriter("fragmentOutput.csv");

            LipidGroupSearchResultWriter.AddHeaderForScoring(filteredLipidGroupSearchResults[0], textWriter);
            LipidGroupSearchResultWriter.WriteToCsvForScoring(filteredLipidGroupSearchResults, textWriter, "Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05");

            // Assure that the source data file is closed
            globalWorkflow.LcMsRun.Close();

            textWriter.Close();
        }

        [Test]
        public void TestGlobalWorkflowNegative()
        {
            const string rawFileLocation = "../../../testFiles/Dey_Lipids_Top_2_3_rerun_Neg_05Jul13_Gimli_12-07-05.raw";
            var globalWorkflow = new GlobalWorkflow(rawFileLocation);

            const string fileLocation = "../../../testFiles/Global_LipidMaps_Neg.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);

            globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

            // Assure that the source data file is closed
            globalWorkflow.LcMsRun.Close();
        }

        [Test]
        public void TestMassCalibration()
        {
            const string rawFileLocation = "../../../testFiles/synaptosome_lipid_rafts_lipidomics_synlr_1_bottom__NEG_Polaroid_17Mar14_14-02-04.raw";
            var globalWorkflow = new GlobalWorkflow(rawFileLocation);

            const string fileLocation = "../../../testFiles/Global_LipidMaps_NEG_3.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);

            var massCalibrationResults = globalWorkflow.RunMassCalibration(lipidList, 50);
            Console.WriteLine(massCalibrationResults.PpmError);
            Console.WriteLine(massCalibrationResults.ErrorWidth);
        }

        [Test]
        public void TestCreateScoringOutput()
        {
            const string positiveTargetsFileLocation = "../../../testFiles/Global_LipidMaps_POS_v3.txt";
            var positiveTargetsFileInfo = new FileInfo(positiveTargetsFileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(positiveTargetsFileInfo);

            if (File.Exists("fragmentOutput.csv")) File.Delete("fragmentOutput.csv");
            TextWriter textWriter = new StreamWriter("fragmentOutput.csv");

            var datasetNames = new List<string>
            {
                //datasetNames.Add("Dey_lipids_Top_1_1_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Top_1_2_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Top_1_3_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_1_1_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_1_2_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_1_3_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Top_2_1_pos_dil_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Top_2_2_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Top_2_3_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_2_2_pos_Gimli_RZ-12-07-05");
                //datasetNames.Add("Dey_lipids_Bottom_2_3_pos_Gimli_RZ-12-07-05");
                "XGA121_lipid_Calu3_1",
                "XGA121_lipid_Calu3_2",
                "XGA121_lipid_Calu3_3",
                "XGA121_lipid_Skin_1",
                "XGA121_lipid_Skin_2",
                "XGA121_lipid_Skin_3",
                "XGA121_lipid_plasma_1",
                "XGA121_lipid_plasma_2",
                "XGA121_lipid_plasma_3",
                "Vero_01_CM_0d_4_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_CM_0d_2_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_CM_0d_3_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_CM_0d_1_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_MTBE_0d_4_Lipid_POS_Gimli_15Jan14_13-07-04",
                "Vero_01_MTBE_0d_3_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_MTBE_0d_2_Lipid_POS_Gimli_15Jan14_13-07-01",
                "Vero_01_MTBE_0d_1_Lipid_POS_Gimli_15Jan14_13-07-01",
                "LCA_Atta_B_gar2_b_Reruns_31May13_Gimli_12-07-01",
                "LCA_Atta_T_gar1_a1_Reruns_31May13_Gimli_12-07-01",
                "LCA_Atta_M_gar3_a_Reruns_31May13_Gimli_12-07-01",
                "Da_12_1_POS_3K_Gimli_9Oct13_13-07-01",
                "Da_24_1_POS_3K_Gimli_9Oct13_13-07-01",
                //datasetNames.Add("Lipid_QC_1_14Jan_POS_Gimli_14Jan14_13-07-01");
                //datasetNames.Add("Lipid_QC_1_14Jan_POS_Gimli_17JAN_13-07-01");
                "Daphnia_gut_TLE_POS_Gimli_21Jan14_13-07-01",
                "OMICS_HH_CDT_Lip_108_01_POS_Gimli_24Jan14_13-07-01",
                "OMICS_HH_CDT_Lip_108_02_POS_Gimli_24Jan14_13-07-01",
                "OMICS_HH_CDT_Lip_108_03_POS_Gimli_24Jan14_13-07-01",
                "Oscar_28days_TLE__POS_04Feb14_13-07-01",
                "Oscar_21days_TLE__POS_04Feb14_13-07-01",
                "Oscar_21days_dark_TLE__POS_04Feb14_13-07-01",
                "Oscar_14day_TLE__POS_04Feb14_13-07-01"
            };
            for (var datasetIndex = 0; datasetIndex < datasetNames.Count; datasetIndex++)
            {
                var datasetName = datasetNames[datasetIndex];
                var rawFileName = datasetName + ".raw";

                Console.WriteLine(DateTime.Now + ": Processing " + datasetName);

                if (File.Exists(rawFileName))
                {
                    Console.WriteLine(DateTime.Now + ": Dataset already exists");
                }
                else
                {
                    Console.WriteLine(DateTime.Now + ": Dataset does not exist locally, so we will go get it");

                    // Lookup the dataset directory in DMS
                    var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
                    var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                    var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

                    // Copy Locally
                    // TODO: Handle files that are on MyEMSL
                    Console.WriteLine(DateTime.Now + ": Copying dataset from " + dmsDirectoryInfo.FullName);
                    File.Copy(fullPathToDmsFile, rawFileName);
                    Console.WriteLine(DateTime.Now + ": Copy complete");
                }

                // Setup workflow
                var globalWorkflow = new GlobalWorkflow(rawFileName);

                // Run workflow
                var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

                var filteredLipidGroupSearchResults = new List<LipidGroupSearchResult>();

                // Group results of same scan together
                var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

                // Grab the result(s) with the best score
                foreach (var group in resultsGroupedByScan)
                {
                    var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

                    for (var i = 0; i < 1 && i < groupOrdered.Count; i++)
                    {
                        var resultToAdd = groupOrdered[i];

                        if (resultToAdd.LipidTarget.LipidClass == LipidClass.PC &&
                            resultToAdd.LipidTarget.AcylChainList.Count(x => x.NumCarbons > 0) == 2 &&
                            resultToAdd.LipidTarget.AcylChainList.Count(x => x.AcylChainType == AcylChainType.Standard) == 2)
                        {
                            filteredLipidGroupSearchResults.Add(resultToAdd);
                        }
                    }
                }

                // Output results
                if (datasetIndex == 0) LipidGroupSearchResultWriter.AddHeaderForScoring(filteredLipidGroupSearchResults[0], textWriter);
                LipidGroupSearchResultWriter.WriteToCsvForScoring(filteredLipidGroupSearchResults, textWriter, datasetName);

                // Assure that the source data file is closed
                globalWorkflow.LcMsRun.Close();
            }

            textWriter.Close();
        }

        public string GetRawFilePath(string directory, string datasetName)
        {
            var rawFileName = datasetName + ".raw";
            var rawFilePath = Path.Combine(directory, rawFileName);

            Console.WriteLine(DateTime.Now + ": Processing " + datasetName);

            if (File.Exists(rawFilePath))
            {
                Console.WriteLine(DateTime.Now + ": Dataset already exists");
            }
            else
            {
                try
                {
                    Console.WriteLine(DateTime.Now + ": Dataset does not exist locally, so we will go get it");

                    // Lookup the dataset directory in DMS
                    var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
                    var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                    var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

                    // Copy Locally
                    // TODO: Handle files that are on MyEMSL
                    Console.WriteLine(DateTime.Now + ": Copying dataset from " + dmsDirectoryInfo.FullName);
                    File.Copy(fullPathToDmsFile, rawFilePath);
                    Console.WriteLine(DateTime.Now + ": Copy complete");
                }
                catch (Exception)
                {
                    rawFilePath = string.Empty;
                    Console.WriteLine("Cannot get {0} from DMS", datasetName);
                }
            }

            return rawFilePath;
        }

        //[TestCase(@"D:\Data\Liquid\liquidFilesToRun.txt",                                                                           // Data list
        //          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b.txt",             // Pos target file
        //          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys.txt",      // Pos decoy file
        //          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4.txt",              // Neg target file
        //          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4_Decoys.txt",       // Neg decoy file
        //          30, 500)]
        [TestCase(@"D:\Data\Liquid\Original\liquidFilesToRun.txt",                                                                           // Data list
          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b.txt",             // Pos target file
          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys.txt",      // Pos decoy file
          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4.txt",              // Neg target file
          @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4_Decoys.txt",       // Neg decoy file
          30, 500)]
        public void RunTrainingOnFileList(
            string fileListPath,
            string posTargetFilePath,
            string posDecoyFilePath,
            string negTargetFilePath,
            string negDecoyFilePath,
            double hcdError = 30,
            double cidError = 500)
        {
            // Read positive target file
            var posTargetReader = new LipidMapsDbReader<Lipid>();
            var posTargets = posTargetReader.ReadFile(new FileInfo(posTargetFilePath));

            // Read positive decoy file
            var posDecoyReader = new LipidMapsDbReader<Lipid>();
            var posDecoys = posDecoyReader.ReadFile(new FileInfo(posDecoyFilePath));

            // Read positive target file
            var negTargetReader = new LipidMapsDbReader<Lipid>();
            var negTargets = negTargetReader.ReadFile(new FileInfo(negTargetFilePath));

            // Read positive decoy file
            var negDecoyReader = new LipidMapsDbReader<Lipid>();
            var negDecoys = negDecoyReader.ReadFile(new FileInfo(negDecoyFilePath));

            var outputDirectory = Path.GetDirectoryName(fileListPath);
            var errorFile = Path.Combine(outputDirectory, "failedDatasets.txt");

            foreach (var datasetName in File.ReadLines(fileListPath))
            {
                if (datasetName.StartsWith("//"))
                {
                    continue;
                }

                try
                {
                    // create output paths
                    var rawFilePath = GetRawFilePath(outputDirectory, datasetName);
                    var rawFileName = Path.GetFileName(rawFilePath);
                    var targetResultsPath = Path.Combine(outputDirectory, string.Format("{0}_target.tsv", datasetName));
                    var decoyResultsPath = Path.Combine(outputDirectory, string.Format("{0}_decoy.tsv", datasetName));

                    IEnumerable<Lipid> targets;
                    IEnumerable<Lipid> decoys;

                    // Select targets and decoys
                    var lowerCaseName = datasetName.ToLower();
                    if (lowerCaseName.Contains("pos"))
                    {
                        targets = posTargets;
                        decoys = posDecoys;
                    }
                    else
                    {
                        targets = negTargets;
                        decoys = negDecoys;
                    }

                    // Run liquid global workflow
                    var globalWorkflow = new GlobalWorkflow(rawFilePath);
                    var targetResults = GetBestResultPerSpectrum(globalWorkflow.RunGlobalWorkflow(targets, hcdError, cidError));
                    var decoyResults = GetBestResultPerSpectrum(globalWorkflow.RunGlobalWorkflow(decoys, hcdError, cidError));

                    // Output results
                    LipidGroupSearchResultWriter.OutputResults(targetResults, targetResultsPath, rawFileName);
                    LipidGroupSearchResultWriter.OutputResults(decoyResults, decoyResultsPath, rawFileName);

                    // Assure that the source data file is closed
                    globalWorkflow.LcMsRun.Close();
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR: Could not process dataset {0}.", datasetName);

                    using var streamWriter = new StreamWriter(errorFile, true);
                    streamWriter.WriteLine(datasetName);
                }
            }
        }

        private List<LipidGroupSearchResult> GetBestResultPerSpectrum(IEnumerable<LipidGroupSearchResult> results)
        {
            return results.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum?.ScanNum ?? x.SpectrumSearchResult.CidSpectrum.ScanNum)
                          .SelectMany(idGroup => idGroup.OrderByDescending(id => id.Score).Take(1))
                          .ToList();
        }

        //[TestCase(@"\\pnl\projects\MSSHARE\Jennifer_Kyle\BetaMarker\2015_Nov_raw\NEG\", @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4.txt", @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_NEG_4_Decoys.txt", 30, 500)]
        [TestCase(@"\\pnl\projects\MSSHARE\Jennifer_Kyle\BetaMarker\2015_Nov_raw\POS\", @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b.txt", @"C:\Users\wilk011\Documents\Projects\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys.txt", 30, 500)]
        public void RunTraining(string rawDirectoryPath, string targetFilePath, string decoyFilePath, double hcdError = 30, double cidError = 500)
        {
            // Read target file
            var targetReader = new LipidMapsDbReader<Lipid>();
            var targets = targetReader.ReadFile(new FileInfo(targetFilePath));

            // Read decoy file
            var decoyReader = new LipidMapsDbReader<Lipid>();
            var decoys = decoyReader.ReadFile(new FileInfo(decoyFilePath));

            var files = Directory.GetFiles(rawDirectoryPath);
            foreach (var rawFilePath in files.Where(file => file.EndsWith(".raw")))
            {
                // create output paths
                var rawFileName = Path.GetFileName(rawFilePath);
                var datasetPath = Path.GetDirectoryName(rawFilePath);
                var datasetName = Path.GetFileNameWithoutExtension(rawFilePath);
                var targetResultsPath = Path.Combine(datasetPath, string.Format("{0}_target.tsv", datasetName));
                var decoyResultsPath = Path.Combine(datasetPath, string.Format("{0}_decoy.tsv", datasetName));

                // Run liquid global workflow
                var globalWorkflow = new GlobalWorkflow(rawFilePath);
                var targetResults = globalWorkflow.RunGlobalWorkflow(targets, hcdError, cidError);
                var decoyResults = globalWorkflow.RunGlobalWorkflow(decoys, hcdError, cidError);

                // Output results
                LipidGroupSearchResultWriter.OutputResults(targetResults, targetResultsPath, rawFileName);
                LipidGroupSearchResultWriter.OutputResults(decoyResults, decoyResultsPath, rawFileName);

                // Assure that the source data file is closed
                globalWorkflow.LcMsRun.Close();
            }
        }
    }
}
