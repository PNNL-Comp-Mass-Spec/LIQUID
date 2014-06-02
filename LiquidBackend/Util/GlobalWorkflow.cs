using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Scoring;

namespace LiquidBackend.Util
{
	public class GlobalWorkflow
	{
		public LcMsRun LcMsRun { get; private set; }
		public ScoreModel ScoreModel { get; private set; }

		public GlobalWorkflow(string rawFileLocation, string scoreModelLocation = "DefaultScoringModel.xml")
		{
			this.LcMsRun = LcMsRun.GetLcMsRun(rawFileLocation, MassSpecDataType.XCaliburRun);
			this.ScoreModel = ScoreModelSerialization.Deserialize(scoreModelLocation);
		}

		public List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, double hcdMassError, double cidMassError, IProgress<int> progress = null)
		{
			return RunGlobalWorkflow(lipidList, this.LcMsRun, hcdMassError, cidMassError, this.ScoreModel, progress);
		}

		public static List<LipidGroupSearchResult> RunGlobalWorkflow(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, double cidMassError, ScoreModel scoreModel, IProgress<int> progress = null)
		{
			//TextWriter textWriter = new StreamWriter("outputNeg.tsv");
			List<LipidGroupSearchResult> lipidGroupSearchResultList = new List<LipidGroupSearchResult>();

			Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
			Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

			var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList();

			int minLcScan = lcmsRun.MinLcScan;
			double maxLcScan = lcmsRun.MaxLcScan;

			ActivationMethodCombination activationMethodCombination = FigureOutActivationMethodCombination(lcmsRun);
			if (activationMethodCombination == ActivationMethodCombination.Unsupported) throw new SystemException("Unsupported activation method.");
			bool useTwoScans = (activationMethodCombination == ActivationMethodCombination.CidThenHcd || activationMethodCombination == ActivationMethodCombination.HcdThenCid);

			for (int i = minLcScan; i <= maxLcScan; i++)
			{
				// Lookup the MS/MS Spectrum
				ProductSpectrum firstMsMsSpectrum = lcmsRun.GetSpectrum(i) as ProductSpectrum;
				if (firstMsMsSpectrum == null) continue;

				// Lookup the MS/MS Spectrum
				ProductSpectrum secondMsMsSpectrum = null;
				if (useTwoScans)
				{
					secondMsMsSpectrum = lcmsRun.GetSpectrum(i + 1) as ProductSpectrum;
					if (secondMsMsSpectrum == null) continue;
				}

				//textWriter.WriteLine(i);
				//Console.WriteLine(DateTime.Now + "\tProcessing Scan" + i);

				// Grab Precursor Spectrum
				int precursorScanNumber = lcmsRun.GetPrecursorScanNum(i);
				Spectrum precursorSpectrum = lcmsRun.GetSpectrum(precursorScanNumber);

				// Assign each MS/MS spectrum to HCD or CID
				ProductSpectrum hcdSpectrum;
				ProductSpectrum cidSpectrum;
				if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.HCD)
				{
					hcdSpectrum = firstMsMsSpectrum;
					cidSpectrum = secondMsMsSpectrum;
				}
				else
				{
					hcdSpectrum = secondMsMsSpectrum;
					cidSpectrum = firstMsMsSpectrum;
				}

				double msMsPrecursorMz = firstMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;
				double mzToSearchTolerance = hcdMassError * msMsPrecursorMz / 1000000;
				double lowMz = msMsPrecursorMz - mzToSearchTolerance;
				double highMz = msMsPrecursorMz + mzToSearchTolerance;

				foreach (var grouping in lipidsGroupedByTarget)
				{
					LipidTarget lipidTarget = grouping.Key;
					double lipidMz = lipidTarget.Composition.Mass;

					// If we reached the point where the m/z is too high, we can exit
					if (lipidMz > highMz) break;

					if (lipidMz > lowMz)
					{
						// Find the MS1 data
						Xic xic = lcmsRun.GetExtractedIonChromatogram(lipidMz, hcdTolerance, i);

						// Bogus data
						if (xic.GetApexScanNum() < 0) continue;

						// Grab the MS/MS peak to search for
						IEnumerable<MsMsSearchUnit> msMsSearchUnits = lipidTarget.GetMsMsSearchUnits();

						// Get all matching peaks
						List<MsMsSearchResult> hcdSearchResultList = hcdSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();
						List<MsMsSearchResult> cidSearchResultList = cidSpectrum != null ? (from msMsSearchUnit in msMsSearchUnits let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList() : new List<MsMsSearchResult>();

						// Create spectrum search results
						SpectrumSearchResult spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic, lcmsRun);

						LipidGroupSearchResult lipidGroupSearchResult = new LipidGroupSearchResult(lipidTarget, grouping.ToList(), spectrumSearchResult, scoreModel);
						lipidGroupSearchResultList.Add(lipidGroupSearchResult);

						//textWriter.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
						//Console.WriteLine(lipidTarget.CommonName + "\t" + spectrumSearchResult.Score);
					}
				}

				// Skip an extra scan if we look at 2 at a time
				if (useTwoScans) i++;

				// Report progress
				if (progress != null)
				{
					int currentProgress = (int)((i / maxLcScan) * 100);
					progress.Report(currentProgress);
				}
			}

