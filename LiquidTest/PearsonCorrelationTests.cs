using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="precursorSpectrum"></param>
        /// <param name="target"></param>
        /// <returns>Pearson correlation between observed and theoretical isotopic profiles.</returns>
        public double GetPearsonCorrelation(Spectrum precursorSpectrum, Composition composition, Tolerance tolerance)
        {
            double pearsonCorrelation = LipidUtil.GetPearsonCorrelation(precursorSpectrum, composition, tolerance);
            return pearsonCorrelation;
        }

        [TestCase(14, "PS(18:0/18:1)", "C42H81N1O10P1", 30)]
        public void TestPearsonCorrelation(int precursor, string commonName, string composition, Tolerance tolerance)
        {
            var rawFilePath = @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw";

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            var parsedComposition = Composition.ParseFromPlainString(composition);
            //var target = new LipidTarget(commonName, LipidClass.DG, FragmentationMode.Positive, parsedComposition, new List<AcylChain>(), Adduct.Hydrogen);
            var correlation = GetPearsonCorrelation(spectrum, parsedComposition, tolerance);
            Console.WriteLine("The Pearson correlation is: " + correlation);
        }

        public string GetRawFilePath(string directory, string datasetName)
        {
            string rawFileName = datasetName + ".raw";
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

                    // Lookup in DMS via Mage
                    string dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
                    DirectoryInfo dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                    string fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

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

        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTrainingFiles\")]
        public void TestPearsonCorrelationWholeFile(string directoryPath)
        {
            var dirFiles = Directory.GetFiles(directoryPath);

            foreach (var pathToResults in dirFiles.Where(path => path.EndsWith(".tsv")))
            {
                var datasetName = Path.GetFileNameWithoutExtension(pathToResults);
                var pathToRaw = GetRawFilePath(directoryPath, datasetName);
                if (string.IsNullOrEmpty(pathToRaw))
                {
                    continue;
                }

                var lcmsRun = PbfLcMsRun.GetLcMsRun(pathToRaw);
                Tolerance tolerance = new Tolerance(30, ToleranceUnit.Ppm);
                var rawFileName = Path.GetFileName(pathToRaw);
                var datasetDirPath = Path.GetDirectoryName(pathToResults);
                var outputFileName = string.Format("{0}_training.tsv", datasetName);
                var outputPath = Path.Combine(datasetDirPath, outputFileName);
                using (var writer = new StreamWriter(outputPath))
                using (var reader = new StreamReader(pathToResults))
                {
                    int lineCount = 0;
                    var headerToIndex = new Dictionary<string, int>();
                    while (reader.Peek() > -1)
                    {
                        var line = reader.ReadLine();
                        var pieces = line.Split('\t').ToArray();

                        if (lineCount++ == 0)
                        {   // First line

                            writer.Write("Raw File\t");
                            for (int i = 0; i < pieces.Length; i++)
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
                        var lipid = new Lipid { AdductFull = adduct, CommonName = commonName };
                        var lipidTarget = lipid.CreateLipidTarget();
                        var spectrumSearchResult = new SpectrumSearchResult(null, null, spectrum, null, null, new Xic(), lcmsRun) { PrecursorTolerance = tolerance };
                        var fitScore = LipidUtil.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
                        var fitMinus1Score = LipidUtil.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);

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
            Lipid lipid = new Lipid() {AdductFull = adduct, CommonName = commonName};
            LipidTarget lipidTarget = lipid.CreateLipidTarget();

            Composition composition = lipidTarget.Composition;

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            double relativeIntensityThreshold = 0.1;

            Tolerance tolerance = new Tolerance(30, ToleranceUnit.Ppm);

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
            foreach (var value in isotopomerEnvelope.Envolope)
            {
                Console.WriteLine(value + ", ");
            }
            
            Console.WriteLine("The observed peak intensity x values are: ");
            foreach (var value in observedIntensities)
            {
                Console.WriteLine(value + ", ");
            }
        }

        //[TestCase(14, "[M+H]+", "PS(18:0/18:1)", "25")]
        //[TestCase(131, "[M+H]+", "PS(18:0/18:1)", "136")]
        //[TestCase(222, "[M+H]+", "PS(18:0/18:1)", "225")]
        //[TestCase(261, "[M+H]+", "PC(19:3/0:0)", "268")]
        //[TestCase(300, "[M+H]+", "PS(18:0/20:3)", "305")]
        [TestCase(2562, "[M+H]+", "PC(6:2/14:2)", "2565")]
        [TestCase(7281, "[M+H]+", "PG(O-16:0/16:0)", "7288")]
        [TestCase(12867, "[M+H]+", "SM(d18:1/24:0)", "12868")]
        [TestCase(14752, "[M+H]+", "PC(18:0/22:0)", "14761")]
        public void TestFitMinusOneScore(int precursor, string adduct, string commonName, string id)
        {
            Lipid lipid = new Lipid() { AdductFull = adduct, CommonName = commonName };
            LipidTarget lipidTarget = lipid.CreateLipidTarget();

            Composition composition = lipidTarget.Composition;
            Composition compMinus1 = new Composition(composition.C, composition.H - 1, composition.N, composition.O, composition.S, composition.P); //Subtract one hydrogen to make this a minus1 fit score

            var rawFilePath = @"\\proto-2\UnitTest_Files\Liquid\PearsonCorrelationTests\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw";

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            double relativeIntensityThreshold = 0.1;

            Tolerance tolerance = new Tolerance(30, ToleranceUnit.Ppm);

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
            foreach (var value in isotopomerEnvelope.Envolope)
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
