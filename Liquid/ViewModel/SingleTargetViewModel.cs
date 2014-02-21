using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using Liquid.OxyPlot;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
	public class SingleTargetViewModel : ViewModelBase
	{
		public LcMsRun LcMsRun { get; set; }
		public string RawFileName { get; set; }
		public LipidTarget CurrentLipidTarget { get; set; }
		public List<FragmentationMode> FragmentationModeList { get; set; }
		public List<SpectrumSearchResult> SpectrumSearchResultList { get; set; }
		public SpectrumSearchResult CurrentSpectrumSearchResult { get; set; }
		public PlotModel MsMsHcdPlot { get; set; }
		public PlotModel MsMsCidPlot { get; set; }
		public PlotModel IsotopicProfilePlot { get; set; }
		public PlotModel XicPlot { get; set; }
		public List<MsMsAnnotation> MsMsAnnotationList { get; set; }
		public List<Adduct> AdductList { get; set; }

		public SingleTargetViewModel()
		{
			this.RawFileName = "None Loaded";
			this.FragmentationModeList = new List<FragmentationMode> { FragmentationMode.Positive, FragmentationMode.Negative };
			this.AdductList = new List<Adduct> { Adduct.Hydrogen, Adduct.Ammonium, Adduct.Acetate };
			this.SpectrumSearchResultList = new List<SpectrumSearchResult>();

			// Run asynchronously inside constructor to avoid slow functionality on first target search
			this.WarmUpInformedProteomics();
		}

		public void UpdateRawFileLocation(string rawFileLocation)
		{
			FileInfo rawFileInfo = new FileInfo(rawFileLocation);

			this.RawFileName = rawFileInfo.Name;
			OnPropertyChanged("RawFileName");

			this.LcMsRun = LcMsRun.GetLcMsRun(rawFileLocation, MassSpecDataType.XCaliburRun);
			OnPropertyChanged("LcMsRun");
		}

		public void SearchForTarget(string commonName, Adduct adduct, FragmentationMode fragmentationMode, double hcdMassError, double cidMassError)
		{
			this.CurrentLipidTarget = LipidUtil.CreateLipidTarget(commonName, fragmentationMode, adduct);
			OnPropertyChanged("CurrentLipidTarget");

			this.SpectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(this.CurrentLipidTarget, this.LcMsRun, hcdMassError, cidMassError);
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

		public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
		{
			this.CurrentSpectrumSearchResult = spectrumSearchResult;
			OnPropertyChanged("CurrentSpectrumSearchResult");

			this.CreateMsMsPlots();
			this.CreateIsotopicProfilePlot();
			this.CreateXicPlot();
		}

		private void CreateMsMsPlots()
		{
			IEnumerable<MsMsSearchResult> hcdSearchResultList = this.CurrentSpectrumSearchResult.HcdSearchResultList.Where(x => x.ObservedPeak != null);
			IEnumerable<MsMsSearchResult> cidSearchResultList = this.CurrentSpectrumSearchResult.CidSearchResultList.Where(x => x.ObservedPeak != null);

			// Reset annotation list
			this.MsMsAnnotationList = new List<MsMsAnnotation>();

			// Create the plot models
			PlotModel hcdPlot = CreateMsMsPlot(hcdSearchResultList, this.CurrentSpectrumSearchResult.HcdSpectrum);
			PlotModel cidPlot = CreateMsMsPlot(cidSearchResultList, this.CurrentSpectrumSearchResult.CidSpectrum);

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
			string commonName = lipidTarget.CommonName;
			int parentScan = spectrumSearchResult.PrecursorSpectrum.ScanNum;
			var peakList = productSpectrum.Peaks;
			var fragmentationType = productSpectrum.ActivationMethod == ActivationMethod.CID ? FragmentationType.CID : FragmentationType.HCD;

			string plotTitle = commonName + "\nMS/MS Spectrum - " + productSpectrum.ActivationMethod + " - " + productSpectrum.ScanNum + " // Parent Scan - " + parentScan + " (" + productSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString("0.000") + " m/z)";

			PlotModel plotModel = new PlotModel(plotTitle);
			plotModel.TitleFontSize = 14;
			plotModel.Padding = new OxyThickness(0);
			plotModel.PlotMargins = new OxyThickness(0);

			var mzPeakSeries = new StemSeries();
			mzPeakSeries.Color = OxyColors.Black;
			mzPeakSeries.StrokeThickness = 0.5;
			mzPeakSeries.Title = "Peaks";

			var annotatedPeakSeries = new StemSeries();
			annotatedPeakSeries.Color = OxyColors.Red;
			annotatedPeakSeries.StrokeThickness = 2;
			annotatedPeakSeries.Title = "Matched Ions";

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
				}

				if (matchedPeaks.Any())
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

			var yAxis = new LinearAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			yAxis.Maximum = maxIntensity + (maxIntensity * .05);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.Maximum = secondMaxIntensity + (secondMaxIntensity * .25);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.StringFormat = "0E00";
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

			var yAxis = new InvisibleAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			yAxis.Maximum = maxLocalIntensity + (maxLocalIntensity * .05);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.ShowMinorTicks = false;
			yAxis.IsAxisVisible = false;
			yAxis.AxisTickToLabelDistance = 0;
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

			var yAxis = new InvisibleAxis(AxisPosition.Left, "Intensity");
			yAxis.Minimum = 0;
			yAxis.AbsoluteMinimum = 0;
			yAxis.Maximum = localMaxIntensity + (localMaxIntensity * .05);
			yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
			yAxis.ShowMinorTicks = false;
			yAxis.IsAxisVisible = false;
			yAxis.AxisTickToLabelDistance = 0;
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

		private void OnYAxisChange(object sender, AxisChangedEventArgs e)
		{
			LinearAxis yAxis = sender as LinearAxis;

			// No need to update anything if the minimum is already <= 0
			if (yAxis.ActualMinimum <= 0) return;

			// Set the minimum to 0 and refresh the plot
			yAxis.Zoom(0, yAxis.ActualMaximum);
			yAxis.PlotModel.RefreshPlot(true);
		}

		private async void WarmUpInformedProteomics()
		{
			await Task.Run(() => Composition.H2O.ComputeApproximateIsotopomerEnvelop());
		}
	}
}
