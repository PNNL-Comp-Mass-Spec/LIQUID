using System;
using Mage;

namespace LiquidBackend.Util
{
    public class DmsDatasetFinder
    {
        public static string FindLocationOfDataset(string datasetName)
        {
            var reader = new MSSQLReader
            {
                Server = "gigasax",
                Database = "DMS5",
                SQLText = "SELECT * FROM V_Mage_Dataset_List WHERE Dataset = '" + datasetName + "'"
            };
            var fileFilter = new FileListFilter
            {
                FileColumnName = "Name",
                OutputColumnList = "Item|+|text, Name|+|text, Folder, Dataset, Dataset_ID, *",
                FileNameSelector = ".raw" // regex style filter for file names – blank means pass all
            };
            var sink = new SimpleSink();

            var pipeline = ProcessingPipeline.Assemble("test_pipeline", reader, fileFilter, sink);
            pipeline.RunRoot(null);

            if (sink.Rows == null || sink.Rows.Count != 1)
            {
                throw new InvalidOperationException("Mage returned invalid results.");
            }

            var folderColumnIndex = sink.ColumnIndex["Folder"];
            var folderName = sink.Rows[0].GetValue(folderColumnIndex).ToString();

            return folderName;
        }
    }
}
