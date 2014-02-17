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
		public SingleTargetViewModel SingleTargetViewModel { get; set; }

		public SingleTargetWindow()
		{
			InitializeComponent();

			this.SingleTargetViewModel = new SingleTargetViewModel();
			this.DataContext = this.SingleTargetViewModel;

			this.FragmentationModeComboBox.SelectedValue = FragmentationMode.Positive;
			this.TargetMzTextBlock.Visibility = Visibility.Collapsed;
			this.NumberOfResultsTextBlock.Visibility = Visibility.Collapsed;
			this.SpectrumSearchResultsDataGrid.Visibility = Visibility.Collapsed;
			this.SpectrumResultPanel.Visibility = Visibility.Collapsed;
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
			string empiricalFormula = this.EmpiricalFormulaTextBox.Text;
			FragmentationMode fragmentationMode = (FragmentationMode)this.FragmentationModeComboBox.SelectedItem;
			double hcdMassError = double.Parse(this.HcdErrorTextBox.Text);
			double cidMassError = double.Parse(this.CidErrorTextBox.Text);

			this.SingleTargetViewModel.SearchForTarget(commonName, empiricalFormula, fragmentationMode, hcdMassError, cidMassError);

			if (this.SingleTargetViewModel.CurrentSpectrumSearchResult != null)
			{
				var dataGrid = this.SpectrumSearchResultsDataGrid;

				dataGrid.SelectedItem = this.SingleTargetViewModel.CurrentSpectrumSearchResult;
				dataGrid.ScrollIntoView(this.SingleTargetViewModel.CurrentSpectrumSearchResult);
			}

			this.TargetMzTextBlock.Visibility = Visibility.Visible;
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

					this.SpectrumResultPanel.Visibility = Visibility.Visible;
				}
			}
		}
	}
}
