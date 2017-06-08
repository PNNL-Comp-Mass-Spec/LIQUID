using System;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.Backend.Utils;

namespace LiquidBackend.Util
{
    /// <summary>
    /// Factory for getting Mass Spec Data in the form of .raw or .mzml files
    /// </summary>
    public class LcMsDataFactory
    {

        /// <summary>
        /// Returns InformedProteomics LcMsRun object from mass spec data types including .raw and .mzml
        /// </summary>
        /// <param name="rawFilePath"></param>
        /// <returns></returns>
        public LcMsRun GetLcMsData(string rawFilePath)
        {
            var progress = new Progress<ProgressData>();

            progress.ProgressChanged += Progress_ProgressChanged;

            var run = PbfLcMsRun.GetLcMsRun(rawFilePath, progress);

            /*
            string ext = Path.GetExtension(rawFilePath);
            switch (ext.ToLower())
            {
                case ".raw":
                    run = PbfLcMsRun.GetLcMsRun(rawFilePath, MassSpecDataType.XCaliburRun);
                    break;
                case ".mzml":
                    run = PbfLcMsRun.GetLcMsRun(rawFilePath, MassSpecDataType.MzMLFile);
                    break;
                case ".gz":
                    if (rawFilePath.ToLower().EndsWith(".mzml.gz"))
                    {
                        run = PbfLcMsRun.GetLcMsRun(rawFilePath, MassSpecDataType.MzMLFile);
                    }
                    break;
            }*/

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
