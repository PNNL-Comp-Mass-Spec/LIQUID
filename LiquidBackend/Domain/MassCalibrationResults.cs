using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiDimensionalPeakFinding;

namespace LiquidBackend.Domain
{
	public class MassCalibrationResults
	{
		private SavitzkyGolaySmoother _smoother;

		public SortedDictionary<double, int> RawResults { get; private set; }
		public SortedDictionary<double, double> SmoothedResults { get; private set; }
		public double PpmError { get; private set; }
		public double ErrorWidth { get; private set; }

		public MassCalibrationResults(SortedDictionary<double, int> rawResults)
		{
			_smoother = new SavitzkyGolaySmoother(13, 2);
			this.RawResults = rawResults;

			FindPpmErrorAndWidth();
		}

		private void FindPpmErrorAndWidth()
		{
			double[] histogramValues = Array.ConvertAll(this.RawResults.Values.ToArray(), input => (double)input);
			double[] smoothedHistogramValues = _smoother.Smooth(histogramValues);

			int indexOfMax = 0;
			double maxValue = 0;

			for (int i = 0; i < smoothedHistogramValues.Length; i++)
			{
				double value = smoothedHistogramValues[i];

				if (value > maxValue)
				{
					maxValue = value;
					indexOfMax = i;
				}
			}

			int leftIndex = indexOfMax;
			int rightIndex = indexOfMax;

			// Move left
			for (int i = indexOfMax - 1; i >= 0; i--)
			{
				double previousValue = smoothedHistogramValues[i + 1];
				double value = smoothedHistogramValues[i];

				if (value > previousValue)
				{
					leftIndex = i + 1;
					break;
				}
			}

			// Move right
			for (int i = indexOfMax + 1; i < smoothedHistogramValues.Length; i++)
			{
				double previousValue = smoothedHistogramValues[i - 1];
				double value = smoothedHistogramValues[i];

				if (value > previousValue)
				{
					rightIndex = i - 1;
					break;
				}
			}

			var ppmHistogramKeys = this.RawResults.Keys.ToList();

			this.PpmError = ppmHistogramKeys[indexOfMax];
			this.ErrorWidth = ppmHistogramKeys[rightIndex] - ppmHistogramKeys[leftIndex];

			this.SmoothedResults = new SortedDictionary<double, double>();
			for (int i = 0; i < ppmHistogramKeys.Count; i++)
			{
				this.SmoothedResults.Add(ppmHistogramKeys[i], smoothedHistogramValues[i]);
			}
		}
	}
}
