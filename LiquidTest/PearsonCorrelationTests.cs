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
            var rawFilePath = @"C:\Users\ryad361\Desktop\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw";

            var lcmsRun = PbfLcMsRun.GetLcMsRun(rawFilePath);

            var spectrum = lcmsRun.GetSpectrum(precursor);

            var parsedComposition = Composition.ParseFromPlainString(composition);
            //var target = new LipidTarget(commonName, LipidClass.DG, FragmentationMode.Positive, parsedComposition, new List<AcylChain>(), Adduct.Hydrogen);
            var correlation = GetPearsonCorrelation(spectrum, parsedComposition, tolerance);
            Console.WriteLine("The Pearson correlation is: " + correlation);
        }

        [TestCase(@"\\protoapps\userdata\Wilkins\FromLillian\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw",
            @"\\protoapps\userdata\Wilkins\FromLillian\LiquidExportedResults.tsv")]
        public void TestPearsonCorrelationWholeFile(string pathToRaw, string pathToResults)
        {
            Tolerance tolerance = new Tolerance(30, ToleranceUnit.Ppm);

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
                        for (int i = 0; i < pieces.Count(); i++)
                        {
                            headerToIndex.Add(pieces[i], i);
                        }

                        continue;
                    }

                    var precursor = Convert.ToInt32(pieces[headerToIndex["Precursor Scan"]]);
                    var composition = pieces[headerToIndex["Formula"]];
                    var parsedCompostion = Composition.ParseFromPlainString(composition);
                    var lcmsRun = PbfLcMsRun.GetLcMsRun(pathToRaw);
                    var spectrum = lcmsRun.GetSpectrum(precursor);
                    var correlation = GetPearsonCorrelation(spectrum, parsedCompostion, tolerance);
                    Console.WriteLine("Common Name: " + pieces[headerToIndex["Common Name"]] + " Correlation: " + correlation);
                }
            }
        }

        [TestCase(14, "[M+H]+", "PS(18:0/18:1)", "25")]
        [TestCase(131, "[M+H]+", "PS(18:0/18:1)", "136")]
        [TestCase(222, "[M+H]+", "PS(18:0/18:1)", "225")]
        [TestCase(261, "[M+H]+", "PC(19:3/0:0)", "268")]
        public void TestIndividualLipidTargets(int precursor, string adduct, string commonName, string id)
        {
            Lipid lipid = new Lipid() {AdductFull = adduct, CommonName = commonName};
            LipidTarget lipidTarget = lipid.CreateLipidTarget();

            Composition composition = lipidTarget.Composition;

            var rawFilePath = @"C:\Users\ryad361\Desktop\OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16.raw";

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
    }
}
