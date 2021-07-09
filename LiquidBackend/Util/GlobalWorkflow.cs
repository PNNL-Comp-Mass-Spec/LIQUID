using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Scoring;
using PRISM;

namespace LiquidBackend.Util
{
    public class GlobalWorkflow
    {
        // Ignore Spelling: foreach, Lumos, Orbitrap, workflow, xic

        public LcMsRun LcMsRun { get; }
        public ScoreModel ScoreModel { get; }

        public GlobalWorkflow(string rawFileLocation, string scoreModelLocation = "DefaultScoringModel.xml")
        {
            var dataFactory = new LcMsDataFactory();
            dataFactory.ProgressChanged += LcMsDataFactory_ProgressChanged;

            LcMsRun = dataFactory.GetLcMsData(rawFileLocation);
            ScoreModel = ScoreModelSerialization.Deserialize(scoreModelLocation);
        }

        /// <summary>
        /// Run the workflow
        /// </summary>
        /// <param name="lipidList"></param>
        /// <param name="hcdMassError"></param>
        /// <param name="cidMassError"></param>
        /// <param name="progress"></param>
        /// <returns>List of search results</returns>
        public List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, double hcdMassError, double cidMassError, IProgress<int> progress = null)
        {
            return RunGlobalWorkflow(lipidList, LcMsRun, hcdMassError, cidMassError, ScoreModel, progress);
        }

        /*
        public static List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, DataReader ImsRun, IEnumerable<ImsFeature> FeatureTargets, double hcdMassError,double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
        {
            Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList();
            int MS1Frames = ImsRun.GetNumberOfFrames(DataReader.FrameType.MS1);
            int MS2Frames = ImsRun.GetNumberOfFrames(DataReader.FrameType.MS2);

            var globalParams = ImsRun.GetGlobalParams();
            var frameList = ImsRun.GetMasterFrameList();

            ActivationMethodCombination activationMethodCombination = MS2Frames > 0 ? ActivationMethodCombination.CidOnly : ActivationMethodCombination.Unsupported;
            if (activationMethodCombination == ActivationMethodCombination.Unsupported) throw new SystemException("Unsupported activation method.");

            List<Spectrum> Spectra = new List<Spectrum>();
            foreach (var feature in FeatureTargets)
            {
                double mzToSearchTolerance = hcdMassError * feature.Mz / 1000000;
                double lowMz = feature.Mz - mzToSearchTolerance;
                double highMz = feature.Mz + mzToSearchTolerance;

                double[] mzArray;
                int[] intensityArray;
                ImsRun.GetSpectrum(feature.LcStart, feature.LcEnd, DataReader.FrameType.MS1, feature.ImsStart,
                    feature.ImsEnd, out mzArray, out intensityArray);

                foreach (var mz in mzArray)
                {
                    if (mz > highMz) break;
                    if (mz > lowMz)
                    {
                        // Target IMS feature found in this scan
                        double[] ms2Mz;
                        int[] ms2Intensities;
                        ImsRun.GetSpectrum(feature.LcStart, feature.LcEnd, DataReader.FrameType.MS2, feature.ImsStart,
                            feature.ImsEnd, out ms2Mz, out ms2Intensities);
                        var ms2Intensity = (from intensity in ms2Intensities select (double) (intensity)).ToArray();
                        Spectrum spec = new ProductSpectrum(ms2Mz, ms2Intensity, feature.ImsScan);
                        spec.MsLevel = 2;
                        Spectra.Add(spec);
                    }
                }


                /*
                for (int lcScan = feature.LcStart; lcScan <= feature.LcEnd; lcScan++)
                {
                    if (frameList[lcScan] == DataReader.FrameType.MS1)
                    {
                        for (int imsScan = feature.ImsStart; imsScan <= feature.ImsEnd; imsScan++)
                        {
                            double[] mzArray;
                            int[] intensityArray;
                            ImsRun.GetSpectrum(lcScan, lcScan, DataReader.FrameType.MS1, imsScan, imsScan, out mzArray, out intensityArray);

                            foreach (var mz in mzArray)
                            {
                                if (mz > highMz) break;
                                if (mz > lowMz)
                                {
                                    // Target IMS feature found in this scan
                                    feature.AddCoord(lcScan, imsScan);
                                }
                            }
                        }
                    }
                }
            }

            return new List<LipidGroupSearchResult>();
        }
        */

