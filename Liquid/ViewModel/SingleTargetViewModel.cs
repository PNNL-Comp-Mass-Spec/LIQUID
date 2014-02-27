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
using LiquidBackend.IO;
using LiquidBackend.Util;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
	public class SingleTargetViewModel : ViewModelBase
	{
		public LcMsRun LcMsRun { get; private set; }
		public string RawFileName { get; private set; }
		public LipidTarget CurrentLipidTarget { get; private set; }
		public List<FragmentationMode> FragmentationModeList { get; private set; }
		public List<SpectrumSearchResult> SpectrumSearchResultList { get; private set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
		public List<Adduct> AdductList { get; private set; }
		public List<Lipid> LipidTargetList { get; private set; }
		public List<LipidGroupSearchResult> LipidGroupSearchResultList { get; private set; }

		public int LipidTargetLoadProgress { get; private set; }
		public int GlobalWorkflowProgress { get; private set; }

		public SingleTargetViewModel()
		{
			this.RawFileName = "None Loaded";
			this.FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
			this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Ammonium, Adduct.Acetate };
			this.SpectrumSearchResultList = new List<SpectrumSearchResult>();
			this.LipidTargetList = new List<Lipid>();

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

		public void LoadMoreLipidTargets(string fileLocation)
		{
			IProgress<int> progress = new Progress<int>(ReportLipidTargetLoadProgress);

			FileInfo fileInfo = new FileInfo(fileLocation);

			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo, progress);

			foreach (var lipid in lipidList)
			{
				this.LipidTargetList.Add(lipid);
			}

			OnPropertyChanged("LipidTargetList");

			//// Reset and populate the list of lipid classes
			//this.LipidClassList = new List<LipidClass>();
			//var groupByLipidClass = this.LipidTargetList.GroupBy(x => x.LipidTarget.LipidClass);
			//foreach (LipidClass lipidClass in groupByLipidClass.Select(@group => @group.Key))
			//{
			//	this.LipidClassList.Add(lipidClass);
			//}

			//if (this.LipidClassList.Count > 0)
			//{
			//	LipidClass lipidClass = this.LipidClassList[0];
			//	this.CurrentLipidClass = lipidClass;
			//	this.CurrentLipidTargetList = this.LipidTargetList.Where(x => x.LipidClass == lipidClass).ToList();

			//	OnPropertyChanged("CurrentLipidClass");
			//}

			//OnPropertyChanged("LipidClassList");
			//OnPropertyChanged("CurrentLipidTargetList");

			// Reset the progress bar back to 0
			progress.Report(0);
		}

		public void OnProcessAllTarget(double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
		{
			IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);

			// Make sure to only look at targets that match the fragmentation mode
			var targetsToProcess = this.LipidTargetList.Where(x => x.LipidTarget.FragmentationMode == fragmentationMode);

			// Run global analysis
			this.LipidGroupSearchResultList = new List<LipidGroupSearchResult>();
			var lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflow(targetsToProcess, this.LcMsRun, hcdError, cidError, progress);

			// Group results of same scan together
			var resultsGroupedByScan = lipidGroupSearchResultList.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

			// Grab the result(s) with the best score
			foreach (var group in resultsGroupedByScan)
			{
				var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

				for (int i = 0; i < numResultsPerScanToInclude && i < groupOrdered.Count; i++)
				{
					LipidGroupSearchResult resultToAdd = groupOrdered[i];
					this.LipidGroupSearchResultList.Add(resultToAdd);
				}
			}

			OnPropertyChanged("LipidGroupSearchResultList");

			// Reset the progress bar back to 0
			progress.Report(0);
		}

		private void ReportLipidTargetLoadProgress(int value)
		{
			this.LipidTargetLoadProgress = value;
			OnPropertyChanged("LipidTargetLoadProgress");
		}

		private void ReportGlobalWorkflowProgress(int value)
		{
			this.GlobalWorkflowProgress = value;
			OnPropertyChanged("GlobalWorkflowProgress");
		}

		private async void WarmUpInformedProteomics()
		{
			await Task.Run(() => Composition.H2O.ComputeApproximateIsotopomerEnvelop());
		}
	}
}
