using System;
using System.IO;

namespace LiquidBackend.Util
{
    public static class DmsDatasetFinder
    {
        public static string FindLocationOfDataset(string datasetName)
        {
            var connectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", "gigasax", "DMS5");
            var dbUtils = PRISMDatabaseUtils.DbToolsFactory.GetDBTools(connectionString);

            var query = "SELECT Folder FROM V_Mage_Dataset_List WHERE Dataset = '" + datasetName + "'";

            var success = dbUtils.GetQueryResults(query, out var results);

            if (!success)
            {
                throw new InvalidOperationException(
                    "Query error looking for the dataset in the DMS database using view V_Mage_Dataset_List; connection string: " + connectionString);
            }

            if (results.Count == 0)
            {
                throw new InvalidOperationException(string.Format("Dataset {0} not found in the database", datasetName));
            }

            var datasetDirectoryPath = results[0][0];
            var datasetDirectory = new DirectoryInfo(datasetDirectoryPath);
            var datasetFile = datasetName + ".raw";

            var datasetFiles = datasetDirectory.GetFiles(datasetFile);

            if (datasetFiles.Length == 0)
            {
                throw new InvalidOperationException(string.Format("File {0} not found for dataset in {1}", datasetFile, datasetDirectoryPath));
            }

            return datasetDirectory.FullName;
        }
    }
}