        /// <summary>
        /// Run the workflow on summed spectra
        /// </summary>
        /// <param name="lipidList"></param>
        /// <param name="lcmsRun"></param>
        /// <param name="hcdMassError"></param>
        /// <param name="cidMassError"></param>
        /// <param name="scoreModel"></param>
        /// <returns>List of search results</returns>
        public static List<LipidGroupSearchResult> RunGlobalWorkflowAvgSpec(
            IEnumerable<Lipid> lipidList,
            LcMsRun lcmsRun,
            double hcdMassError,
            double cidMassError,
            ScoreModel scoreModel)
        {
            var lipidGroupSearchResultList = new List<LipidGroupSearchResult>();

            var hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            var cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.MzRounded).GroupBy(x => x.LipidTarget).ToList();

            var minLcScan = lcmsRun.MinLcScan;
            double maxLcScan = lcmsRun.MaxLcScan;

            var ms2scans = lcmsRun.GetScanNumbers(2);
            var ms2spectra = ms2scans.Select(scan => lcmsRun.GetSpectrum(scan) as ProductSpectrum).ToList();
            var uniqueMz = (from spectrum in ms2spectra select GetMsMsPrecursorMz(spectrum)).ToList().Distinct().ToList();

            foreach (var mz in uniqueMz)
            {
                var hcdScans = ms2spectra.Where(x => Math.Abs(GetMsMsPrecursorMz(x)- mz) < float.Epsilon && x.ActivationMethod == ActivationMethod.HCD).Select(x => x.ScanNum).ToList();
                var summedSpec = lcmsRun.GetSummedSpectrum(hcdScans);
                var summedHcdSpec = new ProductSpectrum(summedSpec.Peaks, 0) { ActivationMethod = ActivationMethod.HCD };

                var cidScans = ms2spectra.Where(x => Math.Abs(GetMsMsPrecursorMz(x) - mz) < float.Epsilon && x.ActivationMethod == ActivationMethod.CID).Select(x => x.ScanNum).ToList();
                summedSpec = lcmsRun.GetSummedSpectrum(cidScans);
                var summedCidSpec = new ProductSpectrum(summedSpec.Peaks, 0) { ActivationMethod = ActivationMethod.CID };

                var hcdPresent = summedHcdSpec.Peaks.Length > 0;
                var cidPresent = summedCidSpec.Peaks.Length > 0;

                if (hcdPresent)
                {
                    summedHcdSpec.IsolationWindow = new IsolationWindow(mz, hcdMassError, hcdMassError);
                }
                if (cidPresent)
                {
                    summedCidSpec.IsolationWindow = new IsolationWindow(mz, cidMassError, cidMassError);
                }

                var mzToSearchTolerance = hcdMassError * mz / 1000000;
                var lowMz = mz - mzToSearchTolerance;
                var highMz = mz + mzToSearchTolerance;

                var hcdSpectrum = summedHcdSpec;
                var cidSpectrum = summedCidSpec;

                // Note: this is unused
                Spectrum precursorSpectrum = null;

                foreach (var grouping in lipidsGroupedByTarget)
                {
                    var lipidTarget = grouping.Key;
                    var lipidMz = lipidTarget.MzRounded;

                    // If we reached the point where the m/z is too high, we can exit
                    if (lipidMz > highMz) break;

                    if (lipidMz > lowMz)
                    {
                        // Find the MS1 data
                        // Xic xic = lcmsRun.GetPrecursorExtractedIonChromatogram(lipidMz, hcdTolerance, i);
                        var xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(lipidMz, hcdTolerance);

                        // Bogus data
                        if (precursorSpectrum != null && (xic.GetApexScanNum() < 0 || xic.GetSumIntensities() <= 0)) continue;

                        // Grab the MS/MS peak to search for
                        IEnumerable<MsMsSearchUnit> msMsSearchUnits = lipidTarget.GetMsMsSearchUnits();

                        // Get all matching peaks
                        var hcdSearchResultList = GetMatchingPeaks(msMsSearchUnits, hcdSpectrum, hcdTolerance);
                        var cidSearchResultList = GetMatchingPeaks(msMsSearchUnits, cidSpectrum, cidTolerance);

                        // Create spectrum search results
                        SpectrumSearchResult spectrumSearchResult;
                        LipidGroupSearchResult lipidGroupSearchResult;
                        if (precursorSpectrum != null)
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }
                        else // If there are no precursor scans in this file
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, hcdSearchResultList, cidSearchResultList, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }

                        lipidGroupSearchResultList.Add(lipidGroupSearchResult);

                        //textWriter.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
                        //Console.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
                    }
                }
            }

            //var summedHcdSpectra = uniqueMz.Select(mz => new Tuple<double, ProductSpectrum>(mz, lcmsRun.GetSummedMs2Spectrum(mz, minLcScan, (int)maxLcScan, 1, 2, ActivationMethod.HCD))).ToDictionary(x => x.Item1, x => x.Item2);
            //if (summedHcdSpectra != null) { foreach (var spec in summedHcdSpectra) { spec.Value.IsolationWindow = new IsolationWindow(spec.Key, spec.Key, spec.Key); } }
            //var summedCidSpectra = uniqueMz.Select(mz => lcmsRun.GetSummedMs2Spectrum(mz, minLcScan, (int)maxLcScan, 1, 2, ActivationMethod.CID)).ToList();

            return lipidGroupSearchResultList;
        }

        public static double GetMsMsPrecursorMz(ProductSpectrum s)
        /// <summary>
        /// Get the precursor m/z of the given spectrum
        /// </summary>
        /// <param name="spectrum"></param>
        /// <remarks>
        /// Preferentially uses IsolationWindow.IsolationWindowTargetMz,
        /// but will use IsolationWindow.MonoisotopicMz if IsolationWindowTargetMz is 0
        /// </remarks>
        /// <returns>Precursor m/z</returns>
        {
            var a = s.IsolationWindow.IsolationWindowTargetMz;
            var b = s.IsolationWindow.MonoisotopicMz;
            if (b == null || b == 0) return a;
            if (a == 0) return (double)b;
            //return Nullable.Compare(a, b) > 0 ? (double)a : (double)b;
            return a;
        }

        /// <summary>
        /// Run the workflow
        /// </summary>
        /// <param name="lipidList"></param>
        /// <param name="lcmsRun"></param>
        /// <param name="hcdMassError"></param>
        /// <param name="cidMassError"></param>
        /// <param name="scoreModel"></param>
        /// <param name="progress"></param>
        /// <returns>List of search results</returns>
        public static List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
        {
            //TextWriter textWriter = new StreamWriter("outputNeg.tsv");
            var lipidGroupSearchResultList = new List<LipidGroupSearchResult>();

            var hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            var cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            //var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList(); //order by mz
            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.MzRounded).GroupBy(x => x.LipidTarget).ToList();

            var minLcScan = lcmsRun.MinLcScan;
            double maxLcScan = lcmsRun.MaxLcScan;

            var activationMethodCombination = FigureOutActivationMethodCombination(lcmsRun);
            if (activationMethodCombination == ActivationMethodCombination.Unsupported) throw new SystemException("Unsupported activation method.");
            var useTwoScans = activationMethodCombination == ActivationMethodCombination.CidThenHcd || activationMethodCombination == ActivationMethodCombination.HcdThenCid;

            for (var i = minLcScan; i <= maxLcScan; i++)
            {
                // Lookup the MS/MS Spectrum
                if (!(lcmsRun.GetSpectrum(i) is ProductSpectrum firstMsMsSpectrum))
                    continue;

                // Lookup the MS/MS Spectrum
                ProductSpectrum secondMsMsSpectrum = null;
                if (useTwoScans)
                {
                    secondMsMsSpectrum = lcmsRun.GetSpectrum(i + 1) as ProductSpectrum;
                    if (secondMsMsSpectrum == null) continue;

                    // If m/z values of the MS/MS spectra do not match, just move on
                    var firstMsMsSpectrumPrecursor = GetMsMsPrecursorMz(firstMsMsSpectrum);
                    var secondMsMsSpectrumPrecursor = GetMsMsPrecursorMz(secondMsMsSpectrum);

                    var deltaMz = firstMsMsSpectrumPrecursor - secondMsMsSpectrumPrecursor;
                    if (Math.Abs(deltaMz) > 0.01) continue;
                }

                //textWriter.WriteLine(i);
                //Console.WriteLine(DateTime.Now + "\tProcessing Scan" + i);

                // Grab Precursor Spectrum

                var precursorScanNumber = 0;
                if (lcmsRun.MinMsLevel == 1) // Make sure there are precursor scans in file
                {
                    precursorScanNumber = lcmsRun.GetPrecursorScanNum(i);
                }

                var precursorSpectrum = lcmsRun.GetSpectrum(precursorScanNumber);

                // Assign each MS/MS spectrum to HCD or CID
                ProductSpectrum hcdSpectrum;
                ProductSpectrum cidSpectrum;
                if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.HCD)
                {
                    hcdSpectrum = firstMsMsSpectrum;
                    cidSpectrum = secondMsMsSpectrum;
                }
                else
                {
                    hcdSpectrum = secondMsMsSpectrum;
                    cidSpectrum = firstMsMsSpectrum;
                }

                var msMsPrecursorMz = GetMsMsPrecursorMz(firstMsMsSpectrum);

                var mzToSearchTolerance = hcdMassError * msMsPrecursorMz / 1000000;
                var lowMz = msMsPrecursorMz - mzToSearchTolerance;
                var highMz = msMsPrecursorMz + mzToSearchTolerance;

                foreach (var grouping in lipidsGroupedByTarget)
                {
                    var lipidTarget = grouping.Key;
                    var lipidMz = lipidTarget.MzRounded;

                    // If we reached the point where the m/z is too high, we can exit
                    if (lipidMz > highMz) break;

                    if (lipidMz > lowMz)
                    {
                        // Find the MS1 data
                        // Xic xic = lcmsRun.GetPrecursorExtractedIonChromatogram(lipidMz, hcdTolerance, i);
                        var xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(lipidMz, hcdTolerance);

                        // Bogus data
                        if (precursorSpectrum != null && (xic.GetApexScanNum() < 0 || xic.GetSumIntensities() <= 0)) continue;

                        // Grab the MS/MS peak to search for
                        IEnumerable<MsMsSearchUnit> msMsSearchUnits = lipidTarget.GetMsMsSearchUnits();

                        // Get all matching peaks
                        var hcdSearchResultList = GetMatchingPeaks(msMsSearchUnits, hcdSpectrum, hcdTolerance);
                        var cidSearchResultList = GetMatchingPeaks(msMsSearchUnits, cidSpectrum, cidTolerance);

                        // Create spectrum search results
                        SpectrumSearchResult spectrumSearchResult;
                        LipidGroupSearchResult lipidGroupSearchResult;

                        if (precursorSpectrum != null)
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }
                        else // If there are no precursor scans in this file
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, hcdSearchResultList, cidSearchResultList, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }

                        lipidGroupSearchResultList.Add(lipidGroupSearchResult);
                    }
                }

                // Skip an extra scan if we look at 2 at a time
                if (useTwoScans) i++;

                // Report progress
                if (progress != null)
                {
                    var currentProgress = (int)(i / maxLcScan * 100);
                    progress.Report(currentProgress);
                }
            }

            //textWriter.Close();
            return lipidGroupSearchResultList;
        }

        public MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, double hcdMassError)
        {
            return RunMassCalibration(lipidList, LcMsRun, hcdMassError);
        }

        public static MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError)
        {
            var ppmErrorList = new List<double>();

            // Iterate through the lipids, group by target
            foreach (var item in lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList())
            {
                var target = item.Key;

                var spectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(target, lcmsRun, hcdMassError, 500, scoreModel: null);

                if (spectrumSearchResultList.Count == 0)
                {
                    continue;
                }

                var targetIon = new Ion(target.Composition - Composition.Hydrogen, 1);
                var targetMz = targetIon.GetMonoIsotopicMz();

                var bestSpectrumSearchResult = spectrumSearchResultList.OrderBy(x => x.Score).First();

                var massSpectrum = bestSpectrumSearchResult.PrecursorSpectrum.Peaks;
                var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();

                var ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
                ppmErrorList.Add(ppmError);
            }

            var ppmHistogram = QcUtil.CalculateHistogram(ppmErrorList, hcdMassError, 0.25);

            var massCalibrationResults = new MassCalibrationResults(ppmHistogram);
            return massCalibrationResults;
        }

        public static ActivationMethodCombination FigureOutActivationMethodCombination(LcMsRun lcmsRun)
        {
            var ms1ScanNumbers = lcmsRun.GetScanNumbers(1).ToList();
            var ms2ScanNumbers = lcmsRun.GetScanNumbers(2).ToList();
            ProductSpectrum firstMsMsSpectrum;
            int firstMsMsScanNumber;
            if (ms1ScanNumbers.Count > 0)
            {
                // Grab an MS1 Scan that's about 33% through the file so that we get accurate MS2 data
                var indexToGrab = (int)Math.Floor(ms1ScanNumbers.Count / 3.0);
                var ms1ScanNumberInMiddleOfRun = ms1ScanNumbers[indexToGrab];

                // Find the scan number of the next MS/MS spectrum
                firstMsMsScanNumber = lcmsRun.GetNextScanNum(ms1ScanNumberInMiddleOfRun, 2);

                // Get the MS/MS spectrum
                firstMsMsSpectrum = lcmsRun.GetSpectrum(firstMsMsScanNumber) as ProductSpectrum;
            }
            else
            {
                // There are no MS1 spectra
                var indexToGrab = (int)Math.Floor(ms2ScanNumbers.Count / 3.0);
                var ms2ScanNumberInMiddleOfRun = ms2ScanNumbers[indexToGrab];
                firstMsMsSpectrum = lcmsRun.GetSpectrum(ms2ScanNumberInMiddleOfRun) as ProductSpectrum;
                if (firstMsMsSpectrum == null) return ActivationMethodCombination.Unsupported;
                firstMsMsScanNumber = firstMsMsSpectrum.ScanNum;
            }

            if (firstMsMsSpectrum == null) return ActivationMethodCombination.Unsupported;

            // Get the next MS/MS spectrum
            var nextMsMsScanNumber = lcmsRun.GetNextScanNum(firstMsMsScanNumber, 2);
            var nextMsMsSpectrum = lcmsRun.GetSpectrum(nextMsMsScanNumber) as ProductSpectrum;

            // Treat PQD scans as if they were CID
            if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.PQD) firstMsMsSpectrum.ActivationMethod = ActivationMethod.CID;
            if (nextMsMsSpectrum?.ActivationMethod == ActivationMethod.PQD) nextMsMsSpectrum.ActivationMethod = ActivationMethod.CID;

            if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.HCD)
            {
                if (nextMsMsScanNumber - firstMsMsScanNumber > 1) return ActivationMethodCombination.HcdOnly;
                if (nextMsMsSpectrum == null) return ActivationMethodCombination.HcdOnly;
                if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.CID) return ActivationMethodCombination.HcdThenCid;
                if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.HCD) return ActivationMethodCombination.HcdOnly;
            }
            else if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.CID)
            {
                if (nextMsMsScanNumber - firstMsMsScanNumber > 1) return ActivationMethodCombination.CidOnly;
                if (nextMsMsSpectrum == null) return ActivationMethodCombination.CidOnly;
                if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.HCD) return ActivationMethodCombination.CidThenHcd;
                if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.CID) return ActivationMethodCombination.CidOnly;
            }

            return ActivationMethodCombination.Unsupported;
        }

        private static List<MsMsSearchResult> GetMatchingPeaks(IEnumerable<MsMsSearchUnit> msMsSearchUnits, Spectrum spectrum, Tolerance tolerance)
        {
            if (spectrum == null)
                return new List<MsMsSearchResult>();

            return (from msMsSearchUnit in msMsSearchUnits
                    let peak = spectrum.FindPeak(msMsSearchUnit.Mz, tolerance)
                    select new MsMsSearchResult(msMsSearchUnit, peak)).ToList();
        }

        #region "Events"

        private void LcMsDataFactory_ProgressChanged(object sender, ProgressData e)
        {
            ProgressChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Raised for each reported progress value
        /// </summary>
        public event EventHandler<ProgressData> ProgressChanged;

        #endregion

    }
}
