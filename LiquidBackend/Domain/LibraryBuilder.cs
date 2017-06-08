using System.Collections.Generic;
using System.IO;
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

            var fileInfo = new FileInfo(datasetLocation);
            DatasetLocations.Add(fileInfo.FullName);
        }

        public void AddDmsDatasets(IEnumerable<string> datasetNames)
        {
            foreach (var datasetName in datasetNames)
            {
                AddDmsDataset(datasetName);
            }
        }

        public static void AddDmsDataset(string datasetName)
        {

            if (!File.Exists(datasetName))
            {
                // Lookup in DMS via Mage
                var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName.Replace(".raw",""));
                var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, datasetName);

                // Copy Locally
                File.Copy(fullPathToDmsFile, datasetName);
            }

        }
    }
}
