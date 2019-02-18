using System;
using InformedProteomics.Backend.MassSpecData;
using PRISM;

namespace LiquidBackend.Util
{
    /// <summary>
    /// Factory for getting Mass Spec Data in the form of .raw or .mzML files
    /// </summary>
    public class LcMsDataFactory
    {

        /// <summary>
        /// Returns InformedProteomics LcMsRun object from mass spec data types including .raw and .mzML
        /// </summary>
        /// <param name="rawFilePath"></param>
        /// <returns></returns>
        public LcMsRun GetLcMsData(string rawFilePath)
        {
            var progress = new Progress<ProgressData>();

            progress.ProgressChanged += Progress_ProgressChanged;

            var run = PbfLcMsRun.GetLcMsRun(rawFilePath, progress);
            return run;
        }

        #region "Events"

        private void Progress_ProgressChanged(object sender, ProgressData e)
        {
            ProgressChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Raised for each reported progress value
        /// </summary>
        public event EventHandler<ProgressData> ProgressChanged;

        #endregion

    }
}
