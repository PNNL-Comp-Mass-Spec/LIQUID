using System;
using System.Linq;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Liquid.ViewModel
{
    public class MsOneInfoViewModel : ViewModelBase
    {
        // Ignore Spelling: Xic

        private const int MINOR_TICK_SIZE = 3;

        public LipidTarget CurrentLipidTarget { get; private set; }
        public SpectrumSearchResult CurrentSpectrumSearchResult { get; private set; }
        public PlotModel IsotopicProfilePlot { get; set; }
        public PlotModel XicPlot { get; set; }

        public double PearsonCorrScore { get; private set; }
        public double PearsonCorrMinus1Score { get; private set; }

        public double CosineScore { get; private set; }
        public double CosineMinus1Score { get; private set; }

        public double CurrentMz { get; private set; }
        public double CurrentPpmError { get; private set; }

        //Used for the calculation of area under ms1 xic curve over manually specified range
        public double AreaUnderCurve { get; private set; }
        private double _startScanForAreaUnderTheCurve;
        private double _stopScanForAreaUnderTheCurve;
        public double StartScanForAreaUnderCurve
        {
            get => _startScanForAreaUnderTheCurve;
            set
            {
                _startScanForAreaUnderTheCurve = Convert.ToDouble(value);
                GetAreaUnderMs1();
            }
        }
        public double StopScanForAreaUnderCurve
        {
            get => _stopScanForAreaUnderTheCurve;
            set
            {
                _stopScanForAreaUnderTheCurve = Convert.ToDouble(value);
                GetAreaUnderMs1();
            }
        }

        public void OnLipidTargetChange(LipidTarget lipidTarget)
        {
            CurrentLipidTarget = lipidTarget;
            OnPropertyChanged("CurrentLipidTarget");
        }

        public void OnSpectrumSearchResultChange(SpectrumSearchResult spectrumSearchResult)
        {
            CurrentSpectrumSearchResult = spectrumSearchResult;
            OnPropertyChanged("CurrentSpectrumSearchResult");
            if (CurrentSpectrumSearchResult.PrecursorSpectrum != null)
            {
                CreateIsotopicProfilePlot();
                CreateXicPlot();
            }

            UpdatePpmError();

            if (CurrentSpectrumSearchResult.PrecursorSpectrum != null)
            {
                if (CurrentLipidTarget.Composition != null) UpdateFitScores();

                StartScanForAreaUnderCurve = CurrentSpectrumSearchResult.ApexScanNum;
                StopScanForAreaUnderCurve = CurrentSpectrumSearchResult.ApexScanNum;
            }
        }

        /// <summary>
        /// Added by grant to calculate area under curve over predefined range
        /// </summary>
        private void GetAreaUnderMs1()
        {
            var chromatogram = CurrentSpectrumSearchResult.Xic;
            double areaUnderCurve = 0;

            foreach (var xicPeak in chromatogram)
            {
                double scanLc = xicPeak.ScanNum;
                var intensity = xicPeak.Intensity;

                if (scanLc >= StartScanForAreaUnderCurve && scanLc <= StopScanForAreaUnderCurve)
                {
                    areaUnderCurve += intensity;
                }
            }
            AreaUnderCurve = areaUnderCurve;
            CurrentSpectrumSearchResult.PeakArea = areaUnderCurve;
            OnPropertyChanged("AreaUnderCurve");
            OnPropertyChanged("StartScanForAreaUnderCurve");
            OnPropertyChanged("StopScanForAreaUnderCurve");
        }

        private void UpdatePpmError()
        {
            var targetMz = CurrentLipidTarget.MzRounded;
            if (CurrentSpectrumSearchResult.PrecursorSpectrum != null)
            {
                var massSpectrum = CurrentSpectrumSearchResult.PrecursorSpectrum.Peaks;
                var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
                CurrentMz = closestPeak.Mz;
                OnPropertyChanged("CurrentMz");
            }
            else
            {
                var isolationMz = CurrentSpectrumSearchResult.HcdSpectrum?.IsolationWindow.IsolationWindowTargetMz ??
                    CurrentSpectrumSearchResult.CidSpectrum.IsolationWindow.IsolationWindowTargetMz;

                CurrentMz = isolationMz;
                OnPropertyChanged("CurrentMz");
            }

            CurrentPpmError = LipidUtil.PpmError(targetMz, CurrentMz);
            OnPropertyChanged("CurrentPpmError");
        }

        private void CreateIsotopicProfilePlot()
        {
            var plotModel = new PlotModel()
            {
                Title = "Isotopic Profile",
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0)
            };
            var mzPeakSeries = new StemSeries()
            {
                Color = OxyColors.Black,
                StrokeThickness = 1
            };

            //var isotopicPeakSeries = new StemSeries();
            //isotopicPeakSeries.Color = OxyColors.Red;
            //isotopicPeakSeries.StrokeThickness = 2;

            var currentMz = CurrentLipidTarget.MzRounded;

            var minMz = double.MaxValue;
            var maxMz = double.MinValue;
            var minLocalMz = currentMz - 2;
            var maxLocalMz = currentMz + 5;
            var maxIntensity = double.MinValue;
            var maxLocalIntensity = double.MinValue;

            var massSpectrum = CurrentSpectrumSearchResult.PrecursorSpectrum.Peaks;

            foreach (var msPeak in massSpectrum)
            {
                var mz = msPeak.Mz;
                var intensity = msPeak.Intensity;

                if (intensity > maxLocalIntensity && mz > minLocalMz && mz < maxLocalMz) maxLocalIntensity = intensity;
                if (intensity > maxIntensity) maxIntensity = intensity;
                if (mz < minMz) minMz = mz;
                if (mz > maxMz) maxMz = mz;

                var dataPoint = new DataPoint(mz, intensity);
                mzPeakSeries.Points.Add(dataPoint);
            }

            plotModel.Series.Add(mzPeakSeries);

            //var isotopicProfile = this.CurrentInformedResultUnit.IsotopicProfile;

            //foreach (var peak in isotopicProfile.PeakList)
            //{
            //  DataPoint dataPoint = new DataPoint(peak.XValue, peak.Height);
            //  isotopicPeakSeries.Points.Add(dataPoint);
            //}

            //plotModel.Series.Add(isotopicPeakSeries);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = maxLocalIntensity + maxLocalIntensity * .05,
                AbsoluteMaximum = maxIntensity + maxIntensity * .05,
                MinorTickSize = MINOR_TICK_SIZE,
                MajorStep = (maxLocalIntensity + maxLocalIntensity * .05) / 5.0,
                //yAxis.IsAxisVisible = false;
                StringFormat = "0.0E00",
                FontSize = 10
            };
            yAxis.AxisChanged += OnYAxisChange;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                TitlePosition = 0.9,
                AxisTitleDistance = 1,
                Minimum = minLocalMz,
                AbsoluteMinimum = minMz - 10,
                Maximum = maxLocalMz,
                AbsoluteMaximum = maxMz + 10
            };
            plotModel.Axes.Add(yAxis);
            plotModel.Axes.Add(xAxis);

            IsotopicProfilePlot = plotModel;
            OnPropertyChanged("IsotopicProfilePlot");
        }

        private void UpdateFitScores()
        {
            var pearsonCorrelationCalculator = new PearsonCorrelationFitUtil();
            PearsonCorrScore = pearsonCorrelationCalculator.GetFitScore(
                CurrentSpectrumSearchResult,
                CurrentLipidTarget.Composition);
            OnPropertyChanged("PearsonCorrScore");

            PearsonCorrMinus1Score = pearsonCorrelationCalculator.GetFitMinus1Score(
                CurrentSpectrumSearchResult,
                CurrentLipidTarget.Composition);
            OnPropertyChanged("PearsonCorrMinus1Score");

            var cosineCalculator = new CosineFitUtil();
            CosineScore = cosineCalculator.GetFitScore(
                CurrentSpectrumSearchResult,
                CurrentLipidTarget.Composition);
            OnPropertyChanged("CosineScore");

            CosineMinus1Score = cosineCalculator.GetFitMinus1Score(
                CurrentSpectrumSearchResult,
                CurrentLipidTarget.Composition);
            OnPropertyChanged("CosineMinus1Score");
        }

        private void CreateXicPlot()
        {
            var plotModel = new PlotModel()
            {
                Title = "XIC",
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0)
            };
            var mzPeakSeries = new LineSeries()
            {
                Color = OxyColors.Black,
                StrokeThickness = 1
            };
            var peakCenterSeries = new StemSeries()
            {
                Color = OxyColors.Red,
                StrokeThickness = 0.5,
                LineStyle = LineStyle.Dash,
                Title = "Apex"
            };
            var precursorSeries = new StemSeries()
            {
                Color = OxyColors.Green,
                StrokeThickness = 0.5,
                LineStyle = LineStyle.Dash,
                Title = "Precursor"
            };
            plotModel.IsLegendVisible = true;
            plotModel.LegendPosition = LegendPosition.TopRight;
            plotModel.LegendPlacement = LegendPlacement.Inside;
            plotModel.LegendMargin = 0;
            plotModel.LegendFontSize = 10;

            double peakCenter = CurrentSpectrumSearchResult.ApexScanNum;
            var localMinScanLc = peakCenter - 500;
            var localMaxScanLc = peakCenter + 500;

            var absoluteMaxScanLc = double.MinValue;
            var absoluteMinScanLc = double.MaxValue;
            var maxIntensity = double.MinValue;
            var localMaxIntensity = double.MinValue;

            var chromatogram = CurrentSpectrumSearchResult.Xic;

            foreach (var xicPeak in chromatogram)
            {
                double scanLc = xicPeak.ScanNum;
                var intensity = xicPeak.Intensity;

                if (scanLc > absoluteMaxScanLc) absoluteMaxScanLc = scanLc;
                if (scanLc < absoluteMinScanLc) absoluteMinScanLc = scanLc;
                if (intensity > maxIntensity) maxIntensity = intensity;
                if (intensity > localMaxIntensity && scanLc <= localMaxScanLc && scanLc >= localMinScanLc) localMaxIntensity = intensity;

                var dataPoint = new DataPoint(scanLc, intensity);
                mzPeakSeries.Points.Add(dataPoint);
            }

            var peakCenterDataPoint = new DataPoint(peakCenter, maxIntensity);
            peakCenterSeries.Points.Add(peakCenterDataPoint);

            var precursorScan = CurrentSpectrumSearchResult.PrecursorSpectrum.ScanNum;
            var precursorDataPoint = new DataPoint(precursorScan, maxIntensity);
            precursorSeries.Points.Add(precursorDataPoint);

            plotModel.Series.Add(mzPeakSeries);
            plotModel.Series.Add(peakCenterSeries);
            plotModel.Series.Add(precursorSeries);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = localMaxIntensity + localMaxIntensity * .05,
                AbsoluteMaximum = maxIntensity + maxIntensity * .05,
                MinorTickSize = MINOR_TICK_SIZE,
                MajorStep = (localMaxIntensity + localMaxIntensity * .05) / 5.0,
                //yAxis.IsAxisVisible = false;
                StringFormat = "0.0E00",
                FontSize = 10
            };
            yAxis.AxisChanged += OnYAxisChange;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Scan #",
                TitlePosition = 0.9,
                AxisTitleDistance = 1,
                Minimum = peakCenter - 500,
                AbsoluteMinimum = absoluteMinScanLc - 500,
                Maximum = peakCenter + 500,
                AbsoluteMaximum = absoluteMaxScanLc + 500
            };
            plotModel.Axes.Add(yAxis);
            plotModel.Axes.Add(xAxis);

            XicPlot = plotModel;
            OnPropertyChanged("XicPlot");
        }

        public override void Dispose()
        {
            // Nothing to do
        }
    }
}
