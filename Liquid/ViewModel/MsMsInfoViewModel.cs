using System.Collections.Generic;
using System.Linq;
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
        private const int MINOR_TICK_SIZE = 3;

        public LipidTarget CurrentLipidTarget { get; private set; }
        public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
        public PlotModel MsMsHcdPlot { get; private set; }
        public PlotModel MsMsCidPlot { get; private set; }
        public PlotModel IsotopicProfilePlot { get; private set; }
        public PlotModel XicPlot { get; private set; }
        public List<MsMsAnnotation> MsMsAnnotationList { get; private set; }

        public void OnLipidTargetChange(LipidTarget lipidTarget)
        {
            CurrentLipidTarget = lipidTarget;
            OnPropertyChanged("CurrentLipidTarget");
        }

        public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
        {
            CurrentSpectrumSearchResult = spectrumSearchResult;
            OnPropertyChanged("CurrentSpectrumSearchResult");

            CreateMsMsPlots();
        }

        private void CreateMsMsPlots()
        {
            var hcdSearchResultList = CurrentSpectrumSearchResult.HcdSearchResultList.Where(x => x.ObservedPeak != null);
            var cidSearchResultList = CurrentSpectrumSearchResult.CidSearchResultList.Where(x => x.ObservedPeak != null);

            // Reset annotation list
            MsMsAnnotationList = new List<MsMsAnnotation>();

            // Create the plot models
            var hcdPlot = new PlotModel();
            var cidPlot = new PlotModel();

            if (CurrentSpectrumSearchResult.HcdSpectrum != null) hcdPlot = CreateMsMsPlot(hcdSearchResultList, CurrentSpectrumSearchResult.HcdSpectrum);
            if (CurrentSpectrumSearchResult.CidSpectrum != null) cidPlot = CreateMsMsPlot(cidSearchResultList, CurrentSpectrumSearchResult.CidSpectrum);

            MsMsHcdPlot = hcdPlot;
            MsMsCidPlot = cidPlot;

            // Update GUI
            OnPropertyChanged("MsMsHcdPlot");
            OnPropertyChanged("MsMsCidPlot");
            OnPropertyChanged("MsMsAnnotationList");
        }

        private PlotModel CreateMsMsPlot(IEnumerable<MsMsSearchResult> searchResultList, ProductSpectrum productSpectrum)
        {
            var spectrumSearchResult = CurrentSpectrumSearchResult;
            var lipidTarget = CurrentLipidTarget;
            var commonName = lipidTarget.StrippedDisplay;
            var parentScan = spectrumSearchResult.PrecursorSpectrum?.ScanNum ?? 0;
            var peakList = productSpectrum.Peaks;
            var fragmentationType = productSpectrum.ActivationMethod == ActivationMethod.CID ? FragmentationType.CID : FragmentationType.HCD;

            if (!peakList.Any()) return new PlotModel();

            var plotTitle = commonName + "\nMS/MS Spectrum - " + productSpectrum.ActivationMethod + " - " + productSpectrum.ScanNum + " // Precursor Scan - " + parentScan + " (" + productSpectrum.IsolationWindow.IsolationWindowTargetMz.ToString("0.0000") + " m/z)";

            var plotModel = new PlotModel
            {
                Title = plotTitle,
                TitleFontSize = 14,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0)
            };
            var mzPeakSeries = new StemSeries
            {
                Color = OxyColors.Black,
                StrokeThickness = 0.5,
                Title = "Peaks"
            };
            var annotatedPeakSeries = new StemSeries
            {
                Color = OxyColors.Green,
                StrokeThickness = 2,
                Title = "Matched Ions"
            };
            var diagnosticPeakSeries = new StemSeries
            {
                Color = OxyColors.Red,
                StrokeThickness = 2,
                Title = "Diagnostic Ion"
            };
            plotModel.IsLegendVisible = true;
            plotModel.LegendPosition = LegendPosition.TopRight;
            plotModel.LegendPlacement = LegendPlacement.Inside;
            plotModel.LegendMargin = 0;
            plotModel.LegendFontSize = 10;

            var minMz = double.MaxValue;
            var maxMz = double.MinValue;
            var maxIntensity = double.MinValue;
            var secondMaxIntensity = double.MinValue;

            foreach (var msPeak in peakList)
            {
                var mz = msPeak.Mz;
                var intensity = msPeak.Intensity;

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

                var dataPoint = new DataPoint(mz, intensity);

                var isDiagnostic = false;

                var matchedPeaks = searchResultList.Where(x => x.ObservedPeak.Equals(msPeak));
                foreach (var matchedSearchResult in matchedPeaks)
                {
                    var annotation = new MsMsAnnotation(fragmentationType)
                    {
                        Text = matchedSearchResult.TheoreticalPeak.DescriptionForUi,
                        TextPosition = dataPoint,
                        TextVerticalAlignment = VerticalAlignment.Middle,
                        TextHorizontalAlignment = HorizontalAlignment.Left,
                        TextRotation = -90,
                        StrokeThickness = 0,
                        Offset = new ScreenVector(0, -5),
                        Selectable = true
                    };
                    plotModel.Annotations.Add(annotation);
                    MsMsAnnotationList.Add(annotation);

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

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0
            };
            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            if (secondMaxIntensity > 0)
            {
                yAxis.Maximum = secondMaxIntensity + secondMaxIntensity * .25;
                yAxis.AbsoluteMaximum = maxIntensity + maxIntensity * .25;
                yAxis.MinorTickSize = MINOR_TICK_SIZE;
                yAxis.MajorStep = (secondMaxIntensity + secondMaxIntensity * .25) / 5.0;
                yAxis.StringFormat = "0.0E00";
            }
            else if (maxIntensity > 0)
            {
                yAxis.Maximum = maxIntensity + maxIntensity * .25;
                yAxis.AbsoluteMaximum = maxIntensity + maxIntensity * .25;
                yAxis.MinorTickSize = MINOR_TICK_SIZE;
                yAxis.MajorStep = (maxIntensity + maxIntensity * .25) / 5.0;
                yAxis.StringFormat = "0.0E00";
            }
            else
            {
                yAxis.Maximum = 1;
                yAxis.AbsoluteMaximum = 1;
                yAxis.MinorTickSize = 0;
                yAxis.MajorStep = 1;
                yAxis.StringFormat = "0.0";
            }

            yAxis.AxisChanged += OnYAxisChange;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                Minimum = minMz - 20,
                AbsoluteMinimum = minMz - 20,
                Maximum = maxMz + 20,
                AbsoluteMaximum = maxMz + 20
            };
            plotModel.Axes.Add(yAxis);
            plotModel.Axes.Add(xAxis);

            return plotModel;
        }
    }
}
