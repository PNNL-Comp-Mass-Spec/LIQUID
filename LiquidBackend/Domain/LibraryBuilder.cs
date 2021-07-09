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

        public void AddDmsDatasets(IEnumerable<string> datasetRawFileNames)
        {
            foreach (var datasetRawFileName in datasetRawFileNames)
            {
                AddDmsDataset(datasetRawFileName);
            }
        }

        public static void AddDmsDataset(string datasetRawFileName)
        {
            if (File.Exists(datasetRawFileName))
            {
                return;
            }

            // Lookup the dataset directory in DMS
            var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetRawFileName.Replace(".raw",""));
            var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
            var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, datasetRawFileName);

            // Copy Locally
            File.Copy(fullPathToDmsFile, datasetRawFileName);
        }
    }
}
