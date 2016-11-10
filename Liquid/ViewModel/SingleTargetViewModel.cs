using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace Liquid.ViewModel
{
	public class SingleTargetViewModel : ViewModelBase
	{
		public LcMsRun LcMsRun { get; private set; }
        public string FeatureFilePath { get; private set; } //Probably replace with a feature table
		public string RawFileName { get; private set; }
		public LipidTarget CurrentLipidTarget { get; private set; }
		public List<FragmentationMode> FragmentationModeList { get; private set; }
		public List<SpectrumSearchResult> SpectrumSearchResultList { get; private set; }
        public ObservableCollection<MsMsSearchUnit> FragmentSearchList { get; private set; }  
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
		public List<Adduct> AdductList { get; private set; }
        public List<string> IonTypeList { get; private set; } 
		public List<Lipid> LipidTargetList { get; private set; }
        public List<Tuple<string, int>> LipidIdentifications { get; private set; } 
		public List<LipidGroupSearchResult> LipidGroupSearchResultList { get; private set; }
		public ScoreModel ScoreModel { get; private set; }
        public Adduct TargetAdduct { get; set; }
        public FragmentationMode TargetFragmentationMode { get; set; }
         

		public int LipidTargetLoadProgress { get; private set; }
		public int GlobalWorkflowProgress { get; private set; }
        public int FragmentSearchProgress { get; private set; }
        public int ExportProgress { get; private set; }
        public bool IsIms { get; private set; }
        public bool AverageSpec { get; set; }

		public SingleTargetViewModel()
		{
			this.RawFileName = "None Loaded";
			this.FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
			//this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Dihydrogen, Adduct.Ammonium, Adduct.Acetate };
		    this.AdductList = Enum.GetValues(typeof (Adduct)).Cast<Adduct>().ToList();
            this.IonTypeList = new List<string>{"Primary Ion", "Neutral Loss"};
			this.SpectrumSearchResultList = new List<SpectrumSearchResult>();
			this.LipidTargetList = new List<Lipid>();
		    this.FragmentSearchList = new ObservableCollection<MsMsSearchUnit>();
            this.LipidIdentifications = new List<Tuple<string, int>>();
			this.ScoreModel = ScoreModelSerialization.Deserialize("DefaultScoringModel.xml");
		    this.AverageSpec = false;

		}

		public void UpdateRawFileLocation(string rawFileLocation)
		{
			FileInfo rawFileInfo = new FileInfo(rawFileLocation);
		    this.IsIms = Path.GetExtension(rawFileLocation).ToLower() == ".uimf";

			this.RawFileName = rawFileInfo.Name;
			OnPropertyChanged("RawFileName");

		    if (IsIms)
		    {
		        //TODO: IMS Workflow
		    }
		    else
		    {
                if(this.LcMsRun != null) this.LcMsRun.Close();
		        this.LcMsRun = LcMsDataFactory.GetLcMsData(rawFileLocation);
                OnPropertyChanged("LcMsRun");
		    }
		    
		}

		public void SearchForTarget(string commonName, Adduct adduct, FragmentationMode fragmentationMode, double hcdMassError, double cidMassError)
		{
			this.CurrentLipidTarget = LipidUtil.CreateLipidTarget(commonName, fragmentationMode, adduct);
			OnPropertyChanged("CurrentLipidTarget");

			this.SpectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(this.CurrentLipidTarget, this.LcMsRun, hcdMassError, cidMassError, this.ScoreModel);
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

	    public void OnUpdateTargetAdductFragmentation(Adduct adduct, FragmentationMode fragmode)
	    {
	        this.TargetAdduct = adduct;
	        this.TargetFragmentationMode = fragmode;
            OnPropertyChanged("TargetAdduct");
            OnPropertyChanged("TargetFragmentationMode");
	    }

		public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
		{
			this.CurrentSpectrumSearchResult = spectrumSearchResult;
			OnPropertyChanged("CurrentSpectrumSearchResult");
            OnPropertyChanged("CurrentLipidTarget");
		}

        public void OnMsMsSearchResultChange(SpectrumSearchResult spectrumSearchResult)
        {
            this.CurrentSpectrumSearchResult = spectrumSearchResult;
            this.CurrentLipidTarget = LipidUtil.CreateLipidTarget((spectrumSearchResult.HcdSpectrum ?? spectrumSearchResult.CidSpectrum).IsolationWindow.IsolationWindowTargetMz, this.TargetFragmentationMode, this.TargetAdduct);

            OnPropertyChanged("CurrentSpectrumSearchResult");
            OnPropertyChanged("CurrentLipidTarget");
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

	        OutputFileReader<Tuple<string, int>> identificationReader = new OutputFileReader<Tuple<string, int>>();
	        List<Tuple<string, int>> idList = identificationReader.ReadFile(fileInfo);

	        foreach (var id in idList)
	        {
	            if (!this.LipidIdentifications.Contains(id))
	            {
	                this.LipidIdentifications.Add(id);
	            }
	        }            
	        SelectLipidIdentifications(this.LipidGroupSearchResultList);
	        OnPropertyChanged("LipidIdentifications");
	    }

        public void OnBuildLibrary(IList<string> filesList, double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
	    {

	        foreach (var file in filesList)
	        {
                StreamReader reader = new StreamReader(file);
	            var header = reader.ReadLine().Split(new char[]{'\t'}).ToList();
	            var index = header.IndexOf("Raw Data File");
	            if (index != -1)
	            {
	                var rawFileName = reader.ReadLine().Split(new char[]{'\t'})[index];
                    LibraryBuilder.AddDmsDataset(rawFileName);
	                UpdateRawFileLocation(rawFileName);
                    OnProcessAllTarget(hcdError, cidError, fragmentationMode, numResultsPerScanToInclude);
	                LoadLipidIdentifications(file);
	                OnExportGlobalResults(file.Replace(".tsv", ".msp"));
                    
                    //Delete the raw files we copied from DMS to save space
                    this.LcMsRun.Close();
	                this.LcMsRun = null;
                    OnPropertyChanged("LcMsRun");
                    GC.Collect();


                    File.Delete(rawFileName);
                    File.Delete(rawFileName.Replace(Path.GetExtension(rawFileName),"pbf"));
	            }
	        }

	    }

		public void OnProcessAllTarget(double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
		{
			IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);

			// Make sure to only look at targets that match the fragmentation mode
			var targetsToProcess = this.LipidTargetList.Where(x => x.LipidTarget.FragmentationMode == fragmentationMode);

			// Run global analysis
			this.LipidGroupSearchResultList = new List<LipidGroupSearchResult>();

		    IEnumerable<IGrouping<Double, LipidGroupSearchResult>> resultsGrouped = null;
            var lipidGroupSearchResultList = new List<LipidGroupSearchResult>();
		    if (AverageSpec)
		    {
		        lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflowAvgSpec(targetsToProcess, this.LcMsRun, hcdError, cidError, this.ScoreModel, progress);
                resultsGrouped = lipidGroupSearchResultList.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum != null ? x.SpectrumSearchResult.HcdSpectrum.IsolationWindow.IsolationWindowTargetMz : x.SpectrumSearchResult.CidSpectrum.IsolationWindow.IsolationWindowTargetMz);
		    }
		    else 
            {
                lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflow(targetsToProcess, this.LcMsRun, hcdError, cidError, this.ScoreModel, progress);
                resultsGrouped = lipidGroupSearchResultList.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum != null ? (Double)x.SpectrumSearchResult.HcdSpectrum.ScanNum : (Double)x.SpectrumSearchResult.CidSpectrum.ScanNum);
            }

            // Group results of same scan together

			// Grab the result(s) with the best score
			foreach (var group in resultsGrouped)
			{
				var groupOrdered = group.OrderByDescending(x => x.Score).ToList();

				for (int i = 0; i < numResultsPerScanToInclude && i < groupOrdered.Count; i++)
				{
					LipidGroupSearchResult resultToAdd = groupOrdered[i];
					this.LipidGroupSearchResultList.Add(resultToAdd);
				}
			}
			OnPropertyChanged("LipidGroupSearchResultList");
			progress.Report(0);
            
		}

	    public void AddFragment(double mz, string ionType)
	    {
            MsMsSearchUnit newFragment = new MsMsSearchUnit(mz, ionType);
            FragmentSearchList.Add(newFragment);
            OnPropertyChanged("FragmentSearchList");
            
            //IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
            //progress.Report(0);
	    }

	    public void RemoveFragment(IList<MsMsSearchUnit> items)
	    {
	        foreach(var i in items)FragmentSearchList.Remove(i);
	    }

        public void SearchForFragments(double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude, int minMatches, Adduct adduct)
        {
            IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
            this.SpectrumSearchResultList = InformedWorkflow.RunFragmentWorkflow(FragmentSearchList, this.LcMsRun, hcdError, cidError, minMatches, progress);
            OnPropertyChanged("SpectrumSearchResultList");
            progress.Report(0);
	        if (this.SpectrumSearchResultList.Any())
	        {
	            SpectrumSearchResult spectrumSearchResult =
	                this.SpectrumSearchResultList.OrderByDescending(x => x.ApexScanNum).First();
                this.CurrentLipidTarget = LipidUtil.CreateLipidTarget((spectrumSearchResult.HcdSpectrum??spectrumSearchResult.CidSpectrum).IsolationWindow.IsolationWindowTargetMz, fragmentationMode, adduct);
	            //OnMsMsSearchResultChange(spectrumSearchResult);
                OnSpectrumSearchResultChange(spectrumSearchResult);
                
	        }
	        else
	        {
	            this.CurrentSpectrumSearchResult = null;
	        }
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

	    public void OnWriteTargetInfo(string fileLocation)
	    {
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
	        LipidGroupSearchResultWriter.OutputTargetInfo(LipidTargetList, fileLocation, RawFileName, progress);
            progress.Report(0);
	    }

	    public void OnWriteFragmentInfo(string fileLocation)
	    {
            IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
	        var resultsToExport = SpectrumSearchResultList.Where(x => x.ShouldExport).ToList();
            LipidGroupSearchResultWriter.OutputFragmentInfo(resultsToExport, TargetAdduct, FragmentSearchList, LcMsRun, fileLocation, RawFileName, progress);
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

        private void ReportFragmentSearchProgress(int value)
        {
            this.FragmentSearchProgress = value;
            OnPropertyChanged("FragmentSearchProgress");
        }

	    private void ReportExportProgress(int value)
	    {
	        this.ExportProgress = value;
            OnPropertyChanged("ExportProgress");
	    }

	}
}
