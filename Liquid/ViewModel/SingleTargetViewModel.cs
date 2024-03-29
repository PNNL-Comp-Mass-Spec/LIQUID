﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Scoring;
using LiquidBackend.Util;
using PRISM;

namespace Liquid.ViewModel
{
    public class SingleTargetViewModel : ViewModelBase
    {
        // Ignore Spelling: foreach, LcMs, Workflow

        public LcMsRun LcMsRun { get; private set; }

        // Unused
        // public string FeatureFilePath { get; private set; }

        /// <summary>
        /// Raw file path
        /// </summary>
        public string RawFilePath { get; private set; }

        public LipidTarget CurrentLipidTarget { get; private set; }
        public List<FragmentationMode> FragmentationModeList { get; }
        public List<SpectrumSearchResult> SpectrumSearchResultList { get; private set; }
        public ObservableCollection<MsMsSearchUnit> FragmentSearchList { get; }
        public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
        public List<Adduct> AdductList { get; }
        public List<string> IonTypeList { get; }
        public List<Lipid> LipidTargetList { get; }
        public List<Tuple<string, int, double?>> LipidIdentifications { get; }
        public List<LipidGroupSearchResult> LipidGroupSearchResultList { get; private set; }
        public ScoreModel ScoreModel { get; }
        public Adduct TargetAdduct { get; set; }
        public FragmentationMode TargetFragmentationMode { get; set; }

        public int LipidTargetLoadProgress { get; private set; }
        public int GlobalWorkflowProgress { get; private set; }
        public int FragmentSearchProgress { get; private set; }
        public string MsDataLoadProgress { get; private set; }
        public bool IsIms { get; private set; }
        public bool AverageSpec { get; set; }
        public bool IncludeMsMsPeaks { get; set; }

        public SingleTargetViewModel()
        {
            RawFilePath = "File loaded: none";
            FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
            //this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Dihydrogen, Adduct.Ammonium, Adduct.Acetate };
            AdductList = Enum.GetValues(typeof(Adduct)).Cast<Adduct>().ToList();
            IonTypeList = new List<string> { "Product Ion", "Neutral Loss" };
            SpectrumSearchResultList = new List<SpectrumSearchResult>();
            LipidTargetList = new List<Lipid>();
            FragmentSearchList = new ObservableCollection<MsMsSearchUnit>();
            LipidIdentifications = new List<Tuple<string, int, double?>>();
            ScoreModel = ScoreModelSerialization.Deserialize("DefaultScoringModel.xml");
            AverageSpec = false;
            IncludeMsMsPeaks = false;
            // load lipid rules
            LipidRules.LoadLipidRules("DefaultCompositionRules.txt", "DefaultFragmentationRules.txt");
        }

        /// <summary>
        /// Clear the progress box, optionally showing the program version,
        /// </summary>
        /// <param name="showProgramVersion"></param>
        public void ClearProgress(bool showProgramVersion = true)
        {
            if (showProgramVersion)
            {
                ShowProgramVersion();
                return;
            }

            MsDataLoadProgress = string.Empty;
            OnPropertyChanged("MsDataLoadProgress");
        }

        /// <summary>
        /// Clear the search results
        /// </summary>
        public void InitializeSearchResults()
        {
            if (LipidGroupSearchResultList == null)
            {
                LipidGroupSearchResultList = new List<LipidGroupSearchResult>();
            }
            else
            {
                LipidGroupSearchResultList.Clear();
            }
        }

        /// <summary>
        /// Show the program version in the MsDataLoadProgress TextBox
        /// </summary>
        private void ShowProgramVersion()
        {
            var programVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MsDataLoadProgress = string.Format("LIQUID v{0}.{1}.{2}", programVersion.Major, programVersion.Minor, programVersion.Build);
            OnPropertyChanged("MsDataLoadProgress");
        }

        public void UpdateRawFileLocation(string rawFileLocation)
        {
            var rawFileInfo = new FileInfo(rawFileLocation);
            IsIms = string.Equals(Path.GetExtension(rawFileLocation), ".uimf", StringComparison.OrdinalIgnoreCase);

            RawFilePath = "File loaded: none";
            OnPropertyChanged("RawFilePath");
            ClearProgress();

            if (IsIms)
            {
                //TODO: IMS Workflow
            }
            else
            {
                LcMsRun?.Close();

                RawFilePath = rawFileInfo.FullName;
                OnPropertyChanged("RawFilePath");

                MsDataLoadProgress = "Opening file";
                OnPropertyChanged("MsDataLoadProgress");

                var dataFactory = new LcMsDataFactory();
                dataFactory.ProgressChanged += LcMsDataFactory_ProgressChanged;

                LcMsRun = dataFactory.GetLcMsData(rawFileLocation);
                OnPropertyChanged("LcMsRun");
            }
        }

