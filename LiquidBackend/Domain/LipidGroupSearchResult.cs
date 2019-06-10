using System.Collections.Generic;
using LiquidBackend.Scoring;

namespace LiquidBackend.Domain
{
    using Util;

    public class LipidGroupSearchResult
    {
        public LipidTarget LipidTarget { get; }
        public List<Lipid> LipidList { get; }
        public SpectrumSearchResult SpectrumSearchResult { get; }
        public bool ShouldExport { get; set; }
        public int DisplayScanNum { get; set; }
        public double DisplayMz { get; set; }
        public double Score { get; }
        public double PearsonCorrScore { get; }
        public double PearsonCorrScoreMinus1 { get; }
        public double CosineScore { get; }
        public double CosineScoreMinus1 { get; }
        public double? DisplayPercentage { get; set; }

        public LipidGroupSearchResult(LipidTarget lipidTarget, List<Lipid> lipidList, SpectrumSearchResult spectrumSearchResult)
        {
            LipidTarget = lipidTarget;
            LipidList = lipidList;
            SpectrumSearchResult = spectrumSearchResult;
            DisplayScanNum = spectrumSearchResult.HcdSpectrum.ScanNum;
            DisplayMz = spectrumSearchResult.HcdSpectrum.IsolationWindow.IsolationWindowTargetMz;
            ShouldExport = false;
            Score = 0;
        }

        public LipidGroupSearchResult(SpectrumSearchResult spectrumSearchResult, FragmentationMode fragmentationMode, Adduct adduct)
        {
            var msmsSpec = spectrumSearchResult.CidSpectrum ?? spectrumSearchResult.HcdSpectrum;

            LipidTarget = new LipidTarget(msmsSpec.IsolationWindow.ToString(),LipidClass.Unknown, fragmentationMode,null,null,adduct);
            LipidList = null;
            SpectrumSearchResult = spectrumSearchResult;
            DisplayScanNum = msmsSpec.ScanNum;
            DisplayMz = msmsSpec.IsolationWindow.IsolationWindowTargetMz;
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

            if (spectrumSearchResult.HcdSpectrum != null)
            {
                DisplayScanNum = spectrumSearchResult.HcdSpectrum.ScanNum;
                DisplayMz = spectrumSearchResult.HcdSpectrum.IsolationWindow.IsolationWindowTargetMz;
            }
            else if (spectrumSearchResult.CidSpectrum != null)
            {
                DisplayScanNum = spectrumSearchResult.CidSpectrum.ScanNum;
                DisplayMz = spectrumSearchResult.CidSpectrum.IsolationWindow.IsolationWindowTargetMz;
            }

            if (spectrumSearchResult.PrecursorSpectrum == null) return;

            var pearsonCorrelationCalculator = new PearsonCorrelationFitUtil();
            PearsonCorrScore = pearsonCorrelationCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
            PearsonCorrScoreMinus1 = pearsonCorrelationCalculator.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);

            var cosineCalculator = new CosineFitUtil();
            CosineScore = cosineCalculator.GetFitScore(spectrumSearchResult, lipidTarget.Composition);
            CosineScoreMinus1 = cosineCalculator.GetFitMinus1Score(spectrumSearchResult, lipidTarget.Composition);
        }
    }
}
