using System.Collections.Generic;
using System.Linq;
using LiquidBackend.Domain;

namespace LiquidBackend.Util
{
    public static class SpectrumSearchResultUtil
    {
        public static IReadOnlyCollection<MsMsSearchResult> MatchingCidResults(this SpectrumSearchResult spectrumSearchResult)
        {
            return FindMatchingMsMsByFragmentationType(spectrumSearchResult, FragmentationType.CID);
        }

        public static IReadOnlyCollection<MsMsSearchResult> MatchingHcdResults(this SpectrumSearchResult spectrumSearchResult)
        {
            return FindMatchingMsMsByFragmentationType(spectrumSearchResult, FragmentationType.HCD);
        }

        private static IReadOnlyCollection<MsMsSearchResult> FindMatchingMsMsByFragmentationType(SpectrumSearchResult spectrumSearchResult, FragmentationType type)
        {
            var resultPeaks = new List<MsMsSearchResult>();

            var spectrum = type == FragmentationType.CID ?
                spectrumSearchResult.CidSpectrum :
                spectrumSearchResult.HcdSpectrum;
            if (spectrum == null || spectrum.Peaks == null)
                return resultPeaks;

            var resultsList = type == FragmentationType.CID ?
                spectrumSearchResult.CidSearchResultList :
                spectrumSearchResult.HcdSearchResultList;

            foreach (var peak in spectrum.Peaks)
            {
                var matchedPeaks = resultsList.Where(x => x.ObservedPeak?.Equals(peak) == true);

                resultPeaks.AddRange(matchedPeaks);
            }

            return resultPeaks;
        }
    }
}
