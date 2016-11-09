using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Scoring;


namespace LiquidBackend.Util
{
    public class GlobalWorkflow
    {
        public LcMsRun LcMsRun { get; private set; }
        public ScoreModel ScoreModel { get; private set; }

        public GlobalWorkflow(string rawFileLocation, string scoreModelLocation = "DefaultScoringModel.xml")
        {
            this.LcMsRun = LcMsDataFactory.GetLcMsData(rawFileLocation);
            this.ScoreModel = ScoreModelSerialization.Deserialize(scoreModelLocation);
        }

        public List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, double hcdMassError, double cidMassError, IProgress<int> progress = null)
        {
            return RunGlobalWorkflow(lipidList, this.LcMsRun, hcdMassError, cidMassError, this.ScoreModel, progress);
        }

        /*
        public static List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, DataReader ImsRun, IEnumerable<ImsFeature> FeatureTargets, double hcdMassError,double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
        {
            Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList();
            int MS1Frames = ImsRun.GetNumberOfFrames(DataReader.FrameType.MS1); 
            int MS2Frames = ImsRun.GetNumberOfFrames(DataReader.FrameType.MS2);

            var gp = ImsRun.GetGlobalParams();
            var framelist = ImsRun.GetMasterFrameList();

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
                        //Target IMS feature found in this scan
                        double[] MS2Mz;
                        int[] MS2Ints;
                        ImsRun.GetSpectrum(feature.LcStart, feature.LcEnd, DataReader.FrameType.MS2, feature.ImsStart,
                            feature.ImsEnd, out MS2Mz, out MS2Ints);
                        var MS2Intensity = (from intensity in MS2Ints select (double) (intensity)).ToArray();
                        Spectrum spec = new ProductSpectrum(MS2Mz, MS2Intensity, feature.ImsScan);
                        spec.MsLevel = 2;
                        Spectra.Add(spec);
                    }
                }


                /*
                for (int lcScan = feature.LcStart; lcScan <= feature.LcEnd; lcScan++)
                {
                    if (framelist[lcScan] == DataReader.FrameType.MS1)
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
                                    //Target IMS feature found in this scan
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

        public static List<LipidGroupSearchResult> RunGlobalWorkflowAvgSpec(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
        { 
            List<LipidGroupSearchResult> lipidGroupSearchResultList = new List<LipidGroupSearchResult>();

            Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.MzRounded).GroupBy(x => x.LipidTarget).ToList();

            int minLcScan = lcmsRun.MinLcScan;
            double maxLcScan = lcmsRun.MaxLcScan;

            var ms2scans = lcmsRun.GetScanNumbers(2);
            List<ProductSpectrum> ms2spectra = ms2scans.Select(scan => lcmsRun.GetSpectrum(scan) as ProductSpectrum).ToList();
            var uniqueMz = (from spectrum in ms2spectra select spectrum.IsolationWindow.IsolationWindowTargetMz).ToList().Distinct().ToList();

            foreach (var mz in uniqueMz)
            {
                var hcdScans = ms2spectra.Where(x => x.IsolationWindow.IsolationWindowTargetMz == mz && x.ActivationMethod == ActivationMethod.HCD).Select(x => x.ScanNum).ToList();
                var summedSpec = lcmsRun.GetSummedSpectrum(hcdScans);
                ProductSpectrum summedHcdSpec = new ProductSpectrum(summedSpec.Peaks, 0) { ActivationMethod = ActivationMethod.HCD };

                var cidScans = ms2spectra.Where(x => x.IsolationWindow.IsolationWindowTargetMz == mz && x.ActivationMethod == ActivationMethod.CID).Select(x => x.ScanNum).ToList();
                summedSpec = lcmsRun.GetSummedSpectrum(cidScans);
                ProductSpectrum summedCidSpec = new ProductSpectrum(summedSpec.Peaks, 0) { ActivationMethod = ActivationMethod.CID };

                bool HcdPresent = summedHcdSpec.Peaks.Any();
                bool CidPresent = summedCidSpec.Peaks.Any();

                if (HcdPresent)
                {
                    summedHcdSpec.IsolationWindow = new IsolationWindow(mz, hcdMassError, hcdMassError);
                    
                }
                if (CidPresent)
                {
                    summedCidSpec.IsolationWindow = new IsolationWindow(mz, cidMassError, cidMassError);
                }

                double mzToSearchTolerance = hcdMassError * mz / 1000000;
                double lowMz = mz - mzToSearchTolerance;
                double highMz = mz + mzToSearchTolerance;

                var hcdSpectrum = summedHcdSpec;
                var cidSpectrum = summedCidSpec;
                Spectrum precursorSpectrum = null;

                foreach (var grouping in lipidsGroupedByTarget)
                {
                    LipidTarget lipidTarget = grouping.Key;
                    //double lipidMz = lipidTarget.Composition.Mass; //change to real mz
                    double lipidMz = lipidTarget.MzRounded;

                    // If we reached the point where the m/z is too high, we can exit
                    if (lipidMz > highMz) break;

                    if (lipidMz > lowMz)
                    {
                        // Find the MS1 data
                        //Xic xic = lcmsRun.GetPrecursorExtractedIonChromatogram(lipidMz, hcdTolerance, i);
                        Xic xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(lipidMz, hcdTolerance);

                        // Bogus data
                        if (precursorSpectrum != null && (xic.GetApexScanNum() < 0 || xic.GetSumIntensities() <= 0)) continue;

                        // Grab the MS/MS peak to search for
                        IEnumerable<MsMsSearchUnit> msMsSearchUnits = lipidTarget.GetMsMsSearchUnits();

                        // Get all matching peaks
                        List<MsMsSearchResult> hcdSearchResultList = hcdSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
                        List<MsMsSearchResult> cidSearchResultList = cidSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();

                        // Create spectrum search results
                        SpectrumSearchResult spectrumSearchResult = null;
                        LipidGroupSearchResult lipidGroupSearchResult = null;
                        if (precursorSpectrum != null)
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }
                        else //If there are no precursor scans in this file
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, hcdSearchResultList, cidSearchResultList, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult);
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

        public static List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
        {
            //TextWriter textWriter = new StreamWriter("outputNeg.tsv");
            List<LipidGroupSearchResult> lipidGroupSearchResultList = new List<LipidGroupSearchResult>();

            Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            //var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList(); //order by mz
            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.MzRounded).GroupBy(x => x.LipidTarget).ToList();

            int minLcScan = lcmsRun.MinLcScan;
            double maxLcScan = lcmsRun.MaxLcScan;



            ActivationMethodCombination activationMethodCombination = FigureOutActivationMethodCombination(lcmsRun);
            if (activationMethodCombination == ActivationMethodCombination.Unsupported) throw new SystemException("Unsupported activation method.");
            bool useTwoScans = (activationMethodCombination == ActivationMethodCombination.CidThenHcd || activationMethodCombination == ActivationMethodCombination.HcdThenCid);

            for (int i = minLcScan; i <= maxLcScan; i++)
            {
                // Lookup the MS/MS Spectrum
                ProductSpectrum firstMsMsSpectrum = lcmsRun.GetSpectrum(i) as ProductSpectrum;
                if (firstMsMsSpectrum == null) continue;

                // Lookup the MS/MS Spectrum
                ProductSpectrum secondMsMsSpectrum = null;
                if (useTwoScans)
                {
                    secondMsMsSpectrum = lcmsRun.GetSpectrum(i + 1) as ProductSpectrum;
                    if (secondMsMsSpectrum == null) continue;

                    // If m/z values of the MS/MS spectrums do not match, just move on
                    if (Math.Abs(firstMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz - secondMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz) > 0.01) continue;
                }

                //textWriter.WriteLine(i);
                //Console.WriteLine(DateTime.Now + "\tProcessing Scan" + i);

                // Grab Precursor Spectrum
                int precursorScanNumber = lcmsRun.GetPrecursorScanNum(i);
                Spectrum precursorSpectrum = lcmsRun.GetSpectrum(precursorScanNumber);

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

                double msMsPrecursorMz = firstMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;
                double mzToSearchTolerance = hcdMassError * msMsPrecursorMz / 1000000;
                double lowMz = msMsPrecursorMz - mzToSearchTolerance;
                double highMz = msMsPrecursorMz + mzToSearchTolerance;

                foreach (var grouping in lipidsGroupedByTarget)
                {
                    LipidTarget lipidTarget = grouping.Key;
                    //double lipidMz = lipidTarget.Composition.Mass; //change to real mz
                    double lipidMz = lipidTarget.MzRounded;

                    // If we reached the point where the m/z is too high, we can exit
                    if (lipidMz > highMz) break;

                    if (lipidMz > lowMz)
                    {
                        // Find the MS1 data
                        //Xic xic = lcmsRun.GetPrecursorExtractedIonChromatogram(lipidMz, hcdTolerance, i);
                        Xic xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(lipidMz, hcdTolerance);

                        // Bogus data
                        if (precursorSpectrum != null && (xic.GetApexScanNum() < 0 || xic.GetSumIntensities() <= 0)) continue;

                        // Grab the MS/MS peak to search for
                        IEnumerable<MsMsSearchUnit> msMsSearchUnits = lipidTarget.GetMsMsSearchUnits();

                        // Get all matching peaks
                        List<MsMsSearchResult> hcdSearchResultList = hcdSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
                        List<MsMsSearchResult> cidSearchResultList = cidSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();

                        // Create spectrum search results
                        SpectrumSearchResult spectrumSearchResult = null;
                        LipidGroupSearchResult lipidGroupSearchResult = null;
                        if (precursorSpectrum != null)
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
                        }
                        else //If there are no precursor scans in this file
                        {
                            spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, hcdSearchResultList, cidSearchResultList, lcmsRun)
                            {
                                PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                            };
                            lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult);
                        }

                       
                        lipidGroupSearchResultList.Add(lipidGroupSearchResult);

                        //textWriter.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
                        //Console.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
                    }
                }

                // Skip an extra scan if we look at 2 at a time
                if (useTwoScans) i++;

                // Report progress
                if (progress != null)
                {
                    int currentProgress = (int)((i / maxLcScan) * 100);
                    progress.Report(currentProgress);
                }
            }

            //textWriter.Close();
            return lipidGroupSearchResultList;
        }

        public void RunGlobalWorkflowSingleScan()
        {
            throw new NotImplementedException();
        }

        public MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, double hcdMassError, IProgress<int> progress = null)
        {
            return RunMassCalibration(lipidList, this.LcMsRun, hcdMassError, progress);
        }

        public static MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, IProgress<int> progress = null)
        {
            List<double> ppmErrorList = new List<double>();

            var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList();

            foreach (var kvp in lipidsGroupedByTarget)
            {
                LipidTarget target = kvp.Key;

                var spectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(target, lcmsRun, hcdMassError, 500);

                if (spectrumSearchResultList.Any())
                {
                    var targetIon = new Ion(target.Composition - Composition.Hydrogen, 1);
                    double targetMz = targetIon.GetMonoIsotopicMz();

                    var bestSpectrumSearchResult = spectrumSearchResultList.OrderBy(x => x.Score).First();

                    var massSpectrum = bestSpectrumSearchResult.PrecursorSpectrum.Peaks;
                    var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();

                    double ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
                    ppmErrorList.Add(ppmError);
                }
            }

            SortedDictionary<double, int> ppmHistogram = QcUtil.CalculateHistogram(ppmErrorList, hcdMassError, 0.25);

            MassCalibrationResults massCalibrationResults = new MassCalibrationResults(ppmHistogram);
            return massCalibrationResults;
        }

        private static ActivationMethodCombination FigureOutActivationMethodCombination(LcMsRun lcmsRun)
        {
            List<int> Ms1ScanNumbers = lcmsRun.GetScanNumbers(1).ToList();
            List<int> Ms2ScanNumbers = lcmsRun.GetScanNumbers(2).ToList();
            ProductSpectrum firstMsMsSpectrum = null;
            int firstMsMsScanNumber = 0;
            if (Ms1ScanNumbers.Count > 0)
            {
                // Grab an MS1 Scan thats about 33% through the file so that we get accurate MS2 data
                int indexToGrab = (int) Math.Floor(Ms1ScanNumbers.Count/3.0);
                int ms1ScanNumberInMiddleOfRun = Ms1ScanNumbers[indexToGrab];

                firstMsMsScanNumber = lcmsRun.GetNextScanNum(ms1ScanNumberInMiddleOfRun, 2);

                // Lookup the first MS/MS Spectrum
                firstMsMsSpectrum = lcmsRun.GetSpectrum(firstMsMsScanNumber) as ProductSpectrum;
            }
            else
            {
                int indexToGrab = (int) Math.Floor(Ms2ScanNumbers.Count/3.0);
                int ms2ScanNumberInMiddleOfRun = Ms2ScanNumbers[indexToGrab];
                firstMsMsSpectrum = lcmsRun.GetSpectrum(ms2ScanNumberInMiddleOfRun) as ProductSpectrum;
                firstMsMsScanNumber = firstMsMsSpectrum.ScanNum;
            }

            if(firstMsMsSpectrum == null) return ActivationMethodCombination.Unsupported;

            // Lookup the second MS/MS Spectrum
            int nextMsMsScanNumber = lcmsRun.GetNextScanNum(firstMsMsScanNumber, 2);
            ProductSpectrum nextMsMsSpectrum = lcmsRun.GetSpectrum(nextMsMsScanNumber) as ProductSpectrum;

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
    }
}
