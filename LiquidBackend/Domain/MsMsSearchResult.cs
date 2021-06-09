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
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MsMsSearchResult) obj);
        }

        public override int GetHashCode()
        {
            return TheoreticalPeak?.GetHashCode() ?? 0;
        }
    }
}
