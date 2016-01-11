using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using Liquid.OxyPlot;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Scoring;
using LiquidBackend.Util;
using Ookii.Dialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using UIMFLibrary;

namespace Liquid.ViewModel
{
	public class SingleTargetViewModel : ViewModelBase
	{
		public LcMsRun LcMsRun { get; private set; }
        public DataReader ImsRun { get; private set; }
        public string FeatureFilePath { get; private set; } //Probably replace with a feature table
		public string RawFileName { get; private set; }
		public LipidTarget CurrentLipidTarget { get; private set; }
		public List<FragmentationMode> FragmentationModeList { get; private set; }
		public List<SpectrumSearchResult> SpectrumSearchResultList { get; private set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
		public List<Adduct> AdductList { get; private set; }
		public List<Lipid> LipidTargetList { get; private set; }
        public List<Tuple<string, int>> LipidIdentifications { get; private set; } 
		public List<LipidGroupSearchResult> LipidGroupSearchResultList { get; private set; }
		public ScoreModel ScoreModel { get; private set; }
        public List<ImsFeature> ImsFeatureTargets { get; private set; } 

		public int LipidTargetLoadProgress { get; private set; }
		public int GlobalWorkflowProgress { get; private set; }
        public int ExportProgress { get; private set; }
        public bool IsIms { get; private set; }

		public SingleTargetViewModel()
		{
			this.RawFileName = "None Loaded";
			this.FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
			this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Ammonium, Adduct.Acetate };
			this.SpectrumSearchResultList = new List<SpectrumSearchResult>();
			this.LipidTargetList = new List<Lipid>();
            this.LipidIdentifications = new List<Tuple<string, int>>();
			this.ScoreModel = ScoreModelSerialization.Deserialize("DefaultScoringModel.xml");

		}

		public void UpdateRawFileLocation(string rawFileLocation, ref bool findFileFlag)
		{
			FileInfo rawFileInfo = new FileInfo(rawFileLocation);
		    this.IsIms = Path.GetExtension(rawFileLocation).ToLower() == ".uimf";

			this.RawFileName = rawFileInfo.Name;
			OnPropertyChanged("RawFileName");

		    if (IsIms)
		    {
                this.ImsRun = new DataReader(rawFileLocation);

		        var featureFilePath = Directory.GetFiles(Path.GetDirectoryName(rawFileLocation),
		            String.Format("{0}{1}", Path.GetFileNameWithoutExtension(rawFileLocation), "_LCMSFeatures.txt"), SearchOption.AllDirectories).FirstOrDefault();
		        if (featureFilePath == null)
		        {
		            DialogResult FindFeatureFile =
		                MessageBox.Show("Unable to find LCMSFeatures file. Please specify the file location.", "Locate File",
		                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
		            if (FindFeatureFile == DialogResult.OK)
		            {
		                findFileFlag = true;
		            }
		            else if (FindFeatureFile == DialogResult.Cancel)
		            {
		                this.ImsRun = null;
		            }
		        }
		        else
		        {
		            BuildImsFeatureList(featureFilePath);
		        }
                OnPropertyChanged("ImsRun");
                OnPropertyChanged("ImsFeatureTargets");
		    }
		    else
		    {
		        this.LcMsRun = LcMsDataFactory.GetLcMsData(rawFileLocation);
                OnPropertyChanged("LcMsRun");
		    }
		    
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

	    public void LoadLipidIdentifications(string fileLocation)
	    {
            FileInfo fileInfo = new FileInfo(fileLocation);

            OutputFileReader<Tuple<string,int>> identificationReader = new OutputFileReader<Tuple<string, int>>();
	        List<Tuple<string, int>> idList = identificationReader.ReadFile(fileInfo);

	        foreach (var id in idList)
	        {
	            if (!this.LipidIdentifications.Contains(id))
	            {
	                this.LipidIdentifications.Add(id);
	            }
	        }
	        if (this.LipidGroupSearchResultList != null)
	        {
                SelectLipidIdentifications(this.LipidGroupSearchResultList);
	        }
            
            OnPropertyChanged("LipidIdentifications");
	    }

	    public void BuildImsFeatureList(string featureFileName)
	    {
            FileInfo featureFile = new FileInfo(featureFileName);
            ImsFeatureReader<ImsFeature> featureReader = new ImsFeatureReader<ImsFeature>();
	        this.ImsFeatureTargets = featureReader.ReadFile(featureFile);
	    }



		public void OnProcessAllTarget(double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
		{
			IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);

			// Make sure to only look at targets that match the fragmentation mode
			var targetsToProcess = this.LipidTargetList.Where(x => x.LipidTarget.FragmentationMode == fragmentationMode);

			// Run global analysis
			this.LipidGroupSearchResultList = new List<LipidGroupSearchResult>();
            
            var lipidGroupSearchResultList = new List<LipidGroupSearchResult>();
		    if (IsIms)
		    {
		        lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflow(targetsToProcess, this.ImsRun, this.ImsFeatureTargets,
		            hcdError, cidError, this.ScoreModel, progress);
		    }
		    else
		    {
		        lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflow(targetsToProcess, this.LcMsRun, hcdError,
		            cidError, this.ScoreModel, progress);
		    }
		    // If identifications have been loaded, select them in the view
		    if (this.LipidIdentifications.Count != 0)
		    {
		        SelectLipidIdentifications(lipidGroupSearchResultList);
		    }

			// Group results of same scan together
			var resultsGroupedByScan = lipidGroupSearchResultList.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum != null ? x.SpectrumSearchResult.HcdSpectrum.ScanNum : x.SpectrumSearchResult.CidSpectrum.ScanNum);

			// Grab the result(s) with the best score
			foreach (var group in resultsGroupedByScan)
			{
				var groupOrdered = group.OrderByDescending(x => x.Score).ToList();

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

	    public void SelectLipidIdentifications(List<LipidGroupSearchResult> lipidGroupSearchResultList)
	    {
	        foreach (var id in this.LipidIdentifications)
	        {
	            foreach (var lipid in lipidGroupSearchResultList)
	            {
	                string name = id.Item1;
	                int scan = id.Item2;
	                if (lipid.LipidTarget.StrippedDisplay == name && lipid.SpectrumSearchResult.HcdSpectrum.ScanNum == scan)
	                {
	                    lipid.ShouldExport = true;
	                    break;
	                }
	            }
	        }
	    }

	    public void OnExportGlobalResults(string fileLocation)
		{
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
			var resultsToExport = LipidGroupSearchResultList.Where(x => x.ShouldExport);
			LipidGroupSearchResultWriter.OutputResults(resultsToExport, fileLocation, RawFileName, progress);
            progress.Report(0);
		}

		public void OnExportAllGlobalResults(string fileLocation)
		{
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
			LipidGroupSearchResultWriter.OutputResults(LipidGroupSearchResultList, fileLocation, RawFileName, progress);
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

	    private void ReportExportProgress(int value)
	    {
	        this.ExportProgress = value;
            OnPropertyChanged("ExportProgress");
	    }

	}
}
