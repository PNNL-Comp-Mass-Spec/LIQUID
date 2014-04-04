using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Liquid
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App() : base()
		{
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
		}

		void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			string errorMessage = string.Format("An unhandled exception occurred: {0}\n{1}", e.Exception.Message, e.Exception.StackTrace);
			MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DataGridCell cell = sender as DataGridCell;
			GridColumnFastEdit(cell, e);
		}

		private void DataGridCell_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			DataGridCell cell = sender as DataGridCell;
			GridColumnFastEdit(cell, e);
		}

		private static void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
		{
			if (cell == null || cell.IsEditing || cell.IsReadOnly)
				return;

			DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
			if (dataGrid == null)
				return;

			if (!cell.IsFocused)
			{
				cell.Focus();
			}

			if (cell.Content is CheckBox)
			{
				if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
				{
					if (!cell.IsSelected)
						cell.IsSelected = true;
				}
				else
				{
					DataGridRow row = FindVisualParent<DataGridRow>(cell);
					if (row != null && !row.IsSelected)
					{
						row.IsSelected = true;
					}
				}
			}
			else
			{
				ComboBox cb = cell.Content as ComboBox;
				if (cb != null)
				{
					//DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
					dataGrid.BeginEdit(e);
					cell.Dispatcher.Invoke(
					 DispatcherPriority.Background,
					 new Action(delegate { }));
					cb.IsDropDownOpen = true;
				}
			}
		}


		private static T FindVisualParent<T>(UIElement element) where T : UIElement
		{
			UIElement parent = element;
			while (parent != null)
			{
				T correctlyTyped = parent as T;
				if (correctlyTyped != null)
				{
					return correctlyTyped;
				}

				parent = VisualTreeHelper.GetParent(parent) as UIElement;
			}
			return null;
		}
	}
}
