using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiDimensionalPeakFinding;

namespace LiquidBackend.Util
{
	public class QcUtil
	{
		/// <summary>
		/// Produces histogram data for a set of error values.
		/// </summary>
		/// <param name="errorValues">A group of values to calculate the histogram data for.</param>
		/// <param name="increment">The increment to use for bins.</param>
		/// <returns>A SortedDictionary mapping maximum error to the number of values with that error.</returns>
		public static SortedDictionary<double, int> CalculateHistogram(IEnumerable<double> errorValues, double maxErrorToCheck, double increment)
		{
			var hist = new SortedDictionary<double, int>();

			// Create full list with initial 0s
			for (double d = -maxErrorToCheck; d <= maxErrorToCheck; d += increment)
			{
				hist.Add(d, 0);
			}

			// Put errors into bins
			int zeroErrorCount = 0;
			foreach (var error in errorValues)
			{
				// Ignore invalid error values
				if (Math.Abs(error) > maxErrorToCheck) continue;

				// If error is exactly 0, alternate between smallest positive and negative bins
				bool zeroError = false;
				if (error == 0.0)
				{
					zeroError = true;
					++zeroErrorCount;
				}

				// Put into negative bin
				if (error < 0 || (zeroError && zeroErrorCount%2 == 0))
				{
					double curBin = -increment;
					while (error < curBin) curBin -= increment;
					++hist[curBin];
				}
					// Put into positive bin
				else if (error > 0 || (zeroError && zeroErrorCount%2 == 1))
				{
					double curBin = increment;
					while (error > curBin) curBin += increment;
					++hist[curBin];
				}
			}

			return hist;
		}
	}
}
