using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Scoring;

namespace LiquidBackend.Domain
{
    public class SpectrumSearchResult
    {
        public Spectrum PrecursorSpectrum { get; }
        public Tolerance PrecursorTolerance { get; set; }
        public ProductSpectrum HcdSpectrum { get; }
        public ProductSpectrum CidSpectrum { get; }
        public List<MsMsSearchResult> HcdSearchResultList { get; }
        public List<MsMsSearchResult> CidSearchResultList { get; }
        public Xic Xic { get; }
        public LcMsRun LcMsRun { get; }
        public double RunLength { get; }
        public bool ShouldExport { get; set; }
        public double ModelScore { get; set; }

        public int NumMatchingMsMsPeaks => GetNumMatchingMsMsPeaks();

        public int ApexScanNum { get; }

        public string StrippedDisplay => HcdSpectrum?.IsolationWindow.IsolationWindowTargetMz.ToString(CultureInfo.InvariantCulture) ??
            CidSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString(CultureInfo.InvariantCulture);

        public int DisplayScanNum => HcdSpectrum?.ScanNum ?? CidSpectrum.ScanNum;

        public double ApexIntensity { get; }

        //Implemented by grant. Used as a way to pass the area under the curve selected by the user to the exported results.
        public double? PeakArea { get; set; }

        public double RetentionTime { get; }

        public double NormalizedElutionTime => RetentionTime/RunLength;

        public int NumObservedPeaks
        {
            get
            {
                return HcdSearchResultList.Count(x => x.ObservedPeak != null) + CidSearchResultList.Count(x => x.ObservedPeak != null);
            }
        }

        public double Score
        {
            get
            {
                //XicPoint searchPoint = new XicPoint(this.PrecursorSpectrum.ScanNum, 0);
                //int index = this.Xic.BinarySearch(searchPoint);
                //double intensityOfPrecursor = this.Xic[index].Intensity;
                //return (this.HcdSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity) + this.CidSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity)) / intensityOfPrecursor;
                return (HcdSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity) + CidSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity));
            }
        }

        public SpectrumSearchResult(ProductSpectrum hcdSpectrum, ProductSpectrum cidSpectrum, Spectrum precursorSpectrum, List<MsMsSearchResult> hcdSearchResultList, List<MsMsSearchResult> cidSearchResultList, Xic xic, LcMsRun lcMsRun, ScoreModel scoreModel = null, LipidTarget lipidTarget = null)
        {
            HcdSpectrum = hcdSpectrum;
            CidSpectrum = cidSpectrum;
            PrecursorSpectrum = precursorSpectrum;
            HcdSearchResultList = hcdSearchResultList;
            CidSearchResultList = cidSearchResultList;
            Xic = xic;
            LcMsRun = lcMsRun;
            PeakArea = null;
            RunLength = lcMsRun.GetElutionTime(lcMsRun.MaxLcScan);
            ApexScanNum = Xic.GetNearestApexScanNum(PrecursorSpectrum.ScanNum, performSmoothing: true);
            ApexIntensity = Xic.Where(x => x.ScanNum == ApexScanNum).Sum(x => x.Intensity);
            RetentionTime = LcMsRun.GetElutionTime(ApexScanNum);
            if(scoreModel != null && lipidTarget != null) ModelScore = scoreModel.ScoreLipid(lipidTarget, this);
        }

        public SpectrumSearchResult(ProductSpectrum hcdSpectrum, ProductSpectrum cidSpectrum, List<MsMsSearchResult> hcdSearchResultList, List<MsMsSearchResult> cidSearchResultList, LcMsRun lcMsRun, ScoreModel scoreModel = null, LipidTarget lipidTarget = null)
        {
            HcdSpectrum = hcdSpectrum;
            CidSpectrum = cidSpectrum;
            PrecursorSpectrum = null;
            HcdSearchResultList = hcdSearchResultList;
            CidSearchResultList = cidSearchResultList;
            Xic = null;
            LcMsRun = lcMsRun;
            PeakArea = null;
            RunLength = lcMsRun.GetElutionTime(lcMsRun.MaxLcScan);
            ApexScanNum = 0;
            ApexIntensity = 0;
            RetentionTime = LcMsRun.GetElutionTime(hcdSpectrum?.ScanNum ?? cidSpectrum.ScanNum);
            if (scoreModel != null && lipidTarget != null) ModelScore = scoreModel.ScoreLipid(lipidTarget, this);
        }

        public int GetNumMatchingMsMsPeaks()
        {
            return (HcdSearchResultList.Where(x => x.ObservedPeak != null).Union(CidSearchResultList.Where(x => x.ObservedPeak != null))).Count();
        }
    }
}
