using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;

namespace LiquidBackend.IO
{
	public class LipidGroupSearchResultWriter
	{
		public static void AddHeaderForScoring(LipidGroupSearchResult lipidGroupSearchResult, TextWriter textWriter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Lipid,");

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

		public static void WriteToCsvForScoring(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, TextWriter textWriter)
		{
			foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
			{
				LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
				string targetName = lipidTarget.StrippedDisplay;
				SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
				List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
				List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(targetName + ",");

				foreach (var cidResult in cidResultList)
				{
					//stringBuilder.Append(cidResult.TheoreticalPeak.Description + "***");
					if (cidResult.ObservedPeak != null) stringBuilder.Append(cidResult.ObservedPeak.Intensity);
					else stringBuilder.Append("0");

					stringBuilder.Append(",");
				}

				foreach (var hcdResult in hcdResultList)
				{
					//stringBuilder.Append(hcdResult.TheoreticalPeak.Description + "***");
					if (hcdResult.ObservedPeak != null) stringBuilder.Append(hcdResult.ObservedPeak.Intensity);
					else stringBuilder.Append("0");

					stringBuilder.Append(",");
				}

				textWriter.WriteLine(stringBuilder.ToString());
			}
		}
	}
}
