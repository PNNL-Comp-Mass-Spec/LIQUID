using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;

namespace LiquidBackend.Util
{
	public class InformedWorkflow
	{
		public LcMsRun LcMsRun { get; private set; }

		public InformedWorkflow(string rawFileLocation)
		{
			this.LcMsRun = LcMsRun.GetLcMsRun(rawFileLocation, MassSpecDataType.XCaliburRun);
		}

		public List<SpectrumSearchResult> RunInformedWorkflow(LipidTarget target, double hcdMassError, double cidMassError)
		{
			return RunInformedWorkflow(target, this.LcMsRun, hcdMassError, cidMassError);
		}

		public static List<SpectrumSearchResult> RunInformedWorkflow(LipidTarget target, LcMsRun lcmsRun, double hcdMassError, double cidMassError)
		{
			IEnumerable<MsMsSearchUnit> msMsSearchUnits = target.GetMsMsSearchUnits();

			// I have to subtract an H for the target Ion since InformedProteomics will assume protenated
			var targetIon = new Ion(target.Composition - Composition.Hydrogen, 1);
			double targetMz = targetIon.GetMonoIsotopicMz();
			Tolerance hcdTolerance = new Tolerance(hcdMassError, ToleranceUnit.Ppm);
			Tolerance cidTolerance = new Tolerance(cidMassError, ToleranceUnit.Ppm);

			// Find out which MS/MS scans have a precursor m/z that matches the target
			List<int> matchingMsMsScanNumbers = lcmsRun.GetFragmentationSpectraScanNums(targetIon).ToList();

			List<SpectrumSearchResult> spectrumSearchResultList = new List<SpectrumSearchResult>();

			for (int i = 0; i+1 < matchingMsMsScanNumbers.Count; i+=2)
			{
				int firstScanNumber = matchingMsMsScanNumbers[i];
				int secondScanNumber = matchingMsMsScanNumbers[i+1];

				// Scan numbers should be consecutive.
				if (secondScanNumber - firstScanNumber != 1)
				{
					i--;
					continue;
				}

				// Lookup the MS/MS Spectrum
				ProductSpectrum firstMsMsSpectrum = lcmsRun.GetSpectrum(firstScanNumber) as ProductSpectrum;
				if (firstMsMsSpectrum == null) continue;

				// Lookup the MS/MS Spectrum
				ProductSpectrum secondMsMsSpectrum = lcmsRun.GetSpectrum(secondScanNumber) as ProductSpectrum;
				if (secondMsMsSpectrum == null) continue;

				// Filter MS/MS Spectrum based on mass error
				double msMsPrecursorMz = firstMsMsSpectrum.IsolationWindow.IsolationWindowTargetMz;
				//if (Math.Abs(msMsPrecursorMz - targetMz) > 0.4) continue;
				double ppmError = LipidUtil.PpmError(targetMz, msMsPrecursorMz);
				if (Math.Abs(ppmError) > hcdMassError) continue;

				// No need to move on if no MS1 data is found
				if (!lcmsRun.CheckMs1Signature(targetIon, firstScanNumber, hcdTolerance)) continue;

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

				// Get all matching peaks
				List<MsMsSearchResult> hcdSearchResultList = (from msMsSearchUnit in msMsSearchUnits let peak = hcdSpectrum.FindPeak(msMsSearchUnit.Mz, hcdTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList();
				List<MsMsSearchResult> cidSearchResultList = (from msMsSearchUnit in msMsSearchUnits let peak = cidSpectrum.FindPeak(msMsSearchUnit.Mz, cidTolerance) select new MsMsSearchResult(msMsSearchUnit, peak)).ToList();

				// Find the MS1 data
				Xic xic = lcmsRun.GetExtractedIonChromatogram(targetMz, hcdTolerance, firstScanNumber);

				// Bogus data
				if (xic.GetApexScanNum() < 0) continue;

				int precursorScanNumber = lcmsRun.GetPrecursorScanNum(firstScanNumber);
				Spectrum precursorSpectrum = lcmsRun.GetSpectrum(precursorScanNumber);

				SpectrumSearchResult spectrumSearchResult = new SpectrumSearchResult(hcdSpectrum, cidSpectrum, precursorSpectrum, hcdSearchResultList, cidSearchResultList, xic);
				spectrumSearchResultList.Add(spectrumSearchResult);
			}

			return spectrumSearchResultList;
		}
	}
}
