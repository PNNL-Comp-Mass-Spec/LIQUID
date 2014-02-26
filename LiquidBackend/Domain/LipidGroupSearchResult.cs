using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.Domain
{
	public class LipidGroupSearchResult
	{
		public LipidTarget LipidTarget { get; private set; }
		public List<Lipid> LipidList { get; private set; }
		public SpectrumSearchResult SpectrumSearchResult { get; private set; }

		public LipidGroupSearchResult(LipidTarget lipidTarget, List<Lipid> lipidList, SpectrumSearchResult spectrumSearchResult)
		{
			LipidTarget = lipidTarget;
			LipidList = lipidList;
			SpectrumSearchResult = spectrumSearchResult;
		}
	}
}
