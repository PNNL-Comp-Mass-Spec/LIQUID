using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;

namespace LiquidBackend.Domain
{
	public class MsMsSearchResult
	{
		public MsMsSearchUnit TheoreticalPeak { get; private set; }
		public Peak ObservedPeak { get; private set; }

		public MsMsSearchResult(MsMsSearchUnit msMsSearchUnit, Peak observedPeak)
		{
			this.TheoreticalPeak = msMsSearchUnit;
			this.ObservedPeak = observedPeak;
		}

		protected bool Equals(MsMsSearchResult other)
		{
			return Equals(TheoreticalPeak, other.TheoreticalPeak);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MsMsSearchResult) obj);
		}

		public override int GetHashCode()
		{
			return (TheoreticalPeak != null ? TheoreticalPeak.GetHashCode() : 0);
		}
	}
}
