using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Liquid.ViewModel;
using LiquidBackend.Domain;
using Ookii.Dialogs;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace Liquid.View
{
	/// <summary>
	/// Interaction logic for SingleTargetWindow.xaml
	/// </summary>
	public partial class SingleTargetWindow
	{
		public SingleTargetViewModel SingleTargetViewModel { get; private set; }

		public SingleTargetWindow()
		{
			InitializeComponent();

			this.SingleTargetViewModel = new SingleTargetViewModel();
			this.DataContext = this.SingleTargetViewModel;

			this.FragmentationModeComboBox.SelectedValue = FragmentationMode.Positive;
			this.AdductComboBox.SelectedValue = Adduct.Hydrogen;
            this.AdductComboBox2.SelectedValue = Adduct.Hydrogen;
		    this.IonTypeComboBox.SelectedIndex = 0; //"Product Ion"
			this.TargetMzTextBlock.Visibility = Visibility.Collapsed;
			this.EmpiricalFormulaTextBlock.Visibility = Visibility.Collapsed;
			this.EmpiricalFormulaRichTextBlock.Visibility = Visibility.Collapsed;
			this.NumberOfResultsTextBlock.Visibility = Visibility.Collapsed;
			this.SpectrumSearchResultsDataGrid.Visibility = Visibility.Collapsed;
			this.MsMsInfoUserControl.Visibility = Visibility.Hidden;
			this.MsOneInfoUserControl.Visibility = Visibility.Hidden;
			this.LipidGroupSearchResultsDataGrid.Visibility = Visibility.Hidden;
			this.ExportGlobalResultsButton.Visibility = Visibility.Hidden;
			this.ExportAllGlobalResultsButton.Visibility = Visibility.Hidden;
		    this.ExportFragmentResultsButton.Visibility = Visibility.Hidden;
		    this.FragmentSearchResultsDataGrid.Visibility = Visibility.Hidden;
		}

		private async void RawFileButtonClick(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog and Set filter for file extension and default file extension
			var dialog = new VistaOpenFileDialog { DefaultExt = ".raw", Filter = "Thermo(*.raw)|*.raw|mzML(*.mzML, *.mzML.gz)|*.mzml;*.mzML;*.mzML.gz;*.mzml.gz" };

			// Get the selected file name and display in a TextBox 
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				this.RawFileLocationTextBlock.Text = "Loading file...";

				// Disable buttons while files is loading
				this.ProcessAllTargetsButton.IsEnabled = false;
				this.SearchForTargetButton.IsEnabled = false;

				// Open file 
				string fileName = dialog.FileName;
				FileInfo fileInfo = new FileInfo(fileName);

			    
				await Task.Run(() => this.SingleTargetViewModel.UpdateRawFileLocation(fileInfo.FullName));
                //Make sure we loaded a file
                if (this.SingleTargetViewModel.LcMsRun != null)  
			    {
			        this.RawFileLocationTextBlock.Text = "File Loaded: " + fileInfo.Name;

			        // Enable processing all targets button if applicable
			        if (this.SingleTargetViewModel.LipidTargetList != null && this.SingleTargetViewModel.LipidTargetList.Any())
			            this.ProcessAllTargetsButton.IsEnabled = true;

			        // Enable search for target button
			        this.SearchForTargetButton.IsEnabled = true;
			    }
			    else { this.RawFileLocationTextBlock.Text = "File Loaded: None Loaded"; }
			}
		}

		private void SearchForTargetButtonClick(object sender, RoutedEventArgs e)
		{
			string commonName = this.CommonNameTextBox.Text;
			Adduct adduct = (Adduct) this.AdductComboBox.SelectedItem;
			FragmentationMode fragmentationMode = (FragmentationMode) this.FragmentationModeComboBox.SelectedItem;
			double hcdMassError = double.Parse(this.HcdErrorTextBox.Text);
			double cidMassError = double.Parse(this.CidErrorTextBox.Text);

			this.SingleTargetViewModel.SearchForTarget(commonName, adduct, fragmentationMode, hcdMassError, cidMassError);

			// Update user controls with new lipid target
			this.MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);
			this.MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);

			// Select the best spectrum search result
            /*
			if (this.SingleTargetViewModel.CurrentSpectrumSearchResult != null)
			{
				var dataGrid = this.SpectrumSearchResultsDataGrid;

				dataGrid.SelectedItem = this.SingleTargetViewModel.CurrentSpectrumSearchResult;
				dataGrid.ScrollIntoView(this.SingleTargetViewModel.CurrentSpectrumSearchResult);
			}*/

			UpdateEmpiricalFormula(this.SingleTargetViewModel.CurrentLipidTarget.EmpiricalFormula);

			this.TargetMzTextBlock.Visibility = Visibility.Visible;
			this.EmpiricalFormulaTextBlock.Visibility = Visibility.Visible;
			this.EmpiricalFormulaRichTextBlock.Visibility = Visibility.Visible;
			this.NumberOfResultsTextBlock.Visibility = Visibility.Visible;
			this.SpectrumSearchResultsDataGrid.Visibility = Visibility.Visible;
		}

		private void SpectrumSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			var dataGrid = sender as DataGrid;
			if (dataGrid != null)
			{
				var selectedItem = dataGrid.SelectedItem;

				if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(SpectrumSearchResult)))
				{
					SpectrumSearchResult spectrumSearchResult = (SpectrumSearchResult)selectedItem;
                    //this.SingleTargetViewModel.OnMsMsSearchResultChange(spectrumSearchResult);
				    this.MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);

                    this.MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);
					this.MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
					this.MsMsInfoUserControl.Visibility = Visibility.Visible;

					this.MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                    
					this.MsOneInfoUserControl.Visibility = Visibility.Visible;
				}
			}
		}


		private void UpdateEmpiricalFormula(string empiricalFormula)
		{
			Paragraph paragraph = new Paragraph();
			FontFamilyConverter ffc = new FontFamilyConverter();
			paragraph.FontFamily = (FontFamily)ffc.ConvertFromString("Palatino Linotype");

			foreach (var empiricalCharacter in empiricalFormula)
			{
				Run run = new Run(empiricalCharacter.ToString());

				// Subscript any numbers
				if (Char.IsNumber(empiricalCharacter)) run.Typography.Variants = FontVariants.Subscript;

				paragraph.Inlines.Add(run);
			}

			FlowDocument flowDocument = new FlowDocument();
			flowDocument.Blocks.Add(paragraph);

			this.EmpiricalFormulaRichTextBlock.Document = flowDocument;
		}

		private async void LoadTargetsFileButtonClick(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog and Set filter for file extension and default file extension
			var dialog = new VistaOpenFileDialog { DefaultExt = ".txt", Filter = "Text Files (*.txt)|*.txt|Tab Separated Files (.tsv)|*.tsv|All Files (*.*)|*.*" };

			// Get the selected file name and display in a TextBox 
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				// Disable processing button while file is loading
				this.ProcessAllTargetsButton.IsEnabled = false;

				// Open file 
				string fileName = dialog.FileName;

				await Task.Run(() => this.SingleTargetViewModel.LoadMoreLipidTargets(fileName));

				// Enable processing all targets button if applicable
				if (this.SingleTargetViewModel.LcMsRun != null) this.ProcessAllTargetsButton.IsEnabled = true;
			}
		}

        private async void LoadIdentificationsFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.SingleTargetViewModel.LipidGroupSearchResultList == null)
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
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Disable processing button while file is loading
                    this.ProcessAllTargetsButton.IsEnabled = false;

                    // Open file 
                    string fileName = dialog.FileName;

                    await Task.Run(() => this.SingleTargetViewModel.LoadLipidIdentifications(fileName));

                    // Enable processing all targets button if applicable
                    if (this.SingleTargetViewModel.LcMsRun != null) this.ProcessAllTargetsButton.IsEnabled = true;
                }
            }
        }

	    private async void BuildLibraryButtonClick(object sender, RoutedEventArgs e)
	    {
            FragmentationMode fragmentationMode = (FragmentationMode)this.FragmentationModeComboBox.SelectedItem;
            double hcdMassError = double.Parse(this.HcdErrorTextBox.Text);
            double cidMassError = double.Parse(this.CidErrorTextBox.Text);
            int resultsPerScan = int.Parse(this.ResultsPerScanTextBox.Text);

            var dialog = new VistaOpenFileDialog
            {
                DefaultExt = ".tsv",
                Filter = "Tab Separated Files (.tsv)|*.tsv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = true
            };

            // Get the selected file name and display in a TextBox 
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Disable processing button while file is loading
                this.ProcessAllTargetsButton.IsEnabled = false;

                // Open file 
                var fileNames = dialog.FileNames;
                await Task.Run(() => this.SingleTargetViewModel.OnBuildLibrary(fileNames, hcdMassError, cidMassError, fragmentationMode, resultsPerScan));
            }
	    }

		private async void ProcessAllTargetsButtonClick(object sender, RoutedEventArgs e)
		{
			FragmentationMode fragmentationMode = (FragmentationMode)this.FragmentationModeComboBox.SelectedItem;
			double hcdMassError = double.Parse(this.HcdErrorTextBox.Text);
			double cidMassError = double.Parse(this.CidErrorTextBox.Text);
			int resultsPerScan = int.Parse(this.ResultsPerScanTextBox.Text);

			this.LipidGroupSearchResultsDataGrid.Visibility = Visibility.Hidden;
			this.ExportGlobalResultsButton.Visibility = Visibility.Hidden;
			this.ExportAllGlobalResultsButton.Visibility = Visibility.Hidden;
			await Task.Run(() => this.SingleTargetViewModel.OnProcessAllTarget(hcdMassError, cidMassError, fragmentationMode, resultsPerScan));
			this.LipidGroupSearchResultsDataGrid.Visibility = Visibility.Visible;
			this.ExportGlobalResultsButton.Visibility = Visibility.Visible;
			this.ExportAllGlobalResultsButton.Visibility = Visibility.Visible;

			// Select the best spectrum search result
			if (this.SingleTargetViewModel.LipidGroupSearchResultList.Count > 0)
			{
				var dataGrid = this.LipidGroupSearchResultsDataGrid;

				dataGrid.SelectedItem = this.SingleTargetViewModel.LipidGroupSearchResultList[0];
				dataGrid.ScrollIntoView(this.SingleTargetViewModel.LipidGroupSearchResultList[0]);
			}
		}

		private void LipidGroupSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
		{
			var dataGrid = sender as DataGrid;
			if (dataGrid != null)
			{
				var selectedItem = dataGrid.SelectedItem;

				if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(LipidGroupSearchResult)))
				{
					LipidGroupSearchResult lipidGroupSearchResult = (LipidGroupSearchResult)selectedItem;
					SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;

					this.SingleTargetViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);

					this.MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(lipidGroupSearchResult.LipidTarget);
					this.MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
					this.MsMsInfoUserControl.Visibility = Visibility.Visible;

					this.MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(lipidGroupSearchResult.LipidTarget);
					this.MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
					this.MsOneInfoUserControl.Visibility = Visibility.Visible;
				}
			}
		}

        private void MsMsSearchResultSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                var selectedItem = dataGrid.SelectedItem;

                if (selectedItem != null && ReferenceEquals(selectedItem.GetType(), typeof(SpectrumSearchResult)))
                {
                    SpectrumSearchResult spectrumSearchResult = (SpectrumSearchResult)selectedItem;
                    this.SingleTargetViewModel.OnMsMsSearchResultChange(spectrumSearchResult);
                    this.MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);

                    this.MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);
                    this.MsMsInfoUserControl.MsMsInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
                    this.MsMsInfoUserControl.Visibility = Visibility.Visible;

                    this.MsOneInfoUserControl.MsOneInfoViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);

                    this.MsOneInfoUserControl.Visibility = Visibility.Visible;
                }
            }
        }

		private async void ExportGlobalResultsButtonClick(object sender, RoutedEventArgs e)
		{
			var dialog = new VistaSaveFileDialog();

			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
			dialog.DefaultExt = ".tsv";
            dialog.Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp";

			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				string fileLocation = dialog.FileName;
				await Task.Run(() => this.SingleTargetViewModel.OnExportGlobalResults(fileLocation));
			}
		}

		private void ExportAllGlobalResultsButtonClick(object sender, RoutedEventArgs e)
		{
			var dialog = new VistaSaveFileDialog();

			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
            dialog.DefaultExt = ".tsv";
            dialog.Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp";

			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				string fileLocation = dialog.FileName;
				this.SingleTargetViewModel.OnExportAllGlobalResults(fileLocation);
			}
		}

	    private void ExportTargetInfoButtonClick(object sender, RoutedEventArgs e)
	    {
            var dialog = new VistaSaveFileDialog();

            dialog.AddExtension = true;
            dialog.OverwritePrompt = true;
            dialog.DefaultExt = ".tsv";
            dialog.Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp";

            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string fileLocation = dialog.FileName;
                this.SingleTargetViewModel.OnWriteTargetInfo(fileLocation);
            }
	    }

	    private void AddFragmentButton_OnClick(object sender, RoutedEventArgs e)
	    {
	        double fragmentMz;
	        var validFragment = Double.TryParse(this.FragmentMassTextBox.Text, out fragmentMz);
	        string ionType = (string)this.IonTypeComboBox.SelectedItem;
	        if (validFragment)
	        {
	            this.SingleTargetViewModel.AddFragment(fragmentMz, ionType);
	            this.FragmentSearchListDataGrid.ItemsSource = this.SingleTargetViewModel.FragmentSearchList;
	            this.SearchForFragmentsButton.IsEnabled = true;
	        }
	        else
	        {
               MessageBox.Show("Invalid m/z. Please only use numbers.","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
	        }

	    }

	    private async void SearchForFragmentsButtonClick(object sender, RoutedEventArgs e)
	    {
            FragmentationMode fragmentationMode = (FragmentationMode) this.FragmentationModeComboBox.SelectedItem;
            double hcdMassError = double.Parse(this.HcdErrorTextBox.Text);
            double cidMassError = double.Parse(this.CidErrorTextBox.Text);
            int resultsPerScan = int.Parse(this.ResultsPerScanTextBox.Text);
	        int minMatches = int.Parse(this.MinimumMatchesTextBox.Text);
	        Adduct adduct = (Adduct) this.AdductComboBox2.SelectedItem;

            this.SingleTargetViewModel.OnUpdateTargetAdductFragmentation(adduct, fragmentationMode);
            await Task.Run(() =>this.SingleTargetViewModel.SearchForFragments(hcdMassError, cidMassError,fragmentationMode,resultsPerScan, minMatches, adduct));

            this.MsMsInfoUserControl.MsMsInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);
            this.MsOneInfoUserControl.MsOneInfoViewModel.OnLipidTargetChange(this.SingleTargetViewModel.CurrentLipidTarget);

            this.TargetMzTextBlock.Visibility = Visibility.Visible;
            this.EmpiricalFormulaTextBlock.Visibility = Visibility.Visible;
            this.EmpiricalFormulaRichTextBlock.Visibility = Visibility.Visible;
            this.NumberOfResultsTextBlock.Visibility = Visibility.Visible;
	        this.FragmentSearchResultsDataGrid.Visibility = Visibility.Visible;
            this.ExportFragmentResultsButton.Visibility = Visibility.Visible;
            
            if (this.SingleTargetViewModel.CurrentSpectrumSearchResult != null)
            {
                var dataGrid = this.FragmentSearchResultsDataGrid;

                dataGrid.SelectedItem = this.SingleTargetViewModel.CurrentSpectrumSearchResult;
                dataGrid.ScrollIntoView(this.SingleTargetViewModel.CurrentSpectrumSearchResult);
            }
	    }

	    private void RemoveFragmentButton_OnClick(object sender, RoutedEventArgs e)
	    {

	        List<MsMsSearchUnit> items = this.FragmentSearchListDataGrid.SelectedItems.OfType<MsMsSearchUnit>().ToList();


            if(this.FragmentSearchListDataGrid.SelectedItem != null) this.SingleTargetViewModel.RemoveFragment(items);
	        if (this.SingleTargetViewModel.FragmentSearchList.Count == 0)
	        {
	            this.SearchForFragmentsButton.IsEnabled = false;
                this.RemoveFragmentButton.IsEnabled = false;
	        }
	    }

	    private void FragmentSelectionChange(object sender, SelectionChangedEventArgs e)
	    {
	        this.RemoveFragmentButton.IsEnabled = this.FragmentSearchListDataGrid.SelectedItem != null;
	    }

	    private void ExportFragmentResultsButtonClick(object sender, RoutedEventArgs e)
	    {
            var dialog = new VistaSaveFileDialog();

            dialog.AddExtension = true;
            dialog.OverwritePrompt = true;
            dialog.DefaultExt = ".tsv";
            dialog.Filter = "Tab-Separated Files (*.tsv)|*.tsv|MzTab Files (*.mzTab)|*.mzTab|MSP Library (*.msp)|*.msp";

            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string fileLocation = dialog.FileName;
                this.SingleTargetViewModel.OnWriteFragmentInfo(fileLocation);
            }
	    }
        /*
        public static RenderTargetBitmap GetImage(View view)
        {
            Size size = new Size(view.ActualWidth, view.ActualHeight);
            if (size.IsEmpty)
                return null;

            RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);
            return result;
        }
         * */
	}
}
