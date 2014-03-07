using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Axes;

namespace Liquid.ViewModel
{
	public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
	{
		public virtual string DisplayName { get; protected set; }

		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This 
		/// method does not exist in a Release build.
		/// </summary>
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public void VerifyPropertyName(string propertyName)
		{
			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[propertyName] == null)
			{
				string msg = "Invalid property name: " + propertyName;

				if (this.ThrowOnInvalidPropertyName)
					throw new Exception(msg);
				else
					Debug.Fail(msg);
			}
		}

		/// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might 
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			this.VerifyPropertyName(propertyName);

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		protected void OnYAxisChange(object sender, AxisChangedEventArgs e)
		{
			LinearAxis yAxis = sender as LinearAxis;

			// Set to use 5 major labels no matter where you are zoomed
			yAxis.MajorStep = yAxis.ActualMaximum / 5.0;

			// No need to update anything else if the minimum is already <= 0
			if (yAxis.ActualMinimum <= 0) return;

			// Set the minimum to 0 and refresh the plot
			yAxis.Zoom(0, yAxis.ActualMaximum);

			yAxis.PlotModel.RefreshPlot(true);
		}
	}
}
