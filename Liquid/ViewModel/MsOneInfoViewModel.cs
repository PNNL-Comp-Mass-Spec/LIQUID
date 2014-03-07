using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
	public class MsOneInfoViewModel : ViewModelBase
	{
		public LipidTarget CurrentLipidTarget { get; private set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
		public PlotModel IsotopicProfilePlot { get; set; }
		public PlotModel XicPlot { get; set; }

		public void OnLipidTargetChange(LipidTarget lipidTarget)
		{
			this.CurrentLipidTarget = lipidTarget;
			OnPropertyChanged("CurrentLipidTarget");
		}

		public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
		{
			this.CurrentSpectrumSearchResult = spectrumSearchResult;
			OnPropertyChanged("CurrentSpectrumSearchResult");

			this.CreateIsotopicProfilePlot();
			this.CreateXicPlot();
		}

		private void CreateIsotopicProfilePlot()
		{
			PlotModel plotModel = new PlotModel("Isotopic Profile");
			plotModel.Padding = new OxyThickness(0);
			plotModel.PlotMargins = new OxyThickness(0);

			var mzPeakSeries = new StemSeries();
			mzPeakSeries.Color = OxyColors.Black;
			mzPeakSeries.StrokeThickness = 1;

			//var isotopicPeakSeries = new StemSeries();
			//isotopicPeakSeries.Color = OxyColors.Red;
			//isotopicPeakSeries.StrokeThickness = 2;

			double currentMz = this.CurrentLipidTarget.Composition.Mass;

			double minMz = double.MaxValue;
			double maxMz = double.MinValue;
			double minLocalMz = currentMz - 2;
			double maxLocalMz = currentMz + 5;
			double maxIntensity = double.MinValue;
			double maxLocalIntensity = double.MinValue;

			var massSpectrum = this.CurrentSpectrumSearchResult.PrecursorSpectrum.Peaks;

			foreach (var msPeak in massSpectrum)
			{
				double mz = msPeak.Mz;
				double intensity = msPeak.Intensity;

				if (intensity > maxLocalIntensity && mz > minLocalMz && mz < maxLocalMz) maxLocalIntensity = intensity;
				if (intensity > maxIntensity) maxIntensity = intensity;
				if (mz < minMz) minMz = mz;
				if (mz > maxMz) maxMz = mz;

				DataPoint dataPoint = new DataPoint(mz, intensity);
				mzPeakSeries.Points.Add(dataPoint);
			}

			plotModel.Series.Add(mzPeakSeries);

			//var isotopicProfile = this.CurrentInformedResultUnit.IsotopicProfile;

			//foreach (var peak in isotopicProfile.Peaklist)
			//{
			//	DataPoint dataPoint = new DataPoint(peak.XValue, peak.Height);
			//	isotopicPeakSeries.Points.Add(dataPoint);
			//}

			//plotModel.Series.Add(isotopicPeakSeries);

			var yAxis = new LinearAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			yAxis.Maximum = maxLocalIntensity + (maxLocalIntensity * .05);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.ShowMinorTicks = true;
			yAxis.MajorStep = (maxLocalIntensity + (maxLocalIntensity * .05)) / 5.0;
			//yAxis.IsAxisVisible = false;
			yAxis.AxisTickToLabelDistance = 0;
			yAxis.StringFormat = "0.0E00";
			yAxis.FontSize = 10;
			yAxis.AxisChanged += OnYAxisChange;

			var xAxis = new LinearAxis(AxisPosition.Bottom, "m/z");
			xAxis.Minimum = minLocalMz;
			xAxis.AbsoluteMinimum = minMz - 10;
			xAxis.Maximum = maxLocalMz;
			xAxis.AbsoluteMaximum = maxMz + 10;

			plotModel.Axes.Add(yAxis);
			plotModel.Axes.Add(xAxis);

			this.IsotopicProfilePlot = plotModel;
			OnPropertyChanged("IsotopicProfilePlot");
		}

		private void CreateXicPlot()
		{
			PlotModel plotModel = new PlotModel("XIC");
			plotModel.Padding = new OxyThickness(0);
			plotModel.PlotMargins = new OxyThickness(0);

			var mzPeakSeries = new LineSeries();
			mzPeakSeries.Color = OxyColors.Black;
			mzPeakSeries.StrokeThickness = 1;

			var peakCenterSeries = new StemSeries();
			peakCenterSeries.Color = OxyColors.Red;
			peakCenterSeries.StrokeThickness = 0.5;
			peakCenterSeries.LineStyle = LineStyle.Dash;
			peakCenterSeries.Title = "Apex";

			var precursorSeries = new StemSeries();
			precursorSeries.Color = OxyColors.Green;
			precursorSeries.StrokeThickness = 0.5;
			precursorSeries.LineStyle = LineStyle.Dash;
			precursorSeries.Title = "Precursor";

			plotModel.IsLegendVisible = true;
			plotModel.LegendPosition = LegendPosition.TopRight;
			plotModel.LegendPlacement = LegendPlacement.Inside;
			plotModel.LegendMargin = 0;
			plotModel.LegendFontSize = 10;

			double peakCenter = this.CurrentSpectrumSearchResult.ApexScanNum;
			double localMinScanLc = peakCenter - 500;
			double localMaxScanLc = peakCenter + 500;

			double absoluteMaxScanLc = double.MinValue;
			double absoluteMinScanLc = double.MaxValue;
			double maxIntensity = double.MinValue;
			double localMaxIntensity = double.MinValue;

			var chromatogram = this.CurrentSpectrumSearchResult.Xic;

			foreach (var xicPeak in chromatogram)
			{
				double scanLc = xicPeak.ScanNum;
				double intensity = xicPeak.Intensity;

				if (scanLc > absoluteMaxScanLc) absoluteMaxScanLc = scanLc;
				if (scanLc < absoluteMinScanLc) absoluteMinScanLc = scanLc;
				if (intensity > maxIntensity) maxIntensity = intensity;
				if (intensity > localMaxIntensity && scanLc <= localMaxScanLc && scanLc >= localMinScanLc) localMaxIntensity = intensity;

				DataPoint dataPoint = new DataPoint(scanLc, intensity);
				mzPeakSeries.Points.Add(dataPoint);
			}

			DataPoint peakCenterDataPoint = new DataPoint(peakCenter, maxIntensity);
			peakCenterSeries.Points.Add(peakCenterDataPoint);

			int precursorScan = this.CurrentSpectrumSearchResult.PrecursorSpectrum.ScanNum;
			DataPoint precursorDataPoint = new DataPoint(precursorScan, maxIntensity);
			precursorSeries.Points.Add(precursorDataPoint);

			plotModel.Series.Add(mzPeakSeries);
			plotModel.Series.Add(peakCenterSeries);
			plotModel.Series.Add(precursorSeries);

			var yAxis = new LinearAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			yAxis.Maximum = localMaxIntensity + (localMaxIntensity * .05);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.ShowMinorTicks = true;
			yAxis.MajorStep = (maxIntensity + (maxIntensity * .05)) / 5.0;
			//yAxis.IsAxisVisible = false;
			yAxis.AxisTickToLabelDistance = 0;
			yAxis.StringFormat = "0.0E00";
			yAxis.FontSize = 10;
			yAxis.AxisChanged += OnYAxisChange;

			var xAxis = new LinearAxis(AxisPosition.Bottom, "Scan #");
			xAxis.Minimum = absoluteMinScanLc - 10;
			xAxis.AbsoluteMinimum = absoluteMinScanLc - 10;
			xAxis.Maximum = absoluteMaxScanLc + 10;
			xAxis.AbsoluteMaximum = absoluteMaxScanLc + 10;

			plotModel.Axes.Add(yAxis);
			plotModel.Axes.Add(xAxis);

			this.XicPlot = plotModel;
			OnPropertyChanged("XicPlot");
		}
	}
}