			//textWriter.Close();
			return lipidGroupSearchResultList;
		}

		public void RunGlobalWorkflowSingleScan()
		{
			throw new NotImplementedException();
		}

		public MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, double hcdMassError, IProgress<int> progress = null)
		{
			return RunMassCalibration(lipidList, this.LcMsRun, hcdMassError, progress);
		}

		public static MassCalibrationResults RunMassCalibration(IEnumerable<Lipid> lipidList, LcMsRun lcmsRun, double hcdMassError, IProgress<int> progress = null)
		{
			List<double> ppmErrorList = new List<double>();

			var lipidsGroupedByTarget = lipidList.OrderBy(x => x.LipidTarget.Composition.Mass).GroupBy(x => x.LipidTarget).ToList();

			foreach (var kvp in lipidsGroupedByTarget)
			{
				LipidTarget target = kvp.Key;

				var spectrumSearchResultList = InformedWorkflow.RunInformedWorkflow(target, lcmsRun, hcdMassError, 500);

				if (spectrumSearchResultList.Any())
				{
					var targetIon = new Ion(target.Composition - Composition.Hydrogen, 1);
					double targetMz = targetIon.GetMonoIsotopicMz();

					var bestSpectrumSearchResult = spectrumSearchResultList.OrderBy(x => x.Score).First();

					var massSpectrum = bestSpectrumSearchResult.PrecursorSpectrum.Peaks;
					var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();

					double ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
					ppmErrorList.Add(ppmError);
				}
			}

			SortedDictionary<double, int> ppmHistogram = QcUtil.CalculateHistogram(ppmErrorList, hcdMassError, 0.25);

			MassCalibrationResults massCalibrationResults = new MassCalibrationResults(ppmHistogram);
			return massCalibrationResults;
		}

		private static ActivationMethodCombination FigureOutActivationMethodCombination(LcMsRun lcmsRun)
		{
			List<int> scanNumbers = lcmsRun.GetScanNumbers(1).ToList();

			// Grab an MS1 Scan thats about 33% through the file so that we get accurate MS2 data
			int indexToGrab = (int)Math.Floor(scanNumbers.Count / 3.0);
			int ms1ScanNumberInMiddleOfRun = scanNumbers[indexToGrab];

			int firstMsMsScanNumber = lcmsRun.GetNextScanNum(ms1ScanNumberInMiddleOfRun, 2);

			// Lookup the first MS/MS Spectrum
			ProductSpectrum firstMsMsSpectrum = lcmsRun.GetSpectrum(firstMsMsScanNumber) as ProductSpectrum;

			if(firstMsMsSpectrum == null) return ActivationMethodCombination.Unsupported;

			// Lookup the second MS/MS Spectrum
			int nextMsMsScanNumber = lcmsRun.GetNextScanNum(firstMsMsScanNumber, 2);
			ProductSpectrum nextMsMsSpectrum = lcmsRun.GetSpectrum(nextMsMsScanNumber) as ProductSpectrum;

			if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.HCD)
			{
				if (nextMsMsScanNumber - firstMsMsScanNumber > 1) return ActivationMethodCombination.HcdOnly;
				if (nextMsMsSpectrum == null) return ActivationMethodCombination.HcdOnly;
				if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.CID) return ActivationMethodCombination.HcdThenCid;
				if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.HCD) return ActivationMethodCombination.HcdOnly;
				
			}
			else if (firstMsMsSpectrum.ActivationMethod == ActivationMethod.CID)
			{
				if (nextMsMsScanNumber - firstMsMsScanNumber > 1) return ActivationMethodCombination.CidOnly;
				if (nextMsMsSpectrum == null) return ActivationMethodCombination.CidOnly;
				if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.HCD) return ActivationMethodCombination.CidThenHcd;
				if (nextMsMsSpectrum.ActivationMethod == ActivationMethod.CID) return ActivationMethodCombination.CidOnly;
			}

			return ActivationMethodCombination.Unsupported;
		}
	}
}
