using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.MassSpecData;

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
		public static LcMsRun GetLcMsData(string rawFilePath)
		{
			LcMsRun run = null;
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
			}

			return run;
		}

	}
}
