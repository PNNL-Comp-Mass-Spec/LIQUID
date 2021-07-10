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

        private struct FragmentationScanInfo
        {
            /// <summary>
            /// Fragmentation type
            /// </summary>
            public ActivationMethod ScanType;

            /// <summary>
            /// Precursor m/z
            /// </summary>
            public double PrecursorMz;

            /// <summary>
            /// Scan Number
            /// </summary>
            public int ScanNumber;

            public override string ToString()
            {
                return string.Format("Scan {0}, {1}, {2:F2} m/z", ScanNumber, ScanType, PrecursorMz);
            }
        }

        /// <summary>
        /// LC/MS Run object
        /// </summary>
        public LcMsRun LcMsRun { get; }

        /// <summary>
        /// Scoring model
        /// </summary>
        public ScoreModel ScoreModel { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawFileLocation"></param>
        /// <param name="scoreModelLocation"></param>
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

            var ms2scans = lcmsRun.GetScanNumbers(2);
            var ms2spectra = ms2scans.Select(scan => lcmsRun.GetSpectrum(scan) as ProductSpectrum).ToList();
            var uniqueMz = (from spectrum in ms2spectra select GetMsMsPrecursorMz(spectrum)).ToList().Distinct().ToList();

            foreach (var mz in uniqueMz)
            {
                var hcdScans = ms2spectra.Where(x => Math.Abs(GetMsMsPrecursorMz(x) - mz) < float.Epsilon && x.ActivationMethod == ActivationMethod.HCD).Select(x => x.ScanNum).ToList();
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

        /// <summary>
        /// Get the precursor m/z of the given spectrum
        /// </summary>
        /// <param name="spectrum"></param>
        /// <remarks>
        /// Preferentially uses IsolationWindow.IsolationWindowTargetMz,
        /// but will use IsolationWindow.MonoisotopicMz if IsolationWindowTargetMz is 0
        /// </remarks>
        /// <returns>Precursor m/z</returns>
        public static double GetMsMsPrecursorMz(ProductSpectrum spectrum)
        {
            var a = spectrum.IsolationWindow.IsolationWindowTargetMz;
            var b = spectrum.IsolationWindow.MonoisotopicMz;
            if (b is null or 0)
                return a;

            if (a == 0)
                return b.Value;

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

            // Obtain the ms/ms scan numbers, sorted by precursor m/z, then HCD, then CID
            // This is necessary because HCD/CID paired MS/MS spectra in datasets from the Orbitrap Fusion Lumos are not necessarily stored adjacent to one another
            var fragScanPairs = GetSortedMsMsScans(lcmsRun);

            if (fragScanPairs.Count == 0)
                throw new SystemException("File has no MS/MS spectra");

            var scanPairCount = fragScanPairs.Count;

            for (var i = 0; i < scanPairCount; i++)
            {
                var scanPair = fragScanPairs[i];

                // Lookup the MS/MS Spectrum
                if (lcmsRun.GetSpectrum(scanPair.FirstScan) is not ProductSpectrum firstMsMsSpectrum)
                    continue;

                // Lookup the MS/MS Spectrum
                ProductSpectrum secondMsMsSpectrum;
                if (scanPair.HasTwoScans)
                {
                    secondMsMsSpectrum = lcmsRun.GetSpectrum(scanPair.SecondScan) as ProductSpectrum;
                    if (secondMsMsSpectrum == null)
                        continue;
                }
                else
                {
                    secondMsMsSpectrum = null;
                }

                //textWriter.WriteLine(i);
                //Console.WriteLine(DateTime.Now + "\tProcessing Scan" + i);

                // Grab Precursor Spectrum
                var precursorSpectrum = lcmsRun.GetSpectrum(scanPair.PrecursorScanNumber);

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

                // Report progress
                if (progress != null)
                {
                    var currentProgress = (int)(i / (double)scanPairCount * 100);
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

            return new MassCalibrationResults(ppmHistogram);
        }

        [Obsolete("Deprecated after switching to List<ScanPair>")]
        private static ActivationMethodCombination FigureOutActivationMethodCombination(ISpectrumAccessor lcmsRun)
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

        /// <summary>
        /// Obtain the ms/ms scan numbers, sorted by precursor m/z, then HCD, then CID
        /// </summary>
        /// <param name="lcmsRun"></param>
        /// <remarks>
        /// This is necessary because HCD/CID paired MS/MS spectra in datasets from the Orbitrap Fusion Lumos are not necessarily stored adjacent to one another
        /// </remarks>
        /// <returns>List of scan numbers</returns>
        internal static List<ScanPair> GetSortedMsMsScans(LcMsRun lcmsRun)
        {
            // ReSharper disable CommentTypo

            // Example scans, from dataset QC_BTLE_01_POS_01Jul21_Glacier-WCSH316601

            // Original order
            // Scan 444,  447.3465 m/z, HCD
            // Scan 445,  447.3465 m/z, CID
            // Scan 446,  411.1716 m/z, HCD
            // Scan 447,  411.1716 m/z, CID
            // Scan 448, 1055.2985 m/z, HCD
            // Scan 449,  308.2068 m/z, HCD
            // Scan 450,  308.2068 m/z, CID
            // Scan 451, 1055.2985 m/z, CID
            // Scan 452,  360.1803 m/z, HCD
            // Scan 453,  360.1803 m/z, CID
            // Scan 454,  822.7541 m/z, HCD
            // Scan 455,  822.7541 m/z, CID

            // Sorted order
            // Scan 449,  308.2068 m/z, HCD
            // Scan 450,  308.2068 m/z, CID
            // Scan 452,  360.1803 m/z, HCD
            // Scan 453,  360.1803 m/z, CID
            // Scan 446,  411.1716 m/z, HCD
            // Scan 447,  411.1716 m/z, CID
            // Scan 444,  447.3465 m/z, HCD
            // Scan 445,  447.3465 m/z, CID
            // Scan 454,  822.7541 m/z, HCD
            // Scan 455,  822.7541 m/z, CID
            // Scan 448, 1055.2985 m/z, HCD
            // Scan 451, 1055.2985 m/z, CID

            // ReSharper restore CommentTypo

            var fragScanPairs = new List<ScanPair>();

            var ms2ScansByPrecursor = new SortedDictionary<int, List<int>>();

            foreach (var ms2Scan in lcmsRun.GetScanNumbers(2))
            {
                var precursorScan = lcmsRun.MinMsLevel <= 1 ? lcmsRun.GetPrecursorScanNum(ms2Scan) : 0;

                if (ms2ScansByPrecursor.TryGetValue(precursorScan, out var precursorScans))
                {
                    precursorScans.Add(ms2Scan);
                    continue;
                }

                ms2ScansByPrecursor.Add(precursorScan, new List<int> { ms2Scan });
            }

            var scanGroupScans = new List<FragmentationScanInfo>();
            var sortComparer = new FragmentationScanComparer();

            foreach (var scanGroup in ms2ScansByPrecursor)
            {
                var precursorScan = scanGroup.Key;
                scanGroupScans.Clear();

                foreach (var ms2Scan in scanGroup.Value)
                {
                    var spectrum = lcmsRun.GetSpectrum(ms2Scan) as ProductSpectrum;
                    if (spectrum == null)
                        continue;

                    var scanInfo = new FragmentationScanInfo
                    {
                        ScanNumber = ms2Scan,
                        ScanType = spectrum.ActivationMethod,
                        PrecursorMz = GetMsMsPrecursorMz(spectrum)
                    };

                    scanGroupScans.Add(scanInfo);
                }

                if (lcmsRun.MinMsLevel > 1)
                {
                    // Do not sort the scans if the dataset only has MS2 and/or MS3 spectra
                }
                else
                {
                    scanGroupScans.Sort(sortComparer);
                }

                for (var scanIndex = 0; scanIndex < scanGroupScans.Count; scanIndex++)
                {
                    var firstScan = scanGroupScans[scanIndex];
                    if (scanIndex == scanGroupScans.Count - 1)
                    {
                        var singleScan = new ScanPair(precursorScan, firstScan.ScanNumber, firstScan.ScanType);

                        fragScanPairs.Add(singleScan);
                        continue;
                    }

                    var secondScan = scanGroupScans[scanIndex + 1];

                    var deltaMz = firstScan.PrecursorMz - secondScan.PrecursorMz;
                    if (Math.Abs(deltaMz) > 0.01)
                    {
                        // Scans do not have the same precursor
                        var singleScan = new ScanPair(precursorScan, firstScan.ScanNumber, firstScan.ScanType);

                        fragScanPairs.Add(singleScan);
                        continue;
                    }

                    // The Sort comparer should have put the HCD scan first and the CID scan second

                    if (firstScan.ScanType == ActivationMethod.HCD && secondScan.ScanType != ActivationMethod.HCD)
                    {
                        var scanPair = new ScanPair(precursorScan, firstScan.ScanNumber, secondScan.ScanNumber);
                        fragScanPairs.Add(scanPair);

                        // Increment the scan index so that the second scan is skipped
                        scanIndex++;
                        continue;
                    }

                    // This is not a valid HCD / CID scan pair; store the scans separately
                    fragScanPairs.Add(new ScanPair(precursorScan, firstScan.ScanNumber, firstScan.ScanType));
                }
            }

            return fragScanPairs;
        }

        private class FragmentationScanComparer : IComparer<FragmentationScanInfo>
        {
            /// <summary>
            /// Sort by m/z, then place HCD scans before other scans
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public int Compare(FragmentationScanInfo x, FragmentationScanInfo y)
            {
                if (x.PrecursorMz > y.PrecursorMz)
                {
                    return 1;
                }

                if (x.PrecursorMz < y.PrecursorMz)
                {
                    return -1;
                }

                // Sort HCD scans before non HCD scans
                var scanType1 = x.ScanType == ActivationMethod.HCD ? 0 : 1;

                var scanType2 = y.ScanType == ActivationMethod.HCD ? 0 : 1;

                return scanType1.CompareTo(scanType2);
            }
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
