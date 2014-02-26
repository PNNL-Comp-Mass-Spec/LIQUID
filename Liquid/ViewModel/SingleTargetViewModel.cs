using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using Liquid.OxyPlot;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
	public class SingleTargetViewModel : ViewModelBase
	{
		public LcMsRun LcMsRun { get; set; }
		public string RawFileName { get; set; }
		public LipidTarget CurrentLipidTarget { get; set; }
		public List<FragmentationMode> FragmentationModeList { get; set; }
		public List<SpectrumSearchResult> SpectrumSearchResultList { get; set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; set; }
		public List<Adduct> AdductList { get; set; }

		public SingleTargetViewModel()
		{
			this.RawFileName = "None Loaded";
			this.FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
			this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Ammonium, Adduct.Acetate };
			this.SpectrumSearchResultList = new List<SpectrumSearchResult>();

			// Run asynchronously inside constructor to avoid slow functionality on first target search
			this.WarmUpInformedProteomics();
		}

		public void UpdateRawFileLocation(string rawFileLocation)
		{
			FileInfo rawFileInfo = new FileInfo(rawFileLocation);

			this.RawFileName = rawFileInfo.Name;
			OnPropertyChanged("RawFileName");

			this.LcMsRun = LcMsRun.GetLcMsRun(rawFileLocation, MassSpecDataType.XCaliburRun);
			OnPropertyChanged("LcMsRun");
		}

		public void SearchForTarget(string commonName, Adduct adduct, FragmentationMode fragmentationMode, double hcdMassError, double cidMassError)
		{
			this.CurrentLipidTarget = LipidUtil.CreateLipidTarget(commonName, fragmentationMode, adduct);
			OnPropertyChanged("CurrentLipidTarget");

			this.SpectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(this.CurrentLipidTarget, this.LcMsRun, hcdMassError, cidMassError);
			OnPropertyChanged("SpectrumSearchResultList");

			if (this.SpectrumSearchResultList.Any())
			{
				SpectrumSearchResult spectrumSearchResult = this.SpectrumSearchResultList.OrderByDescending(x => x.NumMatchingMsMsPeaks).First();
				OnSpectrumSearchResultChange(spectrumSearchResult);
			}
			else
			{
				this.CurrentSpectrumSearchResult = null;
			}
		}

		public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
		{
			this.CurrentSpectrumSearchResult = spectrumSearchResult;
			OnPropertyChanged("CurrentSpectrumSearchResult");
		}

		private async void WarmUpInformedProteomics()
		{
			await Task.Run(() => Composition.H2O.ComputeApproximateIsotopomerEnvelop());
		}
	}
}
