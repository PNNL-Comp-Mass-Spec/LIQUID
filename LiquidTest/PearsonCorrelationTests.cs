using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using NUnit.Framework;

namespace LiquidTest
{
    using LiquidBackend.Util;

    [TestFixture]
    class PearsonCorrelationTests
    {
        [TestCase(14, "PS(18:0/18:1)", "C42H81N1O10P1", 30)]
        public void TestPearsonCorrelation(int precursor, string commonName, string composition, Tolerance tolerance)
        {
            var rawFilePath = @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw";

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            var parsedComposition = Composition.ParseFromPlainString(composition);
            var correlationCalculator = new PearsonCorrelationFitUtil();
            //var target = new LipidTarget(commonName, LipidClass.DG, FragmentationMode.Positive, parsedComposition, new List<AcylChain>(), Adduct.Hydrogen);
            var spectrumSearchResult = new SpectrumSearchResult(null, null, spectrum, null, null, new Xic(), lcmsRun) { PrecursorTolerance = tolerance };
            var correlation = correlationCalculator.GetFitScore(spectrumSearchResult, parsedComposition);
            Console.WriteLine("The Pearson correlation is: " + correlation);
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

        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\LiquidVerifiedOutputFiles\Positive\")]
        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\LiquidVerifiedOutputFiles\Negative\")]
        public void PearsonCorrelationFileCombiner(string directoryPath)
        {
            var dirFiles = Directory.GetFiles(directoryPath);

            var correlationCalculator = new PearsonCorrelationFitUtil();
            var cosineCalculator = new CosineFitUtil();

            // Each dictionary corresponds to a dataset, each dictionary key corresponds to the TSV header.
            var results = new List<Dictionary<string, List<string>>>();
            var headers = new HashSet<string>();
            foreach (var pathToResults in dirFiles.Where(path => path.EndsWith(".txt")))
            {
                var datasetName = Path.GetFileNameWithoutExtension(pathToResults);
                var pathToRaw = GetRawFilePath(directoryPath, datasetName);
                var rawName = Path.GetFileName(pathToRaw);
                if (string.IsNullOrEmpty(pathToRaw))
                {
                    continue;
                }

                var lcmsRun = PbfLcMsRun.GetLcMsRun(pathToRaw);
                var tolerance = new Tolerance(30, ToleranceUnit.Ppm);
                using (var reader = new StreamReader(new FileStream(pathToResults, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    results.Add(new Dictionary<string, List<string>>()); // Add dictionary for new dataset.
                    var datasetResults = results.Last(); // Results for the current dataset.
                    var lineCount = 0;
                    var headerToIndex = new Dictionary<string, int>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var pieces = line.Split('\t').ToArray();

                        if (lineCount++ == 0)
                        {   // First line

                            for (var i = 0; i < pieces.Length; i++)
                            {
                                var header = pieces[i];
                                headerToIndex.Add(header, i);
                                datasetResults.Add(header, new List<string>());
                            }

                            datasetResults.Add("Raw File", new List<string>());
                            datasetResults.Add("Pearson Corr Score", new List<string>());
                            datasetResults.Add("Pearson Corr M-1 Score", new List<string>());
                            datasetResults.Add("Cosine Score", new List<string>());
                            datasetResults.Add("Cosine M-1 Score", new List<string>());
                            headers.UnionWith(datasetResults.Keys);
                            continue;
                        }

                        var precursor = Convert.ToInt32(pieces[headerToIndex["Precursor Scan"]]);
                        var commonName = pieces[headerToIndex["Common Name"]];
                        var adduct = pieces[headerToIndex["Adduct"]];
                        var spectrum = lcmsRun.GetSpectrum(precursor);
                        if (spectrum == null)
                        {
                            Console.WriteLine("Invalid scan number: {0}", precursor);
                            continue;
                        }

                        var lipid = new Lipid { AdductFull = adduct, CommonName = commonName };
                        var lipidTarget = lipid.CreateLipidTarget();
                        var spectrumSearchResult = new SpectrumSearchResult(null, null, spectrum, null, null, new Xic(), lcmsRun) { PrecursorTolerance = tolerance };
                        var pearsonCorrScore = correlationCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
                        var pearsonCorrMinus1Score = correlationCalculator.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);
                        var cosineScore = cosineCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
                        var cosineMinus1Score = cosineCalculator.GetFitScore(
                            spectrumSearchResult,
                            lipidTarget.Composition);

                        // Add results to results dictionary.
                        datasetResults["Raw File"].Add(rawName);
                        foreach (var header in headerToIndex.Keys)
                        {
                            datasetResults[header].Add(pieces[headerToIndex[header]]);
                        }

                        datasetResults["Pearson Corr Score"].Add(pearsonCorrScore.ToString());
                        datasetResults["Pearson Corr M-1 Score"].Add(pearsonCorrMinus1Score.ToString());
                        datasetResults["Cosine Score"].Add(cosineScore.ToString());
                        datasetResults["Cosine M-1 Score"].Add(cosineMinus1Score.ToString());
                    }
                }
            }

            // Write results
            var outputFilePath = Path.Combine(directoryPath, "training.tsv");
            using (var writer = new StreamWriter(outputFilePath))
            {
                // Write headers
                foreach (var header in headers)
                {
                    writer.Write("{0}\t", header);
                }

                writer.WriteLine();

                // Write data
                foreach (var datasetResults in results)
                {
                    var fileLength = datasetResults["Pearson Corr Score"].Count;
                    for (var i = 0; i < fileLength; i++)
                    {
                        foreach (var header in headers)
                        {
                            var value = datasetResults.ContainsKey(header) ? datasetResults[header][i] : string.Empty;
                            writer.Write("{0}\t", value);
                        }

                        writer.WriteLine();
                    }
                }
            }
        }

        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTrainingFiles\")]
        public void TestPearsonCorrelationWholeFile(string directoryPath)
        {
            var dirFiles = Directory.GetFiles(directoryPath);

            var correlationCalculator = new PearsonCorrelationFitUtil();
            foreach (var pathToResults in dirFiles.Where(path => path.EndsWith(".txt")))
            {
                var datasetName = Path.GetFileNameWithoutExtension(pathToResults);
                var pathToRaw = GetRawFilePath(directoryPath, datasetName);
                if (string.IsNullOrEmpty(pathToRaw))
                {
                    continue;
                }

                var lcmsRun = PbfLcMsRun.GetLcMsRun(pathToRaw);
                var tolerance = new Tolerance(30, ToleranceUnit.Ppm);
                var rawFileName = Path.GetFileName(pathToRaw);
                var datasetDirPath = Path.GetDirectoryName(pathToResults);
                var outputFileName = string.Format("{0}_training.tsv", datasetName);
                var outputPath = Path.Combine(datasetDirPath, outputFileName);
                using (var writer = new StreamWriter(outputPath))
                using (var reader = new StreamReader(new FileStream(pathToResults, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var lineCount = 0;
                    var headerToIndex = new Dictionary<string, int>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var pieces = line.Split('\t').ToArray();

                        if (lineCount++ == 0)
                        {   // First line

                            writer.Write("Raw File\t");
                            for (var i = 0; i < pieces.Length; i++)
                            {
                                headerToIndex.Add(pieces[i], i);
                                writer.Write("{0}\t", pieces[i]);
                            }

                            writer.WriteLine("Fit Score\tFit M-1 Score");

                            continue;
                        }

                        var precursor = Convert.ToInt32(pieces[headerToIndex["Precursor Scan"]]);
                        var commonName = pieces[headerToIndex["Common Name"]];
                        var adduct = pieces[headerToIndex["Adduct"]];
                        var spectrum = lcmsRun.GetSpectrum(precursor);
                        if (spectrum == null)
                        {
                            Console.WriteLine("Invalid scan number: {0}", precursor);
                            continue;
                        }

                        var lipid = new Lipid { AdductFull = adduct, CommonName = commonName };
                        var lipidTarget = lipid.CreateLipidTarget();
                        var spectrumSearchResult = new SpectrumSearchResult(null, null, spectrum, null, null, new Xic(), lcmsRun) { PrecursorTolerance = tolerance };
                        var fitScore = correlationCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
                        var fitMinus1Score = correlationCalculator.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);

                        writer.Write(rawFileName + "\t");
                        writer.Write(line);

                        writer.WriteLine("{0}\t{1}", fitScore, fitMinus1Score);
                    }
                }
            }
        }

        //[TestCase(14, "[M+H]+", "PS(18:0/18:1)", "25", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(131, "[M+H]+", "PS(18:0/18:1)", "136", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(222, "[M+H]+", "PS(18:0/18:1)", "225", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(261, "[M+H]+", "PC(19:3/0:0)", "268", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]

        //[TestCase(2562, "[M+H]+", "PC(6:2/14:2)", "2565", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(7281, "[M+H]+", "PG(O-16:0/16:0)", "7288", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(12867, "[M+H]+", "SM(d18:1/24:0)", "12868", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(14752, "[M+H]+", "PC(18:0/22:0)", "14761", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]

          [TestCase(40, "[M-H]-", "CL(20:2/16:0/18:2/20:2)", "45", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
          [TestCase(139, "[M-H]-", "CL(20:2/16:0/18:2/20:2)", "142", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
          [TestCase(4909, "[M-H]-", "PG(21:0/22:4)", "4914", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
        public void TestIndividualLipidTargets(int precursor, string adduct, string commonName, string id, string rawFilePath)
        {
            var lipid = new Lipid() {AdductFull = adduct, CommonName = commonName};
            var lipidTarget = lipid.CreateLipidTarget();

            var composition = lipidTarget.Composition;

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            var relativeIntensityThreshold = 0.1;

            var tolerance = new Tolerance(30, ToleranceUnit.Ppm);

            //Get the values to use to calculate pearson correlation
            var observedPeaks = LipidUtil.GetAllIsotopePeaks(spectrum, composition, tolerance,
                relativeIntensityThreshold);
            if (observedPeaks == null) Console.WriteLine("Observed peaks is null for scan " + id);

            var isotopomerEnvelope = IsoProfilePredictor.GetIsotopomerEnvelop(
                composition.C,
                composition.H,
                composition.N,
                composition.O,
                composition.S);

            var observedIntensities = new double[observedPeaks.Length];

            for (var i = 0; i < observedPeaks.Length; i++)
            {
                var observedPeak = observedPeaks[i];
                observedIntensities[i] = observedPeak != null ? (float)observedPeak.Intensity : 0.0;
            }

            Console.WriteLine("The theoretical y values are: ");
            foreach (var value in isotopomerEnvelope.Envelope)
            {
                Console.WriteLine(value + ", ");
            }

            Console.WriteLine("The observed peak intensity x values are: ");
            foreach (var value in observedIntensities)
            {
                Console.WriteLine(value + ", ");
            }
        }

        //[TestCase(14, "[M+H]+", "PS(18:0/18:1)", "25", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(131, "[M+H]+", "PS(18:0/18:1)", "136", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(222, "[M+H]+", "PS(18:0/18:1)", "225", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(261, "[M+H]+", "PC(19:3/0:0)", "268", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(300, "[M+H]+", "PS(18:0/20:3)", "305", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]

        //[TestCase(2562, "[M+H]+", "PC(6:2/14:2)", "2565", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(7281, "[M+H]+", "PG(O-16:0/16:0)", "7288",@"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(12867, "[M+H]+", "SM(d18:1/24:0)", "12868", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]
        //[TestCase(14752, "[M+H]+", "PC(18:0/22:0)", "14761", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw")]

        [TestCase(40, "[M-H]-", "CL(20:2/16:0/18:2/20:2)", "45", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
        [TestCase(139, "[M-H]-", "CL(20:2/16:0/18:2/20:2)", "142", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
        [TestCase(4909, "[M-H]-", "PG(21:0/22:4)", "4914", @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_SL_8_Lipid_pooled_2_NEG_150mm_09Nov15_Polaroid_HSST3-02.raw")]
        public void TestFitMinusOneScore(int precursor, string adduct, string commonName, string id, string rawFilePath)
        {
            var lipid = new Lipid() { AdductFull = adduct, CommonName = commonName };
            var lipidTarget = lipid.CreateLipidTarget();

            var composition = lipidTarget.Composition;
            var compMinus1 = new Composition(composition.C, composition.H - 1, composition.N, composition.O, composition.S, composition.P); //Subtract one hydrogen to make this a minus1 fit score

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            var relativeIntensityThreshold = 0.1;

            var tolerance = new Tolerance(30, ToleranceUnit.Ppm);

            //Get the values to use to calculate pearson correlation
            var observedPeaks = LipidUtil.GetAllIsotopePeaks(spectrum, compMinus1, tolerance,
                relativeIntensityThreshold);
            if (observedPeaks == null) Console.WriteLine("Observed peaks is null for scan " + id);

            var isotopomerEnvelope = IsoProfilePredictor.GetIsotopomerEnvelop(
                compMinus1.C,
                compMinus1.H,
                compMinus1.N,
                compMinus1.O,
                compMinus1.S);

            var observedIntensities = new double[observedPeaks.Length];

            for (var i = 0; i < observedPeaks.Length; i++)
            {
                var observedPeak = observedPeaks[i];
                observedIntensities[i] = observedPeak != null ? (float)observedPeak.Intensity : 0.0;
            }

            Console.WriteLine("The theoretical y values are: ");
            foreach (var value in isotopomerEnvelope.Envelope)
            {
                Console.WriteLine(value + ", ");
            }

            Console.WriteLine("The observed peak intensity x values are: ");
            foreach (var value in observedIntensities)
            {
                Console.WriteLine(value + ", ");
            }
        }
    }
}
