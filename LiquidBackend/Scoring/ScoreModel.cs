using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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
			this.ScoreModelUnitList.Sort();
		}

		public double ScoreLipid(LipidGroupSearchResult lipidGroupSearchResult)
		{
			List<ScoreModelUnit> relatedScoreModelUnits = GetRelatedScoreModelUnits(lipidGroupSearchResult);

			SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
			List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
			List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

			double cidMaxIntensity = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
			double hcdMaxIntensity = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

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

		private List<ScoreModelUnit> GetRelatedScoreModelUnits(LipidGroupSearchResult lipidGroupSearchResult)
		{
			LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
			LipidClass lipidClass = lipidTarget.LipidClass;
			LipidType lipidType = lipidTarget.LipidType;
			FragmentationMode fragmentationMode = lipidTarget.FragmentationMode;

			return this.ScoreModelUnitList.Where(x => x.LipidClass == lipidClass && x.LipidType == lipidType && x.FragmentationMode == fragmentationMode).ToList();
		}

		private double ScoreSingleFragmentationType(IEnumerable<MsMsSearchResult> searchResultList, IEnumerable<ScoreModelUnit> relatedScoreModelUnits, FragmentationType fragmentationType, double maxIntensity)
		{
			double fragmentationTypeScore = 0;

			foreach (var result in searchResultList)
			{
				string fragment = result.TheoreticalPeak.Description;
				double intensity = 0;

				if (result.ObservedPeak != null)
				{
					intensity = Math.Log10(result.ObservedPeak.Intensity) / Math.Log10(maxIntensity);
				}

				var scoreUnits = relatedScoreModelUnits.Where(x => x.FragmentationType == fragmentationType && x.FragmentDescription.Equals(fragment));
				bool found = false;

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

		public override string ToString()
		{
			return string.Format("ScoreModelUnitList: {0}", string.Join(";\n", ScoreModelUnitList));
		}
	}
}
