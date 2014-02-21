using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;

namespace LiquidBackend.Domain
{
	public class SpectrumSearchResult
	{
		public Spectrum PrecursorSpectrum { get; private set; }
		public ProductSpectrum HcdSpectrum { get; private set; }
		public ProductSpectrum CidSpectrum { get; private set; }
		public List<MsMsSearchResult> HcdSearchResultList { get; private set; }
		public List<MsMsSearchResult> CidSearchResultList { get; private set; }
		public Xic Xic { get; private set; }

		public int NumMatchingMsMsPeaks
		{
			get { return GetNumMatchingMsMsPeaks(); }
		}

		public int ApexScanNum
		{
			get { return this.Xic.GetNearestApexScanNum(this.PrecursorSpectrum.ScanNum, true); }
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

		public SpectrumSearchResult(ProductSpectrum hcdSpectrum, ProductSpectrum cidSpectrum, Spectrum precursorSpectrum, List<MsMsSearchResult> hcdSearchResultList, List<MsMsSearchResult> cidSearchResultList, Xic xic)
		{
			this.HcdSpectrum = hcdSpectrum;
			this.CidSpectrum = cidSpectrum;
			this.PrecursorSpectrum = precursorSpectrum;
			this.HcdSearchResultList = hcdSearchResultList;
			this.CidSearchResultList = cidSearchResultList;
			this.Xic = xic;
		}

		public int GetNumMatchingMsMsPeaks()
		{
			return (this.HcdSearchResultList.Where(x => x.ObservedPeak != null).Union(this.CidSearchResultList.Where(x => x.ObservedPeak != null))).Count();
		}
	}
}
