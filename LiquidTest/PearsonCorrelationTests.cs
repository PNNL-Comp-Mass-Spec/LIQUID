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
    }
}
