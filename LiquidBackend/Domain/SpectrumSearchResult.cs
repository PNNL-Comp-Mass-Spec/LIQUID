using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;

namespace LiquidBackend.Domain
{
	public class SpectrumSearchResult
	{
		public Spectrum PrecursorSpectrum { get; private set; }
        public Tolerance PrecursorTolerance { get; set; }
		public ProductSpectrum HcdSpectrum { get; private set; }
		public ProductSpectrum CidSpectrum { get; private set; }
		public List<MsMsSearchResult> HcdSearchResultList { get; private set; }
		public List<MsMsSearchResult> CidSearchResultList { get; private set; }
		public Xic Xic { get; private set; }
		public LcMsRun LcMsRun { get; private set; }
        public double RunLength { get; private set; }
        public bool ShouldExport { get; set; }

		public int NumMatchingMsMsPeaks
		{
			get { return GetNumMatchingMsMsPeaks(); }
		}

		public int ApexScanNum { get; private set; }

	    public string StrippedDisplay
	    {
	        get { return HcdSpectrum != null? HcdSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString(): CidSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString(); }
	    }

	    public int DisplayScanNum
	    {
	        get 
            {
	            return HcdSpectrum != null ? HcdSpectrum.ScanNum : CidSpectrum.ScanNum;
	        }
	    }

	    public double ApexIntensity { get; private set; }

		//Implemented by grant. Used as a way to pass the area under the curve selected by the user to the exported results.
		public double? PeakArea { get; set; }

	    public double RetentionTime { get; private set; }

		public double NormalizedElutionTime
		{
			get
            { 
			    return RetentionTime/RunLength;
		    }
		}

	    public int NumObservedPeaks
	    {
	        get
	        {
	            return this.HcdSearchResultList.Count(x => x.ObservedPeak != null) + this.CidSearchResultList.Count(x => x.ObservedPeak != null);
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
				return (this.HcdSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity) + this.CidSearchResultList.Where(x => x.ObservedPeak != null).Sum(x => x.ObservedPeak.Intensity));
			}
		}

		public SpectrumSearchResult(ProductSpectrum hcdSpectrum, ProductSpectrum cidSpectrum, Spectrum precursorSpectrum, List<MsMsSearchResult> hcdSearchResultList, List<MsMsSearchResult> cidSearchResultList, Xic xic, LcMsRun lcMsRun)
		{
			this.HcdSpectrum = hcdSpectrum;
			this.CidSpectrum = cidSpectrum;
			this.PrecursorSpectrum = precursorSpectrum;
			this.HcdSearchResultList = hcdSearchResultList;
			this.CidSearchResultList = cidSearchResultList;
			this.Xic = xic;
			this.LcMsRun = lcMsRun;
			this.PeakArea = null;
		    this.RunLength = lcMsRun.GetElutionTime(lcMsRun.MaxLcScan);
            this.ApexScanNum = this.Xic.GetNearestApexScanNum(this.PrecursorSpectrum.ScanNum, true);
            this.ApexIntensity = this.Xic.Where(x => x.ScanNum == this.ApexScanNum).Sum(x => x.Intensity);
            this.RetentionTime = this.LcMsRun.GetElutionTime(this.ApexScanNum);
            
		}

		public int GetNumMatchingMsMsPeaks()
		{
			return (this.HcdSearchResultList.Where(x => x.ObservedPeak != null).Union(this.CidSearchResultList.Where(x => x.ObservedPeak != null))).Count();
		}
	}
}
