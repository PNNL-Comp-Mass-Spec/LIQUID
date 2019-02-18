using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LiquidBackend.Domain;

namespace LiquidBackend.Scoring
{
    [DataContract]
    public class ScoreModel
    {
        [DataMember]
        public List<ScoreModelUnit> ScoreModelUnitList { get; private set; }

        private ScoreModel()
        {

        }

        public ScoreModel(List<ScoreModelUnit> scoreModelUnitList)
        {
            ScoreModelUnitList = scoreModelUnitList;
            ScoreModelUnitList.Sort();
        }

        public double ScoreLipid(LipidTarget lipidTarget, SpectrumSearchResult spectrumSearchResult)
        {
            var relatedScoreModelUnits = GetRelatedScoreModelUnits(lipidTarget);

            var cidResultList = spectrumSearchResult.CidSearchResultList;
            var hcdResultList = spectrumSearchResult.HcdSearchResultList;

            var cidMaxIntensity = spectrumSearchResult.CidSpectrum != null && spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
            var hcdMaxIntensity = spectrumSearchResult.HcdSpectrum != null && spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

            double lipidScore = 0;

            if (cidMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationType(cidResultList, relatedScoreModelUnits, FragmentationType.CID, cidMaxIntensity);
            }

            if (hcdMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationType(hcdResultList, relatedScoreModelUnits, FragmentationType.HCD, hcdMaxIntensity);
            }

            return lipidScore;
        }

        public double ScoreLipid(LipidGroupSearchResult lipidGroupSearchResult)
        {
            var relatedScoreModelUnits = GetRelatedScoreModelUnits(lipidGroupSearchResult);

            var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
            var cidResultList = spectrumSearchResult.CidSearchResultList;
            var hcdResultList = spectrumSearchResult.HcdSearchResultList;

            var cidMaxIntensity = spectrumSearchResult.CidSpectrum != null && spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
            var hcdMaxIntensity = spectrumSearchResult.HcdSpectrum != null && spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

            double lipidScore = 0;

            if (cidMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationType(cidResultList, relatedScoreModelUnits, FragmentationType.CID, cidMaxIntensity);
            }

            if (hcdMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationType(hcdResultList, relatedScoreModelUnits, FragmentationType.HCD, hcdMaxIntensity);
            }

            return lipidScore;
        }

        // ReSharper disable once UnusedMember.Global
        public double ScoreLipidDissimilarity(LipidGroupSearchResult lipidGroupSearchResult)
        {
            var relatedScoreModelUnits = GetRelatedScoreModelUnits(lipidGroupSearchResult);

            var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
            var cidResultList = spectrumSearchResult.CidSearchResultList;
            var hcdResultList = spectrumSearchResult.HcdSearchResultList;

            var cidMaxIntensity = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
            var hcdMaxIntensity = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

            double lipidScore = 0;

            if (cidMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationTypeDissimilarity(cidResultList, relatedScoreModelUnits, FragmentationType.CID, cidMaxIntensity);
            }

            if (hcdMaxIntensity > 1)
            {
                // Score CID Results
                lipidScore += ScoreSingleFragmentationTypeDissimilarity(hcdResultList, relatedScoreModelUnits, FragmentationType.HCD, hcdMaxIntensity);
            }

            return lipidScore;
        }

        private List<ScoreModelUnit> GetRelatedScoreModelUnits(LipidGroupSearchResult lipidGroupSearchResult)
        {
            var lipidTarget = lipidGroupSearchResult.LipidTarget;
            return GetRelatedScoreModelUnits(lipidTarget);
        }

        private List<ScoreModelUnit> GetRelatedScoreModelUnits(LipidTarget lipidTarget)
        {
            var lipidClass = lipidTarget.LipidClass;
            var lipidType = lipidTarget.LipidType;
            var fragmentationMode = lipidTarget.FragmentationMode;

            return ScoreModelUnitList.Where(x => x.LipidClass == lipidClass && x.LipidType == lipidType && x.FragmentationMode == fragmentationMode).ToList();
        }

        private double ScoreSingleFragmentationType(
            IEnumerable<MsMsSearchResult> searchResultList,
            IReadOnlyCollection<ScoreModelUnit> relatedScoreModelUnits,
            FragmentationType fragmentationType, double maxIntensity)
        {
            double fragmentationTypeScore = 0;

            foreach (var result in searchResultList)
            {
                var fragment = result.TheoreticalPeak.Description;
                double intensity = 0;

                if (result.ObservedPeak != null)
                {
                    intensity = Math.Log10(result.ObservedPeak.Intensity) / Math.Log10(maxIntensity);
                }

                var scoreUnits = relatedScoreModelUnits.Where(x => x.FragmentationType == fragmentationType && x.FragmentDescription.Equals(fragment));
                var found = false;

                foreach (var scoreUnit in scoreUnits)
                {
                    double fragmentScore;

                    // Observed
                    if (!found && intensity <= scoreUnit.IntensityMax)
                    {
                        fragmentScore = Math.Log10(scoreUnit.Probability / scoreUnit.ProbabilityNoise);
                        found = true;
                    }
                    // Not Observed
                    else
                    {
                        fragmentScore = Math.Log10((1.0 - scoreUnit.Probability) / (1.0 - scoreUnit.ProbabilityNoise));
                    }

                    fragmentationTypeScore += fragmentScore;
                }
            }

            return fragmentationTypeScore;
        }

        private double ScoreSingleFragmentationTypeDissimilarity(
            IEnumerable<MsMsSearchResult> searchResultList,
            IReadOnlyCollection<ScoreModelUnit> relatedScoreModelUnits,
            FragmentationType fragmentationType,
            double maxIntensity)
        {
            double fragmentationTypeScore = 0;

            foreach (var result in searchResultList)
            {
                var fragment = result.TheoreticalPeak.Description;
                double intensity = 0;

                if (result.ObservedPeak != null)
                {
                    intensity = Math.Log10(result.ObservedPeak.Intensity) / Math.Log10(maxIntensity);
                }

                var scoreUnits = relatedScoreModelUnits.Where(x => x.FragmentationType == fragmentationType && x.FragmentDescription.Equals(fragment));
                var found = false;

                foreach (var scoreUnit in scoreUnits)
                {
                    double fragmentScore;

                    var inverseProbability = 1 - scoreUnit.Probability;

                    // Observed
                    if (!found && intensity <= scoreUnit.IntensityMax)
                    {
                        fragmentScore = Math.Log(inverseProbability / scoreUnit.Probability) - Math.Log(inverseProbability);
                        found = true;
                    }
                    // Not Observed
                    else
                    {
                        fragmentScore = -Math.Log(inverseProbability);
                    }

                    fragmentationTypeScore += fragmentScore;
                }
            }

            return fragmentationTypeScore;
        }

        public override string ToString()
        {
            return string.Format("ScoreModelUnitList: {0}", string.Join(";\n", ScoreModelUnitList));
        }

        public string GetTsvHeader()
        {
            return "LipidClass\tLipidType\tFragmentDescription\tFragmentationMode\tFragmentationType\tIntensityMax\tProbability\tProbabilityNoise";
        }
    }
}
