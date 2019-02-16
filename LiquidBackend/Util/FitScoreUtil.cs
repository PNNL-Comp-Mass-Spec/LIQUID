using System.Collections.Generic;
using InformedProteomics.Backend.MathAndStats;

namespace LiquidBackend.Util
{
    using System.Linq;

    using InformedProteomics.Backend.Data.Biology;
    using InformedProteomics.Backend.Data.Composition;
    using InformedProteomics.Backend.Data.Spectrometry;

    using Domain;

    /// <summary>
    /// Base class for calculating the fit between a theoretical isotopic profile and observed
    /// isotopic profile using various different mathematical formulas.
    /// </summary>
    public abstract class FitUtilBase
    {
        /// <summary>
        /// Gets the fit score for the given spectrum and chemical formula.
        /// </summary>
        /// <param name="spectrumResult">Information about spectra and tolerances.</param>
        /// <param name="composition">Composition of liquid to find in the spectrum.</param>
        /// <returns>The correlation score between the theoretical spectrum and actual.</returns>
        public double GetFitScore(SpectrumSearchResult spectrumResult, Composition composition)
        {
            return GetFitScore(
                            spectrumResult.PrecursorSpectrum,
                            composition,
                            spectrumResult.PrecursorTolerance);
        }

        /// <summary>
        /// Get the fit score of Mass -1.
        /// </summary>
        /// <param name="spectrumResult">Information about spectra and tolerances.</param>
        /// <param name="composition">Composition of liquid to find in the spectrum.</param>
        /// <returns>The correlation score between the theoretical spectrum and actual.</returns>
        public double GetFitMinus1Score(SpectrumSearchResult spectrumResult, Composition composition)
        {
            var compositionMinus1 = new Composition(composition.C, composition.H - 1, composition.N, composition.O, composition.S, composition.P);
            return GetFitScore(
                                spectrumResult.PrecursorSpectrum,
                                compositionMinus1,
                                spectrumResult.PrecursorTolerance);
        }

        /// <summary>
        /// Gets the fit score for the given spectrum and chemical formula.
        /// </summary>
        /// <param name="precursorSpectrum">The spectrum to locate the ion from.</param>
        /// <param name="precursorTolerance">The tolerance to use for finding peaks.</param>
        /// <param name="composition">Composition of liquid to find in the spectrum.</param>
        /// <returns>The correlation score between the theoretical spectrum and actual.</returns>
        public double GetFitScore(Spectrum precursorSpectrum, Tolerance precursorTolerance, Composition composition)
        {
            return GetFitScore(
                            precursorSpectrum,
                            composition,
                            precursorTolerance);
        }

        /// <summary>
        /// Get the fit score of Mass -1.
        /// </summary>
        /// <param name="precursorSpectrum">The spectrum to locate the ion from.</param>
        /// <param name="precursorTolerance">The tolerance to use for finding peaks.</param>
        /// <param name="composition">Composition of liquid to find in the spectrum.</param>
        /// <returns>The correlation score between the theoretical spectrum and actual.</returns>
        public double GetFitMinus1Score(Spectrum precursorSpectrum, Tolerance precursorTolerance, Composition composition)
        {
            var compositionMinus1 = new Composition(composition.C, composition.H - 1, composition.N, composition.O, composition.S, composition.P);
            return GetFitScore(
                                precursorSpectrum,
                                compositionMinus1,
                                precursorTolerance);
        }

        /// <summary>
        /// Get the fit between a theoretical and actual isotopic profile.
        /// </summary>
        /// <param name="theoretical">The theoretical isotopic profile.</param>
        /// <param name="observed">The actual observed isotopic profile.</param>
        /// <returns>The fit score.</returns>
        protected abstract double GetFitScore(double[] theoretical, double[] observed);

