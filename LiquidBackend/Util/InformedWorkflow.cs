﻿using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Scoring;
using PRISM;

namespace LiquidBackend.Util
{
    public class InformedWorkflow
    {
        // Ignore Spelling: protonated, xic

        public LcMsRun LcMsRun { get; }

        public InformedWorkflow(string rawFileLocation)
        {
            var dataFactory = new LcMsDataFactory();
            dataFactory.ProgressChanged += LcMsDataFactory_ProgressChanged;

            LcMsRun = dataFactory.GetLcMsData(rawFileLocation);
        }

        public List<SpectrumSearchResult> RunInformedWorkflow(LipidTarget target, double hcdMassError, double cidMassError)
        {
            return RunInformedWorkflow(target, LcMsRun, hcdMassError, cidMassError);
        }

        public static List<SpectrumSearchResult> RunInformedWorkflow(
            LipidTarget target,
            LcMsRun lcmsRun,
            double hcdMassError,
            double cidMassError,
            ScoreModel scoreModel = null)
        {
            IEnumerable<MsMsSearchUnit> msMsSearchUnits = target.GetMsMsSearchUnits();

            var targetMz = target.MzRounded;
            var hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            var cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

            var fragScanPairs = GlobalWorkflow.GetSortedMsMsScans(lcmsRun);

            // Find out which MS/MS scans have a precursor m/z that matches the target
            var matchingScanPairs = GetFilteredFragmentationScanPairs(lcmsRun, fragScanPairs, targetMz);

            var spectrumSearchResultList = new List<SpectrumSearchResult>();

            var scanPairCount = matchingScanPairs.Count;

            for (var i = 0; i < scanPairCount; i++)
            {
                var scanPair = fragScanPairs[i];

                ProductSpectrum firstMsMsSpectrum;
                ProductSpectrum secondMsMsSpectrum;
                if (scanPair.HasTwoScans)
                {
                    firstMsMsSpectrum = lcmsRun.GetSpectrum(scanPair.FirstScan) as ProductSpectrum;
                    secondMsMsSpectrum = lcmsRun.GetSpectrum(scanPair.SecondScan) as ProductSpectrum;
                }
                else
                {
                    firstMsMsSpectrum = lcmsRun.GetSpectrum(scanPair.FirstScan) as ProductSpectrum;
                    secondMsMsSpectrum = null;
                }

                if (firstMsMsSpectrum == null)
                    continue;

                // Filter MS/MS Spectrum based on mass error
                var msMsPrecursorMz = firstMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;

                //if (Math.Abs(msMsPrecursorMz - targetMz) > 0.4) continue;
                var ppmError = LipidUtil.PpmError(targetMz, msMsPrecursorMz);
                if (Math.Abs(ppmError) > hcdMassError)
                    continue;

                // Assign each MS/MS spectrum to HCD or CID
                ProductSpectrum hcdSpectrum;
                ProductSpectrum cidSpectrum;
                if (scanPair.HasTwoScans)
                {
                    hcdSpectrum = firstMsMsSpectrum;
                    cidSpectrum = secondMsMsSpectrum;
                }
                else
                {
                    cidSpectrum = firstMsMsSpectrum;
                    hcdSpectrum = null;
                }

                // Get all matching peaks
                var hcdSearchResultList = hcdSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
                var cidSearchResultList = cidSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();

                // Find the MS1 data
                // Xic xic = lcmsRun.GetPrecursorExtractedIonChromatogram(targetMz, hcdTolerance, firstScanNumber);

                var precursorSpectrum = lcmsRun.GetSpectrum(scanPair.PrecursorScanNumber);
                var xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(targetMz, hcdTolerance);

                // Bogus data
                if (precursorSpectrum != null && (xic.GetApexScanNum() < 0 || xic.GetSumIntensities() <= 0)) continue;

                SpectrumSearchResult spectrumSearchResult;
                if (precursorSpectrum != null)
                {
                    spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun, scoreModel, target)
                    {
                        PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                    };
                }
                else // If there are no precursor scans in this file
                {
                    spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, hcdSearchResultList, cidSearchResultList, lcmsRun, scoreModel, target)
                    {
                        PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm),
                    };
                }
                spectrumSearchResultList.Add(spectrumSearchResult);
            }

            return spectrumSearchResultList;
        }

        public static List<SpectrumSearchResult> RunFragmentWorkflow(ICollection<MsMsSearchUnit> fragments, LcMsRun lcmsRun, double hcdMassError, double cidMassError, int minMatches, IProgress<int> progress = null)
        {
            var PISearchUnits = fragments.Where(x => x.Description.Equals("Product Ion")).ToList();
            var hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
            var cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);
            var scanTracker = new List<int>(); //track what scans have been included in spectrumSearchResultsList so we don't make duplicate entries for matched CID and HCD

            // Find all MS/MS scans
            var msmsScanNumbers = lcmsRun.GetScanNumbers(2);
            var spectrumSearchResultList = new List<SpectrumSearchResult>();
            var maxScans = msmsScanNumbers.Count;

            foreach (var scan in msmsScanNumbers)
            {
                // Lookup the MS/MS Spectrum
                if (lcmsRun.GetSpectrum(scan) is not ProductSpectrum MsMsSpectrum)
                    continue;

                ProductSpectrum MatchedSpectrum = null;
                var spectrum1 = lcmsRun.GetSpectrum(scan + 1);
                var spectrum2 = lcmsRun.GetSpectrum(scan - 1);
                if (spectrum1?.MsLevel == 2 && spectrum1 is ProductSpectrum productSpectrum1)
                {
                    var deltaMz = productSpectrum1.IsolationWindow.IsolationWindowTargetMz - MsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;
                    if (Math.Abs(deltaMz) < float.Epsilon)
                    {
                        MatchedSpectrum = productSpectrum1;
                    }
                }

                if (spectrum2?.MsLevel == 2 && spectrum2 is ProductSpectrum productSpectrum2)
                {
                    var deltaMz = productSpectrum2.IsolationWindow.IsolationWindowTargetMz - MsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;
                    if (Math.Abs(deltaMz) < float.Epsilon)
                    {
                        MatchedSpectrum = productSpectrum2;
                    }
                }

                if (scanTracker.Contains(MsMsSpectrum.ScanNum))
                    continue;

                var msmsPrecursorMz = MsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;

                var xic = lcmsRun.GetFullPrecursorIonExtractedIonChromatogram(msmsPrecursorMz, hcdTolerance);

                // Bogus data
                //if (xic.GetApexScanNum() < 0) continue;
                var msmsPrecursorScan = 0;
                if (lcmsRun.MinMsLevel == 1) // Make sure there are precursor scans in file
                {
                    msmsPrecursorScan = lcmsRun.GetPrecursorScanNum(scan);
                }
                var precursorSpectrum = lcmsRun.GetSpectrum(msmsPrecursorScan);

                // Get all matching peaks

                //IEnumerable<MsMsSearchUnit> NLSearchUnits = fragments.Where(x=> x.Description.Equals("Neutral Loss")).Select(x => {x.Mz = (msmsPrecursorMz - x.Mz); return x;});
                var NLSearchUnits = fragments.Where(x => x.Description.Equals("Neutral Loss")).Select(y => new MsMsSearchUnit(msmsPrecursorMz - y.Mz, "Neutral Loss"));
                var MsMsSearchUnits = PISearchUnits.Concat(NLSearchUnits).ToList();
                SpectrumSearchResult spectrumSearchResult;

                var hcdSpectrum = MsMsSpectrum.ActivationMethod == ActivationMethod.HCD ? MsMsSpectrum : MatchedSpectrum;
                var cidSpectrum = MsMsSpectrum.ActivationMethod == ActivationMethod.CID ? MsMsSpectrum : MatchedSpectrum;

                var HcdSearchResultList = hcdSpectrum != null ? (from msMsSearchUnit in MsMsSearchUnits
                                                                 let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance)
                                                                 select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
                var CidSearchResultList = cidSpectrum != null ? (from msMsSearchUnit in MsMsSearchUnits
                                                                 let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance)
                                                                 select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
                var SearchResultList = HcdSearchResultList.Concat(CidSearchResultList).ToList();
                if (precursorSpectrum != null)
                {
                    spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum,
                        HcdSearchResultList, CidSearchResultList, xic, lcmsRun)
                    {
                        PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                    };
                }
                else
                {
                    spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, HcdSearchResultList, CidSearchResultList, lcmsRun)
                    {
                        PrecursorTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm)
                    };
                }

                if (hcdSpectrum != null) scanTracker.Add(hcdSpectrum.ScanNum);
                if (cidSpectrum != null) scanTracker.Add(cidSpectrum.ScanNum);

                if (SearchResultList.Count(x => x.ObservedPeak != null) < minMatches) continue;
                spectrumSearchResultList.Add(spectrumSearchResult);

                // Report progress
                if (progress != null)
                {
                    var currentProgress = (int)((double)scan / maxScans * 100);
                    progress.Report(currentProgress);
                }
            }

            return spectrumSearchResultList;
        }

        /// <summary>
        /// Gets scan numbers of the fragmentation spectra whose isolation window contains the precursor ion specified
        /// </summary>
        /// <param name="lcmsRun"></param>
        /// <param name="fragScanPairs"></param>
        /// <param name="mostAbundantIsotopeMz"></param>
        /// <returns>scan numbers of fragmentation spectra</returns>
        private static List<ScanPair> GetFilteredFragmentationScanPairs(ILcMsRun lcmsRun, IEnumerable<ScanPair> fragScanPairs, double mostAbundantIsotopeMz)
        {
            var matchingScanNumbers = new SortedSet<int>();

            foreach (var scanNumber in lcmsRun.GetFragmentationSpectraScanNums(mostAbundantIsotopeMz))
            {
                matchingScanNumbers.Add(scanNumber);
            }

            var matchingScanPairs = new List<ScanPair>();

            foreach (var scanPair in fragScanPairs)
            {
                if (matchingScanNumbers.Contains(scanPair.FirstScan))
                {
                    matchingScanPairs.Add(scanPair);
                }
            }

            return matchingScanPairs;
        }

        private void LcMsDataFactory_ProgressChanged(object sender, ProgressData e)
        {
            ProgressChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Raised for each reported progress value
        /// </summary>
        public event EventHandler<ProgressData> ProgressChanged;
    }
}
