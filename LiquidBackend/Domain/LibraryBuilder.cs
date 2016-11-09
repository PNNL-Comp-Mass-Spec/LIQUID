using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Util;

namespace LiquidBackend.Domain
{
    public class LibraryBuilder
    {
        public HashSet<string> DatasetLocations { get; set; }


        public void AddDataset(string datasetLocation)
        {            
            if (!File.Exists(datasetLocation))
            {
                throw new FileNotFoundException("Unable to load dataset at " + datasetLocation + ". File not found.");
            }

            FileInfo fileInfo = new FileInfo(datasetLocation);
            this.DatasetLocations.Add(fileInfo.FullName);
        }

        public void AddDmsDatasets(IEnumerable<string> datasetNames)
        {
            foreach (string datasetName in datasetNames)
            {
                AddDmsDataset(datasetName);
            }
        }

        public static void AddDmsDataset(string datasetName)
        {

            if (!File.Exists(datasetName))
            {
                // Lookup in DMS via Mage
                string dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName.Replace(".raw",""));
                DirectoryInfo dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                string fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, datasetName);

                // Copy Locally
                File.Copy(fullPathToDmsFile, datasetName);
            }

        }
    }
}
