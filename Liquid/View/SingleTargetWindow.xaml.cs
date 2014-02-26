using System;
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
			this.TargetMzTextBlock.Visibility = Visibility.Collapsed;
			this.EmpiricalFormulaTextBlock.Visibility = Visibility.Collapsed;
			this.EmpiricalFormulaRichTextBlock.Visibility = Visibility.Collapsed;
			this.NumberOfResultsTextBlock.Visibility = Visibility.Collapsed;
			this.SpectrumSearchResultsDataGrid.Visibility = Visibility.Collapsed;
			this.MsMsInfoUserControl.Visibility = Visibility.Collapsed;
			this.MsOneInfoUserControl.Visibility = Visibility.Collapsed;
		}

		private async void RawFileButtonClick(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog and Set filter for file extension and default file extension
			var dialog = new VistaOpenFileDialog { DefaultExt = ".raw", Filter = "Thermo(*.raw)|*.raw" };

			this.RawFileLocationTextBlock.Visibility = Visibility.Hidden;

			// Get the selected file name and display in a TextBox 
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				// Open file 
				string fileName = dialog.FileName;
				FileInfo fileInfo = new FileInfo(fileName);

				string extension = fileInfo.Extension.ToLower();
				if (extension.Contains("raw"))
				{
					await Task.Run(() => this.SingleTargetViewModel.UpdateRawFileLocation(fileInfo.FullName));
				}
				else
				{
					// Invalid file type ... should be impossible
				}
			}

			this.RawFileLocationTextBlock.Visibility = Visibility.Visible;
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
			if (this.SingleTargetViewModel.CurrentSpectrumSearchResult != null)
			{
				var dataGrid = this.SpectrumSearchResultsDataGrid;

				dataGrid.SelectedItem = this.SingleTargetViewModel.CurrentSpectrumSearchResult;
				dataGrid.ScrollIntoView(this.SingleTargetViewModel.CurrentSpectrumSearchResult);
			}

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
					this.SingleTargetViewModel.OnSpectrumSearchResultChange(spectrumSearchResult);
					
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
	}
}
