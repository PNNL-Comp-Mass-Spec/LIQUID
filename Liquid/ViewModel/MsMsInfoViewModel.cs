using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using Liquid.OxyPlot;
using LiquidBackend.Domain;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
	public class MsMsInfoViewModel : ViewModelBase
	{
		public LipidTarget CurrentLipidTarget { get; private set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
		public PlotModel MsMsHcdPlot { get; private set; }
		public PlotModel MsMsCidPlot { get; private set; }
		public PlotModel IsotopicProfilePlot { get; private set; }
		public PlotModel XicPlot { get; private set; }
		public List<MsMsAnnotation> MsMsAnnotationList { get; private set; }

		public void OnLipidTargetChange(LipidTarget lipidTarget)
		{
			this.CurrentLipidTarget = lipidTarget;
			OnPropertyChanged("CurrentLipidTarget");
		}

		public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
		{
			this.CurrentSpectrumSearchResult = spectrumSearchResult;
			OnPropertyChanged("CurrentSpectrumSearchResult");

			this.CreateMsMsPlots();
		}

		private void CreateMsMsPlots()
		{
			IEnumerable<MsMsSearchResult> hcdSearchResultList = this.CurrentSpectrumSearchResult.HcdSearchResultList.Where(x => x.ObservedPeak != null);
			IEnumerable<MsMsSearchResult> cidSearchResultList = this.CurrentSpectrumSearchResult.CidSearchResultList.Where(x => x.ObservedPeak != null);

			// Reset annotation list
			this.MsMsAnnotationList = new List<MsMsAnnotation>();

			// Create the plot models
			PlotModel hcdPlot = new PlotModel();
			PlotModel cidPlot = new PlotModel();

			if (this.CurrentSpectrumSearchResult.HcdSpectrum != null) hcdPlot = CreateMsMsPlot(hcdSearchResultList, this.CurrentSpectrumSearchResult.HcdSpectrum);
			if (this.CurrentSpectrumSearchResult.CidSpectrum != null) cidPlot = CreateMsMsPlot(cidSearchResultList, this.CurrentSpectrumSearchResult.CidSpectrum);

			this.MsMsHcdPlot = hcdPlot;
			this.MsMsCidPlot = cidPlot;

			// Update GUI
			OnPropertyChanged("MsMsHcdPlot");
			OnPropertyChanged("MsMsCidPlot");
			OnPropertyChanged("MsMsAnnotationList");
		}

		private PlotModel CreateMsMsPlot(IEnumerable<MsMsSearchResult> searchResultList, ProductSpectrum productSpectrum)
		{
			SpectrumSearchResult spectrumSearchResult = this.CurrentSpectrumSearchResult;
			LipidTarget lipidTarget = this.CurrentLipidTarget;
			string commonName = lipidTarget.StrippedDisplay;
			int parentScan = spectrumSearchResult.PrecursorSpectrum.ScanNum;
			var peakList = productSpectrum.Peaks;
			var fragmentationType = productSpectrum.ActivationMethod == ActivationMethod.CID ? FragmentationType.CID : FragmentationType.HCD;

			if (!peakList.Any()) return new PlotModel();

			string plotTitle = commonName + "\nMS/MS Spectrum - " + productSpectrum.ActivationMethod + " - " + productSpectrum.ScanNum + " // Parent Scan - " + parentScan + " (" + productSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString("0.0000") + " m/z)";

			PlotModel plotModel = new PlotModel(plotTitle);
			plotModel.TitleFontSize = 14;
			plotModel.Padding = new OxyThickness(0);
			plotModel.PlotMargins = new OxyThickness(0);

			var mzPeakSeries = new StemSeries();
			mzPeakSeries.Color = OxyColors.Black;
			mzPeakSeries.StrokeThickness = 0.5;
			mzPeakSeries.Title = "Peaks";

			var annotatedPeakSeries = new StemSeries();
			annotatedPeakSeries.Color = OxyColors.Green;
			annotatedPeakSeries.StrokeThickness = 2;
			annotatedPeakSeries.Title = "Matched Ions";

			var diagnosticPeakSeries = new StemSeries();
			diagnosticPeakSeries.Color = OxyColors.Red;
			diagnosticPeakSeries.StrokeThickness = 2;
			diagnosticPeakSeries.Title = "Diagnostic Ion";

			plotModel.IsLegendVisible = true;
			plotModel.LegendPosition = LegendPosition.TopRight;
			plotModel.LegendPlacement = LegendPlacement.Inside;
			plotModel.LegendMargin = 0;
			plotModel.LegendFontSize = 10;

			double minMz = double.MaxValue;
			double maxMz = double.MinValue;
			double maxIntensity = double.MinValue;
			double secondMaxIntensity = double.MinValue;

			foreach (var msPeak in peakList)
			{
				double mz = msPeak.Mz;
				double intensity = msPeak.Intensity;

				if (mz < minMz) minMz = mz;
				if (mz > maxMz) maxMz = mz;
				if (intensity > maxIntensity)
				{
					secondMaxIntensity = maxIntensity;
					maxIntensity = intensity;
				}
				else if (intensity > secondMaxIntensity)
				{
					secondMaxIntensity = intensity;
				}

				DataPoint dataPoint = new DataPoint(mz, intensity);

				bool isDiagnostic = false;

				var matchedPeaks = searchResultList.Where(x => x.ObservedPeak.Equals(msPeak));
				foreach (var matchedSearchResult in matchedPeaks)
				{
					MsMsAnnotation annotation = new MsMsAnnotation(fragmentationType);
					annotation.Text = matchedSearchResult.TheoreticalPeak.DescriptionForUi;
					annotation.Position = dataPoint;
					annotation.VerticalAlignment = VerticalAlignment.Middle;
					annotation.HorizontalAlignment = HorizontalAlignment.Left;
					annotation.Rotation = -90;
					annotation.StrokeThickness = 0;
					annotation.Offset = new ScreenVector(0, -5);
					annotation.Selectable = true;

					plotModel.Annotations.Add(annotation);
					this.MsMsAnnotationList.Add(annotation);

					if (!isDiagnostic) isDiagnostic = matchedSearchResult.TheoreticalPeak.IsDiagnostic;
				}

				if (isDiagnostic)
				{
					diagnosticPeakSeries.Points.Add(dataPoint);
				}
				else if (matchedPeaks.Any())
				{
					annotatedPeakSeries.Points.Add(dataPoint);
				}
				else
				{
					mzPeakSeries.Points.Add(dataPoint);
				}
			}

			plotModel.Series.Add(mzPeakSeries);
			plotModel.Series.Add(annotatedPeakSeries);
			plotModel.Series.Add(diagnosticPeakSeries);

			var yAxis = new LinearAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			//yAxis.Maximum = maxIntensity + (maxIntensity * .05);
			//yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			if (secondMaxIntensity > 0)
			{
				yAxis.Maximum = secondMaxIntensity + (secondMaxIntensity*.25);
				yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity*.25);
				yAxis.ShowMinorTicks = true;
				yAxis.MajorStep = (secondMaxIntensity + (secondMaxIntensity*.25))/5.0;
				yAxis.StringFormat = "0.0E00";
			}
			else if (maxIntensity > 0)
			{
				yAxis.Maximum = maxIntensity + (maxIntensity * .25);
				yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .25);
				yAxis.ShowMinorTicks = true;
				yAxis.MajorStep = (maxIntensity + (maxIntensity * .25)) / 5.0;
				yAxis.StringFormat = "0.0E00";
			}
			else
			{
				yAxis.Maximum = 1;
				yAxis.AbsoluteMaximum = 1;
				yAxis.ShowMinorTicks = false;
				yAxis.MajorStep = 1;
				yAxis.StringFormat = "0.0";
			}
			
			yAxis.AxisChanged += OnYAxisChange;

			var xAxis = new LinearAxis(AxisPosition.Bottom, "m/z");
			xAxis.Minimum = minMz - 20;
			xAxis.AbsoluteMinimum = minMz - 20;
			xAxis.Maximum = maxMz + 20;
			xAxis.AbsoluteMaximum = maxMz + 20;

			plotModel.Axes.Add(yAxis);
			plotModel.Axes.Add(xAxis);

			return plotModel;
		}
	}
}
