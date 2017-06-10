using InformedProteomics.Backend.Data.Spectrometry;

namespace LiquidBackend.Domain
{
    public class MsMsSearchResult
    {
        public MsMsSearchUnit TheoreticalPeak { get; }
        public Peak ObservedPeak { get; }

        public MsMsSearchResult(MsMsSearchUnit msMsSearchUnit, Peak observedPeak)
        {
            TheoreticalPeak = msMsSearchUnit;
            ObservedPeak = observedPeak;
        }

        protected bool Equals(MsMsSearchResult other)
        {
            return Equals(TheoreticalPeak, other.TheoreticalPeak);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MsMsSearchResult) obj);
        }

        public override int GetHashCode()
        {
            return (TheoreticalPeak != null ? TheoreticalPeak.GetHashCode() : 0);
        }
    }
}
