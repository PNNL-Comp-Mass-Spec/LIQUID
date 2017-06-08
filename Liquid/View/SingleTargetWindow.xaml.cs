using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Liquid.ViewModel;
using LiquidBackend.Domain;
using Ookii.Dialogs.Wpf;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Liquid.View
{
    /// <summary>
    /// Interaction logic for SingleTargetWindow.xaml
    /// </summary>
    public partial class SingleTargetWindow
    {
        public SingleTargetViewModel SingleTargetViewModel { get; }

        public SingleTargetWindow()
        {
            InitializeComponent();

            SingleTargetViewModel = new SingleTargetViewModel();
            DataContext = SingleTargetViewModel;

            FragmentationModeComboBox.SelectedValue = FragmentationMode.Positive;
            AdductComboBox.SelectedValue = Adduct.Hydrogen;
            AdductComboBox2.SelectedValue = Adduct.Hydrogen;
            IonTypeComboBox.SelectedIndex = 0; //"Primary Ion"
            TargetMzTextBlock.Visibility = Visibility.Collapsed;
            EmpiricalFormulaTextBlock.Visibility = Visibility.Collapsed;
            EmpiricalFormulaRichTextBlock.Visibility = Visibility.Collapsed;
            NumberOfResultsTextBlock.Visibility = Visibility.Collapsed;
            SpectrumSearchResultsDataGrid.Visibility = Visibility.Collapsed;
            MsMsInfoUserControl.Visibility = Visibility.Hidden;
            MsOneInfoUserControl.Visibility = Visibility.Hidden;
            LipidGroupSearchResultsDataGrid.Visibility = Visibility.Hidden;
            ExportGlobalResultsButton.Visibility = Visibility.Hidden;
            ExportAllGlobalResultsButton.Visibility = Visibility.Hidden;
            ExportFragmentResultsButton.Visibility = Visibility.Hidden;
            FragmentSearchResultsDataGrid.Visibility = Visibility.Hidden;
        }

        private async void RawFileButtonClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog and Set filter for file extension and default file extension
            var dialog = new VistaOpenFileDialog { DefaultExt = ".raw", Filter = "Thermo(*.raw)|*.raw|mzML(*.mzML, *.mzML.gz)|*.mzml;*.mzML;*.mzML.gz;*.mzml.gz" };

            // Get the selected file name and display in a TextBox
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {

                // Disable buttons while files is loading
                ProcessAllTargetsButton.IsEnabled = false;
                SearchForTargetButton.IsEnabled = false;

                // Open file
                var fileName = dialog.FileName;
                var fileInfo = new FileInfo(fileName);

                await Task.Run(() => SingleTargetViewModel.UpdateRawFileLocation(fileInfo.FullName));

                //Make sure we loaded a file
                if (SingleTargetViewModel.LcMsRun != null)
                {
                    // Enable processing all targets button if applicable
                    if (SingleTargetViewModel.LipidTargetList != null && SingleTargetViewModel.LipidTargetList.Any())
                        ProcessAllTargetsButton.IsEnabled = true;

                    // Enable search for target button
                    SearchForTargetButton.IsEnabled = true;
                }

                // Delay before clearing the progress to give the data loading thread a chance to report the final progress value
                System.Threading.Thread.Sleep(250);
                SingleTargetViewModel.ClearProgress();

            }
        }

        private void SearchForTargetButtonClick(object sender, RoutedEventArgs e)
        {
            var commonName = CommonNameTextBox.Text;
            var adduct = (Adduct) AdductComboBox.SelectedItem;
            var fragmentationMode = (FragmentationMode) FragmentationModeComboBox.SelectedItem;
            var hcdMassError = double.Parse(HcdErrorTextBox.Text);
            var cidMassError = double.Parse(CidErrorTextBox.Text);

            SingleTargetViewModel.SearchForTarget(commonName, adduct, fragmentationMode, hcdMassError, cidMassError);

            // Update user controls with new lipid target
            MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);
            MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);

            // Select the best spectrum search result
            /*
            if (this.SingleTargetViewModel.CurrentSpectrumSearchResult != null)
            {
                var dataGrid = this.SpectrumSearchResultsDataGrid;

                dataGrid.SelectedItem = this.SingleTargetViewModel.CurrentSpectrumSearchResult;
                dataGrid.ScrollIntoView(this.SingleTargetViewModel.CurrentSpectrumSearchResult);
            }*/

            UpdateEmpiricalFormula(SingleTargetViewModel.CurrentLipidTarget.EmpiricalFormula);

            TargetMzTextBlock.Visibility = Visibility.Visible;
            EmpiricalFormulaTextBlock.Visibility = Visibility.Visible;
            EmpiricalFormulaRichTextBlock.Visibility = Visibility.Visible;
            NumberOfResultsTextBlock.Visibility = Visibility.Visible;
            SpectrumSearchResultsDataGrid.Visibility = Visibility.Visible;
        }

        private void SpectrumSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var selectedItem = dataGrid?.SelectedItem;

            if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(SpectrumSearchResult)))
            {
                var spectrumSearchResult = (SpectrumSearchResult)selectedItem;
                //this.SingleTargetViewModel.OnMsMsSearchResultChange(spectrumSearchResult);
                MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);

                MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);
                MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                MsMsInfoUserControl.Visibility = Visibility.Visible;

                MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);

                MsOneInfoUserControl.Visibility = Visibility.Visible;
            }
        }


        private void UpdateEmpiricalFormula(string empiricalFormula)
        {
            var paragraph = new Paragraph();
            var ffc = new FontFamilyConverter();
            paragraph.FontFamily = (FontFamily)ffc.ConvertFromString("Palatino Linotype");

            foreach (var empiricalCharacter in empiricalFormula)
            {
                var run = new Run(empiricalCharacter.ToString());

                // Subscript any numbers
                if (Char.IsNumber(empiricalCharacter)) run.Typography.Variants = FontVariants.Subscript;

                paragraph.Inlines.Add(run);
            }

            var flowDocument = new FlowDocument();
            flowDocument.Blocks.Add(paragraph);

            EmpiricalFormulaRichTextBlock.Document = flowDocument;
        }

        private async void LoadTargetsFileButtonClick(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog and Set filter for file extension and default file extension
            var dialog = new VistaOpenFileDialog { DefaultExt = ".txt", Filter = "Text Files (*.txt)|*.txt|Tab Separated Files (.tsv)|*.tsv|All Files (*.*)|*.*" };

            // Get the selected file name and display in a TextBox
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                // Disable processing button while file is loading
                ProcessAllTargetsButton.IsEnabled = false;

                // Open file
                var fileName = dialog.FileName;

                await Task.Run(() => SingleTargetViewModel.LoadMoreLipidTargets(fileName));

                // Enable processing all targets button if applicable
                if (SingleTargetViewModel.LcMsRun != null) ProcessAllTargetsButton.IsEnabled = true;
            }
        }

        private async void LoadIdentificationsFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (SingleTargetViewModel.LipidGroupSearchResultList == null)
            {
                MessageBox.Show("Please process a file prior to loading lipid identifications.");
            }
            else
            {
                // Create OpenFileDialog and Set filter for file extension and default file extension
                var dialog = new VistaOpenFileDialog
                {
                    DefaultExt = ".tsv",
                    Filter = "Tab Separated Files (.tsv)|*.tsv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
                };

                // Get the selected file name and display in a TextBox
                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    // Disable processing button while file is loading
                    ProcessAllTargetsButton.IsEnabled = false;

                    // Open file
                    var fileName = dialog.FileName;

                    await Task.Run(() => SingleTargetViewModel.LoadLipidIdentifications(fileName));

                    // Enable processing all targets button if applicable
                    if (SingleTargetViewModel.LcMsRun != null) ProcessAllTargetsButton.IsEnabled = true;
                }
            }
        }

        private async void BuildLibraryButtonClick(object sender, RoutedEventArgs e)
        {
            var fragmentationMode = (FragmentationMode)FragmentationModeComboBox.SelectedItem;
            var hcdMassError = double.Parse(HcdErrorTextBox.Text);
            var cidMassError = double.Parse(CidErrorTextBox.Text);
            var resultsPerScan = int.Parse(ResultsPerScanTextBox.Text);

            var dialog = new VistaOpenFileDialog
            {
                DefaultExt = ".tsv",
                Filter = "Tab Separated Files (.tsv)|*.tsv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = true
            };

            // Get the selected file name and display in a TextBox
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                // Disable processing button while file is loading
                ProcessAllTargetsButton.IsEnabled = false;

                // Open file
                var fileNames = dialog.FileNames;
                await Task.Run(() => SingleTargetViewModel.OnBuildLibrary(fileNames, hcdMassError, cidMassError, fragmentationMode, resultsPerScan));
            }
        }

        private async void ProcessAllTargetsButtonClick(object sender, RoutedEventArgs e)
        {
            var fragmentationMode = (FragmentationMode)FragmentationModeComboBox.SelectedItem;
            var hcdMassError = double.Parse(HcdErrorTextBox.Text);
            var cidMassError = double.Parse(CidErrorTextBox.Text);
            var resultsPerScan = int.Parse(ResultsPerScanTextBox.Text);

            LipidGroupSearchResultsDataGrid.Visibility = Visibility.Hidden;
            ExportGlobalResultsButton.Visibility = Visibility.Hidden;
            ExportAllGlobalResultsButton.Visibility = Visibility.Hidden;
            await Task.Run(() => SingleTargetViewModel.OnProcessAllTarget(hcdMassError, cidMassError, fragmentationMode, resultsPerScan));
            LipidGroupSearchResultsDataGrid.Visibility = Visibility.Visible;
            ExportGlobalResultsButton.Visibility = Visibility.Visible;
            ExportAllGlobalResultsButton.Visibility = Visibility.Visible;

            // Select the best spectrum search result
            if (SingleTargetViewModel.LipidGroupSearchResultList.Count > 0)
            {
                var dataGrid = LipidGroupSearchResultsDataGrid;

                dataGrid.SelectedItem = SingleTargetViewModel.LipidGroupSearchResultList[0];
                dataGrid.ScrollIntoView(SingleTargetViewModel.LipidGroupSearchResultList[0]);
            }
        }

        private void LipidGroupSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var selectedItem = dataGrid?.SelectedItem;

            if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(LipidGroupSearchResult)))
            {
                var lipidGroupSearchResult = (LipidGroupSearchResult)selectedItem;
                var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;

                SingleTargetViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);

                MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(lipidGroupSearchResult.LipidTarget);
                MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                MsMsInfoUserControl.Visibility = Visibility.Visible;

                MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(lipidGroupSearchResult.LipidTarget);
                MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                MsOneInfoUserControl.Visibility = Visibility.Visible;
            }
        }

        private void MsMsSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var selectedItem = dataGrid?.SelectedItem;

            if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(SpectrumSearchResult)))
            {
                var spectrumSearchResult = (SpectrumSearchResult)selectedItem;
                SingleTargetViewModel.OnMsMsSearchResultChange(spectrumSearchResult);
                MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);

                MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);
                MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                MsMsInfoUserControl.Visibility = Visibility.Visible;

                MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);

                MsOneInfoUserControl.Visibility = Visibility.Visible;
            }
        }

        private async void ExportGlobalResultsButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog()
            {
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = ".tsv",
                Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp"
            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fileLocation = dialog.FileName;
                await Task.Run(() => SingleTargetViewModel.OnExportGlobalResults(fileLocation));
            }
        }

        private void ExportAllGlobalResultsButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog()
            {
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = ".tsv",
                Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp"
            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fileLocation = dialog.FileName;
                SingleTargetViewModel.OnExportAllGlobalResults(fileLocation);
            }
        }

        private void ExportTargetInfoButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog()
            {
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = ".tsv",
                Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp"
            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fileLocation = dialog.FileName;
                SingleTargetViewModel.OnWriteTargetInfo(fileLocation);
            }
        }

        private void AddFragmentButton_OnClick(object sender, RoutedEventArgs e)
        {
            double fragmentMz;
            var validFragment = double.TryParse(FragmentMassTextBox.Text, out fragmentMz);
            var ionType = (string)IonTypeComboBox.SelectedItem;
            if (validFragment)
            {
                SingleTargetViewModel.AddFragment(fragmentMz, ionType);
                FragmentSearchListDataGrid.ItemsSource = SingleTargetViewModel.FragmentSearchList;
                SearchForFragmentsButton.IsEnabled = true;
            }
            else
            {
               MessageBox.Show("Invalid m/z. Please only use numbers.","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
            }

        }

        private async void SearchForFragmentsButtonClick(object sender, RoutedEventArgs e)
        {
            var fragmentationMode = (FragmentationMode) FragmentationModeComboBox.SelectedItem;
            var hcdMassError = double.Parse(HcdErrorTextBox.Text);
            var cidMassError = double.Parse(CidErrorTextBox.Text);
            var resultsPerScan = int.Parse(ResultsPerScanTextBox.Text);
            var minMatches = int.Parse(MinimumMatchesTextBox.Text);
            var adduct = (Adduct) AdductComboBox2.SelectedItem;

            SingleTargetViewModel.OnUpdateTargetAdductFragmentation(adduct, fragmentationMode);
            await Task.Run(() =>SingleTargetViewModel.SearchForFragments(hcdMassError, cidMassError,fragmentationMode,resultsPerScan, minMatches, adduct));

            MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);
            MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(SingleTargetViewModel.CurrentLipidTarget);

            TargetMzTextBlock.Visibility = Visibility.Visible;
            EmpiricalFormulaTextBlock.Visibility = Visibility.Visible;
            EmpiricalFormulaRichTextBlock.Visibility = Visibility.Visible;
            NumberOfResultsTextBlock.Visibility = Visibility.Visible;
            FragmentSearchResultsDataGrid.Visibility = Visibility.Visible;
            ExportFragmentResultsButton.Visibility = Visibility.Visible;

            if (SingleTargetViewModel.CurrentSpectrumSearchResult != null)
            {
                var dataGrid = FragmentSearchResultsDataGrid;

                dataGrid.SelectedItem = SingleTargetViewModel.CurrentSpectrumSearchResult;
                dataGrid.ScrollIntoView(SingleTargetViewModel.CurrentSpectrumSearchResult);
            }
        }

        private void RemoveFragmentButton_OnClick(object sender, RoutedEventArgs e)
        {

            var items = FragmentSearchListDataGrid.SelectedItems.OfType<MsMsSearchUnit>().ToList();


            if(FragmentSearchListDataGrid.SelectedItem != null) SingleTargetViewModel.RemoveFragment(items);
            if (SingleTargetViewModel.FragmentSearchList.Count == 0)
            {
                SearchForFragmentsButton.IsEnabled = false;
                RemoveFragmentButton.IsEnabled = false;
            }
        }

        private void FragmentSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            RemoveFragmentButton.IsEnabled = FragmentSearchListDataGrid.SelectedItem != null;
        }

        private void ExportFragmentResultsButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog()
            {
                AddExtension = true,
                OverwritePrompt = true,
                DefaultExt = ".tsv",
                Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp"
            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fileLocation = dialog.FileName;
                SingleTargetViewModel.OnWriteFragmentInfo(fileLocation);
            }
        }
    }
}