        /// <summary>
        /// Finds all isotope peaks corresponding to theoretical profiles with relative intensity higher than the threshold
        /// </summary>
        /// <param name="spectrum">Observed spectrum.</param>
        /// <param name="isotopomerEnvelope">The theoretical isotopic profile.</param>
        /// <param name="mass">Monoisotopic mass of the lipid.</param>
        /// <param name="tolerance">Peak ppm tolerance.</param>
        /// <returns>array of observed isotope peaks in the spectrum. null if no peak found.</returns>
        /// <remarks>
        /// This differs from the GetAllIsotopePeaks in <see cref="LipidUtil" /> in that it accepts the isotopomer envelope
        /// as an argument rather than calculating it on its own. This way we only calculate it once.
        /// </remarks>
        private Peak[] GetAllIsotopePeaks(Spectrum spectrum, IReadOnlyCollection<double> isotopomerEnvelope, double mass, Tolerance tolerance)
        {
            var peaks = spectrum.Peaks;
            var mostAbundantIsotopeIndex = 0;
            var mostAbundantIsotopeMz = mass;
            var mostAbundantIsotopeMatchedPeakIndex = spectrum.FindPeakIndex(mostAbundantIsotopeMz, tolerance);
            if (mostAbundantIsotopeMatchedPeakIndex < 0) return null;

            var observedPeaks = new Peak[isotopomerEnvelope.Count];
            observedPeaks[mostAbundantIsotopeIndex] = peaks[mostAbundantIsotopeMatchedPeakIndex];

            // go up
            var peakIndex = mostAbundantIsotopeMatchedPeakIndex + 1;
            for (var isotopeIndex = mostAbundantIsotopeIndex + 1; isotopeIndex < isotopomerEnvelope.Count; isotopeIndex++)
            {
                var isotopeMz = mostAbundantIsotopeMz + isotopeIndex * Constants.C13MinusC12;
                var tolTh = tolerance.GetToleranceAsMz(isotopeMz);
                var minMz = isotopeMz - tolTh;
                var maxMz = isotopeMz + tolTh;
                for (var i = peakIndex; i < peaks.Length; i++)
                {
                    var peakMz = peaks[i].Mz;
                    if (peakMz > maxMz)
                    {
                        peakIndex = i;
                        break;
                    }
                    if (peakMz >= minMz)    // find match, move to prev isotope
                    {
                        var peak = peaks[i];
                        if (observedPeaks[isotopeIndex] == null ||
                            peak.Intensity > observedPeaks[isotopeIndex].Intensity)
                        {
                            observedPeaks[isotopeIndex] = peak;
                        }
                    }
                }
            }

            return observedPeaks;
        }

        /// <summary>
        /// Calculates fit score between an observed spectrum and theoretical.
        /// </summary>
        /// <param name="spectrum">Observed spectrum.</param>
        /// <param name="composition">Composition to calculate theoretical isotopic profile for.</param>
        /// <param name="tolerance">Peak ppm tolerance.</param>
        /// <param name="relativeIntensityThreshold"></param>
        /// <returns>The fit score.</returns>
        private double GetFitScore(
            Spectrum spectrum,
            Composition composition,
            Tolerance tolerance,
            double relativeIntensityThreshold = 0.1)
        {
            var theoreticalIntensities = GetTheoreticalIntensities(composition, relativeIntensityThreshold);
            var observedIntensities = GetObservedIntensities(
                                                spectrum,
                                                theoreticalIntensities,
                                                composition.Mass,
                                                tolerance);

            var fitScore = GetFitScore(theoreticalIntensities, observedIntensities);
            return fitScore;
        }

        /// <summary>
        /// Get the theoretical isotopic distribution for the given composition.
        /// </summary>
        /// <param name="composition">The composition to calculate the theoretical isotopic profile.</param>
        /// <param name="relativeIntensityThreshold">The least abundant peak to consider as a percentage of the most abundant peak.</param>
        /// <returns>The theoretical isotopic distribution.</returns>
        private double[] GetTheoreticalIntensities(Composition composition, double relativeIntensityThreshold = 0.1)
        {
            var isotopomerEnvelope = IsoProfilePredictor.GetIsotopomerEnvelop(
                                                            composition.C,
                                                            composition.H,
                                                            composition.N,
                                                            composition.O,
                                                            composition.S);

            return isotopomerEnvelope.Envelope.TakeWhile(isotope => !(isotope < relativeIntensityThreshold)).ToArray();
        }