        public void SearchForTarget(string commonName, Adduct adduct, FragmentationMode fragmentationMode, double hcdMassError, double cidMassError)
        {
            CurrentLipidTarget = LipidUtil.CreateLipidTarget(commonName, fragmentationMode, adduct);
            OnPropertyChanged("CurrentLipidTarget");

            SpectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(CurrentLipidTarget, LcMsRun, hcdMassError, cidMassError, ScoreModel);
            OnPropertyChanged("SpectrumSearchResultList");

            if (SpectrumSearchResultList.Count > 0)
            {
                var spectrumSearchResult = SpectrumSearchResultList.OrderByDescending(x => x.NumMatchingMsMsPeaks).First();
                OnSpectrumSearchResultChange(spectrumSearchResult);
            }
            else
            {
                CurrentSpectrumSearchResult = null;
            }
        }

        public void OnUpdateTargetAdductFragmentation(Adduct adduct, FragmentationMode fragMode)
        {
            TargetAdduct = adduct;
            TargetFragmentationMode = fragMode;
            OnPropertyChanged("TargetAdduct");
            OnPropertyChanged("TargetFragmentationMode");
        }

        public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
        {
            CurrentSpectrumSearchResult = spectrumSearchResult;
            OnPropertyChanged("CurrentSpectrumSearchResult");
            OnPropertyChanged("CurrentLipidTarget");
        }

        public void OnMsMsSearchResultChange(SpectrumSearchResult spectrumSearchResult)
        {
            CurrentSpectrumSearchResult = spectrumSearchResult;
            CurrentLipidTarget = LipidUtil.CreateLipidTarget((spectrumSearchResult.HcdSpectrum ?? spectrumSearchResult.CidSpectrum).IsolationWindow.IsolationWindowTargetMz, TargetFragmentationMode, TargetAdduct);

            OnPropertyChanged("CurrentSpectrumSearchResult");
            OnPropertyChanged("CurrentLipidTarget");
        }

        public void LoadMoreLipidTargets(string fileLocation)
        {
            IProgress<int> progress = new Progress<int>(ReportLipidTargetLoadProgress);

            var fileInfo = new FileInfo(fileLocation);

            var lipidReader = new LipidMapsDbReader<Lipid>();

            LipidTargetList.AddRange(lipidReader.ReadFile(fileInfo, progress));

            OnPropertyChanged("LipidTargetList");

            //// Reset and populate the list of lipid classes
            //this.LipidClassList = new List<LipidClass>();
            //var groupByLipidClass = this.LipidTargetList.GroupBy(x => x.LipidTarget.LipidClass);
            //foreach (LipidClass lipidClass in groupByLipidClass.Select(@group => @group.Key))
            //{
            //  this.LipidClassList.Add(lipidClass);
            //}

            //if (this.LipidClassList.Count > 0)
            //{
            //  LipidClass lipidClass = this.LipidClassList[0];
            //  this.CurrentLipidClass = lipidClass;
            //  this.CurrentLipidTargetList = this.LipidTargetList.Where(x => x.LipidClass == lipidClass).ToList();

            //  OnPropertyChanged("CurrentLipidClass");
            //}

            //OnPropertyChanged("LipidClassList");
            //OnPropertyChanged("CurrentLipidTargetList");

            // Reset the progress bar back to 0
            progress.Report(0);
        }

        public void LoadLipidIdentifications(string fileLocation)
        {
            var fileInfo = new FileInfo(fileLocation);

            var identificationReader = new OutputFileReader<Tuple<string, int, double?>>();
            var idList = identificationReader.ReadFile(fileInfo);

            foreach (var id in idList)
            {
                if (!LipidIdentifications.Contains(id))
                {
                    LipidIdentifications.Add(id);
                }
            }
            SelectLipidIdentifications(LipidGroupSearchResultList);
            OnPropertyChanged("LipidIdentifications");
        }

