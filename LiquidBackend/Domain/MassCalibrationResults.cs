using System;
using System.Collections.Generic;
using System.Linq;
using MultiDimensionalPeakFinding;

namespace LiquidBackend.Domain
{
	public class MassCalibrationResults
	{
		private readonly SavitzkyGolaySmoother _smoother;

		public SortedDictionary<double, int> RawResults { get; }
		public SortedDictionary<double, double> SmoothedResults { get; private set; }
		public double PpmError { get; private set; }
		public double ErrorWidth { get; private set; }

		public MassCalibrationResults(SortedDictionary<double, int> rawResults)
		{
			_smoother = new SavitzkyGolaySmoother(13, 2);
			RawResults = rawResults;

			FindPpmErrorAndWidth();
		}

		private void FindPpmErrorAndWidth()
		{
			var histogramValues = Array.ConvertAll(RawResults.Values.ToArray(), input => (double)input);
			var smoothedHistogramValues = _smoother.Smooth(histogramValues);

			var indexOfMax = 0;
			double maxValue = 0;

			for (var i = 0; i < smoothedHistogramValues.Length; i++)
			{
				var value = smoothedHistogramValues[i];

				if (value > maxValue)
				{
					maxValue = value;
					indexOfMax = i;
				}
			}

			var leftIndex = indexOfMax;
			var rightIndex = indexOfMax;

			// Move left
			for (var i = indexOfMax - 1; i >= 0; i--)
			{
				var previousValue = smoothedHistogramValues[i + 1];
				var value = smoothedHistogramValues[i];

				if (value > previousValue)
				{
					leftIndex = i + 1;
					break;
				}
			}

			// Move right
			for (var i = indexOfMax + 1; i < smoothedHistogramValues.Length; i++)
			{
				var previousValue = smoothedHistogramValues[i - 1];
				var value = smoothedHistogramValues[i];

				if (value > previousValue)
				{
					rightIndex = i - 1;
					break;
				}
			}

			var ppmHistogramKeys = RawResults.Keys.ToList();

			PpmError = ppmHistogramKeys[indexOfMax];
			ErrorWidth = ppmHistogramKeys[rightIndex] - ppmHistogramKeys[leftIndex];

			SmoothedResults = new SortedDictionary<double, double>();
			for (var i = 0; i < ppmHistogramKeys.Count; i++)
			{
				SmoothedResults.Add(ppmHistogramKeys[i], smoothedHistogramValues[i]);
			}
		}
	}
}