        /// <summary>
        /// Extract the actual isotopic profile from the given spectrum.
        /// </summary>
        /// <param name="spectrum">The spectrum to extract peaks from.</param>
        /// <param name="isotopomerEnvelope">The theoretical isotopic profile.</param>
        /// <param name="mass">Monoisotopic mass of the lipid.</param>
        /// <param name="tolerance">The m/z tolerance of the isotope peaks.</param>
        /// <returns>The observed isotopic profile.</returns>
        private double[] GetObservedIntensities(Spectrum spectrum, IReadOnlyCollection<double> isotopomerEnvelope, double mass, Tolerance tolerance)
        {
            var observedPeaks = GetAllIsotopePeaks(spectrum, isotopomerEnvelope, mass, tolerance);
            if (observedPeaks == null) return null;
            var observedIntensities = new double[observedPeaks.Length];

            for (var i = 0; i < observedPeaks.Length; i++)
            {
                var observedPeak = observedPeaks[i];
                observedIntensities[i] = observedPeak != null ? (float)observedPeak.Intensity : 0.0;
            }

            return observedIntensities;
        }
    }

    /// <summary>
    /// Calculator for correlation between theoretical and observed isotopic profiles using Pearson correlation.
    /// </summary>
    public class PearsonCorrelationFitUtil : FitUtilBase
    {
        /// <summary>
        /// Get the fit between a theoretical and actual isotopic profile using Pearson correlation.
        /// </summary>
        /// <param name="theoretical">The theoretical isotopic profile.</param>
        /// <param name="observed">The actual observed isotopic profile.</param>
        /// <returns>The Pearson correlation score.</returns>
        /// <remarks>1 is the best score, 0 is the worst score.</remarks>
        protected override double GetFitScore(double[] theoretical, double[] observed)
        {
            if (theoretical == null || observed == null || theoretical.Length != observed.Length)
            {
                return 0.0;
            }

            return FitScoreCalculator.GetPearsonCorrelation(theoretical, observed);
        }
    }

    /// <summary>
    /// Calculator for correlation between theoretical and observed isotopic profiles using Cosine.
    /// </summary>
    public class CosineFitUtil : FitUtilBase
    {
        /// <summary>
        /// Get the fit between a theoretical and actual isotopic profile using Cosine between the two vectors.
        /// </summary>
        /// <param name="theoretical">The theoretical isotopic profile.</param>
        /// <param name="observed">The actual observed isotopic profile.</param>
        /// <returns>The cosine score.</returns>
        /// <remarks>1 is the best score, 0 is the worst score.</remarks>
        protected override double GetFitScore(double[] theoretical, double[] observed)
        {
            if (theoretical == null || observed == null || theoretical.Length != observed.Length)
            {
                return 0.0;
            }

            return FitScoreCalculator.GetCosine(theoretical, observed);
        }
    }

    /// <summary>
    /// Calculator for correlation between theoretical and observed isotopic profiles using DeconTools fit score.
    /// </summary>
    public class DeconToolsFitUtil : FitUtilBase
    {
        /// <summary>
        /// Get the fit between a theoretical and actual isotopic profile using DeconTools fit score.
        /// </summary>
        /// <param name="theoretical">The theoretical isotopic profile.</param>
        /// <param name="observed">The actual observed isotopic profile.</param>
        /// <returns>The DeconTools fit score.</returns>
        /// <remarks>0 is the best score, 1 is the worst score.</remarks>
        protected override double GetFitScore(double[] theoretical, double[] observed)
        {
            if (theoretical == null || observed == null || theoretical.Length != observed.Length)
            {
                return 1.0;
            }

            return FitScoreCalculator.GetDeconToolsFit(theoretical, observed);
        }
    }
}
