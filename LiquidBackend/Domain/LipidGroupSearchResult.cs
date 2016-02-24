using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Scoring;

namespace LiquidBackend.Domain
{
    using LiquidBackend.Util;

    public class LipidGroupSearchResult
	{
		public LipidTarget LipidTarget { get; private set; }
		public List<Lipid> LipidList { get; private set; }
		public SpectrumSearchResult SpectrumSearchResult { get; private set; }
		public bool ShouldExport { get; set; }
		public double Score { get; private set; }
        public double PearsonCorrScore { get; private set; }
        public double PearsonCorrScoreMinus1 { get; private set; }
        public double CosineScore { get; private set; }
        public double CosineScoreMinus1 { get; private set; }

		public LipidGroupSearchResult(LipidTarget lipidTarget, List<Lipid> lipidList, SpectrumSearchResult spectrumSearchResult)
		{
			LipidTarget = lipidTarget;
			LipidList = lipidList;
			SpectrumSearchResult = spectrumSearchResult;
			ShouldExport = false;
			Score = 0;
		}

		public LipidGroupSearchResult(LipidTarget lipidTarget, List<Lipid> lipidList, SpectrumSearchResult spectrumSearchResult, ScoreModel scoreModel)
		{
			LipidTarget = lipidTarget;
			LipidList = lipidList;
			SpectrumSearchResult = spectrumSearchResult;
			ShouldExport = false;
			Score = scoreModel.ScoreLipid(this);
            var pearsonCorrelationCalculator = new PearsonCorrelationFitUtil();
            PearsonCorrScore = pearsonCorrelationCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
            PearsonCorrScoreMinus1 = pearsonCorrelationCalculator.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);

            var cosineCalculator = new CosineFitUtil();
		    CosineScore = cosineCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
		    CosineScoreMinus1 = cosineCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
		}
	}
}
