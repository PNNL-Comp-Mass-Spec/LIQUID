using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.Util;

namespace LiquidBackend.IO
{
	public class LipidGroupSearchResultWriter
	{
		public static void AddHeaderForScoring(LipidGroupSearchResult lipidGroupSearchResult, TextWriter textWriter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Dataset,Lipid,");

			SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
			List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
			List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

			foreach (var cidResult in cidResultList)
			{
				stringBuilder.Append("CID-" + cidResult.TheoreticalPeak.Description + ",");
			}

			foreach (var hcdResult in hcdResultList)
			{
				stringBuilder.Append("HCD-" + hcdResult.TheoreticalPeak.Description + ",");
			}

			textWriter.WriteLine(stringBuilder.ToString());
		}

		public static void WriteToCsvForScoring(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, TextWriter textWriter, string datasetName)
		{
			foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
			{
				LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
				string targetName = lipidTarget.StrippedDisplay;
				SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
				List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
				List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

				double cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
				double hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(datasetName + ",");
				stringBuilder.Append(targetName + ",");

				foreach (var cidResult in cidResultList)
				{
					//stringBuilder.Append(cidResult.TheoreticalPeak.Description + "***");
					if (cidResult.ObservedPeak != null) stringBuilder.Append(Math.Log10(cidResult.ObservedPeak.Intensity) / Math.Log10(cidMaxValue));
					else stringBuilder.Append("0");

					stringBuilder.Append(",");
				}

				foreach (var hcdResult in hcdResultList)
				{
					//stringBuilder.Append(hcdResult.TheoreticalPeak.Description + "***");
					if (hcdResult.ObservedPeak != null) stringBuilder.Append(Math.Log10(hcdResult.ObservedPeak.Intensity) / Math.Log10(hcdMaxValue));
					else stringBuilder.Append("0");

					stringBuilder.Append(",");
				}

				textWriter.WriteLine(stringBuilder.ToString());
			}
		}

		public static void OutputResults(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, string fileLocation)
		{
			if(File.Exists(fileLocation)) File.Delete(fileLocation);

			using (TextWriter textWriter = new StreamWriter(fileLocation))
			{
				textWriter.WriteLine("LM_ID\tCommon Name\tAdduct\tCategory\tMain Class\tSub Class\tExact m/z\tFormula\tObserved m/z\tppm Error\tNET\tIntensity\tScore\tMS/MS Scan\tParent Scan\tPUBCHEM_SID\tPUBCHEM_CID\tINCHI_KEY\tKEGG_ID\tHMDBID\tCHEBI_ID\tLIPIDAT_ID\tLIPIDBANK_ID");

				foreach (LipidGroupSearchResult lipidGroupSearchResult in lipidGroupSearchResults)
				{
					LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
					SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;

					double targetMz = lipidTarget.MzRounded;
					var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
					var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
					double observedMz = closestPeak.Mz;
					double ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
					int msmsScan = spectrumSearchResult.HcdSpectrum != null ? spectrumSearchResult.HcdSpectrum.ScanNum : spectrumSearchResult.CidSpectrum.ScanNum;

					foreach (Lipid lipid in lipidGroupSearchResult.LipidList)
					{
						StringBuilder line = new StringBuilder();
						line.Append(lipid.LipidMapsId + "\t");
						line.Append(lipidTarget.StrippedDisplay + "\t");
						line.Append(lipid.AdductFull + "\t");
						line.Append(lipid.Category + "\t");
						line.Append(lipid.MainClass + "\t");
						line.Append(lipid.SubClass + "\t");
						line.Append(lipidTarget.MzRounded + "\t");
						line.Append(lipidTarget.EmpiricalFormula + "\t");
						line.Append(observedMz + "\t");
						line.Append(ppmError + "\t");
						line.Append(spectrumSearchResult.NormalizedElutionTime + "\t");
						line.Append(spectrumSearchResult.ApexIntensity + "\t");
						line.Append(spectrumSearchResult.Score + "\t");
						line.Append(msmsScan + "\t");
						line.Append(spectrumSearchResult.PrecursorSpectrum.ScanNum + "\t");
						line.Append(lipid.PubChemSid + "\t");
						line.Append(lipid.PubChemCid + "\t");
						line.Append(lipid.InchiKey + "\t");
						line.Append(lipid.KeggId + "\t");
						line.Append(lipid.HmdbId + "\t");
						line.Append(lipid.ChebiId + "\t");
						line.Append(lipid.LipidatId + "\t");
						line.Append(lipid.LipidBankId + "\t");

						textWriter.WriteLine(line.ToString());
					}
				}
			}
		}
	}
}
