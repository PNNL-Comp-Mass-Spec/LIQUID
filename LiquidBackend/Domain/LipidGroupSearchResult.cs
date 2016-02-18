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
        public double FitScore { get; private set; }
        public double FitMinus1Score { get; private set; }

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
		    FitScore = LipidUtil.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
		    FitMinus1Score = LipidUtil.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);
		}
	}
}
