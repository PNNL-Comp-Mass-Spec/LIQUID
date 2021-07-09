using InformedProteomics.Backend.Data.Spectrometry;

namespace LiquidBackend.Util
{
    internal class ScanPair
    {
        /// <summary>
        /// First scan number
        /// </summary>
        /// <remarks>
        /// <para>
        /// If both HCDScan and CIDScan are defined, this will be the HCD scan number
        /// </para>
        /// <para>
        /// If only HCDScan or only CIDScan is defined, this will hold the scan number of that scan
        /// </para>
        /// </remarks>
        public int FirstScan { get; }

        /// <summary>
        /// Second scan number
        /// </summary>
        /// <remarks>
        /// <para>
        /// If both HCDScan and CIDScan are defined, this will be the CID scan number
        /// </para>
        /// <para>
        /// If only HCDScan or only CIDScan is defined, this will be 0
        /// </para>
        /// </remarks>
        public int SecondScan { get; }

        /// <summary>
        /// CID Scan Number
        /// </summary>
        /// <remarks>
        /// 0 if not defined
        /// </remarks>
        public int CIDScan { get; }

        /// <summary>
        /// HCD Scan Number
        /// </summary>
        /// <remarks>
        /// 0 if not defined
        /// </remarks>
        public int HCDScan { get; }

        public bool HasTwoScans { get; }

        public int PrecursorScanNumber { get; set; }

        /// <summary>
        /// Constructor, for use when we don't have paired scans
        /// </summary>
        /// <param name="precursorScan"></param>
        /// <param name="scanNumber"></param>
        /// <param name="fragmentationType"></param>
        public ScanPair(int precursorScan, int scanNumber, ActivationMethod fragmentationType)
        {
            PrecursorScanNumber = precursorScan;
            if (fragmentationType == ActivationMethod.HCD)
            {
                CIDScan = 0;
                HCDScan = scanNumber;
            }
            else
            {
                CIDScan = scanNumber;
                HCDScan = 0;
            }

            FirstScan = scanNumber;
            SecondScan = 0;
            HasTwoScans = false;
        }

        /// <summary>
        /// Constructor, for use when we have a pair of fragmentation scans (both have the same m/z)
        /// </summary>
        /// <param name="precursorScan"></param>
        /// <param name="hcdScan"></param>
        /// <param name="cidScan"></param>
        public ScanPair(int precursorScan, int hcdScan, int cidScan)
        {
            PrecursorScanNumber = precursorScan;

            if (cidScan > 0 && hcdScan > 0)
            {
                CIDScan = cidScan;
                HCDScan = hcdScan;

                FirstScan = HCDScan;
                SecondScan = cidScan;
                HasTwoScans = true;
            }
            else if (hcdScan > 0)
            {
                HCDScan = hcdScan;
                FirstScan = hcdScan;
                SecondScan = 0;
                HasTwoScans = false;
            }
            else
            {
                CIDScan = cidScan;
                FirstScan = cidScan;
                SecondScan = 0;
                HasTwoScans = false;
            }
        }
    }
}