        public void OnBuildLibrary(IList<string> filesList, double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
        {
            foreach (var file in filesList)
            {
                using var reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                if (reader.EndOfStream)
                    continue;

                var headerLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    // Empty header line
                    continue;
                }
                var header = headerLine.Split('\t').ToList();

                var index = header.IndexOf("Raw Data File");
                if (index == -1)
                    continue;

                if (reader.EndOfStream)
                    continue;

                try
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(dataLine))
                    {
                        // Empty data line
                        continue;
                    }

                    var rawFileName = dataLine.Split('\t')[index];
                    LibraryBuilder.AddDmsDataset(rawFileName);
                    UpdateRawFileLocation(rawFileName);
                    OnProcessAllTarget(hcdError, cidError, fragmentationMode, numResultsPerScanToInclude);
                    LoadLipidIdentifications(file);
                    OnExportGlobalResults(file.Replace(".tsv", ".msp"));

                    // Delete the raw files we copied from DMS to save space
                    LcMsRun.Close();
                    LcMsRun = null;
                    OnPropertyChanged("LcMsRun");
                    GC.Collect();

                    // File.Delete(rawFileName);
                    var pbfFilePath = Path.ChangeExtension(rawFileName, ".pbf");
                    if (string.IsNullOrWhiteSpace(pbfFilePath))
                        continue;

                    var pbfFile = new FileInfo(pbfFilePath);
                    if (pbfFile.Exists)
                        pbfFile.Delete();
                }
                catch (Exception ex)
                {
                    // Ignore the error
                    Console.WriteLine("Exception in OnBuildLibrary: " + ex.Message);
                }
            }
        }

        public void OnProcessAllTarget(double hcdError, double cidError, FragmentationMode fragmentationMode, int numResultsPerScanToInclude)
        {
            try
            {
                IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);

                // Make sure to only look at targets that match the fragmentation mode
                var targetsToProcess = LipidTargetList.Where(x => x.LipidTarget.FragmentationMode == fragmentationMode);

                // Run global analysis
                InitializeSearchResults();

                IEnumerable<IGrouping<double, LipidGroupSearchResult>> resultsGrouped;
                List<LipidGroupSearchResult> lipidGroupSearchResultList;
                if (AverageSpec)
                {
                    lipidGroupSearchResultList = GlobalWorkflow.RunGlobalWorkflowAvgSpec(targetsToProcess, LcMsRun, hcdError, cidError, ScoreModel);
                    resultsGrouped = lipidGroupSearchResultList.GroupBy(x =>
                        x.SpectrumSearchResult.HcdSpectrum?.IsolationWindow.IsolationWindowTargetMz ??
                        x.SpectrumSearchResult.CidSpectrum.IsolationWindow.IsolationWindowTargetMz);
                }
                else
                {
                    lipidGroupSearchResultList =
                        GlobalWorkflow.RunGlobalWorkflow(targetsToProcess, LcMsRun, hcdError, cidError, ScoreModel, progress);

                    resultsGrouped = lipidGroupSearchResultList.GroupBy(x =>
                        x.SpectrumSearchResult.HcdSpectrum?.ScanNum ?? (double)x.SpectrumSearchResult.CidSpectrum.ScanNum);
                }

                // Group results of same scan together

                // Grab the result(s) with the best score
                foreach (var group in resultsGrouped)
                {
                    var groupOrdered = group.OrderByDescending(x => x.Score).ToList();

                    for (var i = 0; i < numResultsPerScanToInclude && i < groupOrdered.Count; i++)
                    {
                        var resultToAdd = groupOrdered[i];
                        LipidGroupSearchResultList.Add(resultToAdd);
                    }
                }

                OnPropertyChanged("LipidGroupSearchResultList");
                progress.Report(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception caught: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void AddFragment(double mz, string ionType)
        {
            var newFragment = new MsMsSearchUnit(mz, ionType);
            FragmentSearchList.Add(newFragment);
            OnPropertyChanged("FragmentSearchList");

            //IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
            //progress.Report(0);
        }

        public void RemoveFragment(IList<MsMsSearchUnit> items)
        {
            foreach (var i in items)
            {
                FragmentSearchList.Remove(i);
            }
        }

        public void SearchForFragments(double hcdError, double cidError, FragmentationMode fragmentationMode, int minMatches, Adduct adduct)
        {
            IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
            SpectrumSearchResultList = InformedWorkflow.RunFragmentWorkflow(FragmentSearchList, LcMsRun, hcdError, cidError, minMatches, progress);
            OnPropertyChanged("SpectrumSearchResultList");
            progress.Report(0);
            if (SpectrumSearchResultList.Count > 0)
            {
                var spectrumSearchResult =
                    SpectrumSearchResultList.OrderByDescending(x => x.ApexScanNum).First();
                CurrentLipidTarget = LipidUtil.CreateLipidTarget((spectrumSearchResult.HcdSpectrum ?? spectrumSearchResult.CidSpectrum).IsolationWindow.IsolationWindowTargetMz, fragmentationMode, adduct);
                //OnMsMsSearchResultChange(spectrumSearchResult);
                OnSpectrumSearchResultChange(spectrumSearchResult);
            }
            else
            {
                CurrentSpectrumSearchResult = null;
            }
        }

        public void SelectLipidIdentifications(List<LipidGroupSearchResult> lipidGroupSearchResultList)
        {
            foreach (var id in LipidIdentifications)
            {
                foreach (var lipid in lipidGroupSearchResultList)
                {
                    var name = id.Item1;
                    var scan = id.Item2;
                    var percentage = id.Item3;
                    if (lipid.LipidTarget.StrippedDisplay == name && lipid.SpectrumSearchResult.HcdSpectrum.ScanNum == scan)
                    {
                        lipid.ShouldExport = true;
                        lipid.DisplayPercentage = percentage;
                        break;
                    }
                }
            }
        }

        public void OnExportGlobalResults(string fileLocation)
        {
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
            var resultsToExport = LipidGroupSearchResultList.Where(x => x.ShouldExport).ToList();
            LipidGroupSearchResultWriter.OutputResults(resultsToExport, fileLocation, Path.GetFileName(RawFilePath), progress, false, true, IncludeMsMsPeaks);
            progress.Report(0);
        }

        public void OnExportAllGlobalResults(string fileLocation)
        {
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
            LipidGroupSearchResultWriter.OutputResults(LipidGroupSearchResultList, fileLocation, Path.GetFileName(RawFilePath), progress, false, true, IncludeMsMsPeaks);
            progress.Report(0);
        }

        public void OnWriteTargetInfo(string fileLocation)
        {
            IProgress<int> progress = new Progress<int>(ReportGlobalWorkflowProgress);
            LipidGroupSearchResultWriter.OutputTargetInfo(LipidTargetList, fileLocation, progress);
            progress.Report(0);
        }

        public void OnWriteFragmentInfo(string fileLocation)
        {
            IProgress<int> progress = new Progress<int>(ReportFragmentSearchProgress);
            var resultsToExport = SpectrumSearchResultList.Where(x => x.ShouldExport).ToList();
            LipidGroupSearchResultWriter.OutputFragmentInfo(resultsToExport, TargetAdduct, FragmentSearchList, LcMsRun, fileLocation, Path.GetFileName(RawFilePath));
            progress.Report(0);
        }

        private void ReportLipidTargetLoadProgress(int value)
        {
            LipidTargetLoadProgress = value;
            OnPropertyChanged("LipidTargetLoadProgress");
        }

        private void ReportGlobalWorkflowProgress(int value)
        {
            GlobalWorkflowProgress = value;
            OnPropertyChanged("GlobalWorkflowProgress");
        }

        private void ReportFragmentSearchProgress(int value)
        {
            FragmentSearchProgress = value;
            OnPropertyChanged("FragmentSearchProgress");
        }

        // private void ReportExportProgress(int value)
        // {
        //     ExportProgress = value;
        //     OnPropertyChanged("ExportProgress");
        // }

        private void ReportMsDataLoadProgress(double value)
        {
            if (value > 99.9)
                MsDataLoadProgress = string.Empty;
            else
                MsDataLoadProgress = string.Format("Caching data ... {0:F1}%", value);

            OnPropertyChanged("MsDataLoadProgress");
        }

        #region "Events"

        private void LcMsDataFactory_ProgressChanged(object sender, ProgressData e)
        {
            ReportMsDataLoadProgress(e.Percent);
        }

        #endregion

        public override void Dispose()
        {
            // Nothing to do
        }
    }
}
