using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mage;

namespace LiquidBackend.Util
{
	public class DmsDatasetFinder
	{
		public static string FindLocationOfDataset(string datasetName)
		{
			MSSQLReader reader = new MSSQLReader();
			reader.Server = "gigasax";
			reader.Database = "DMS5";
			reader.SQLText = "SELECT * FROM V_Mage_Dataset_List WHERE Dataset = '" + datasetName + "'";

			FileListFilter fileFilter = new FileListFilter();
			fileFilter.FileColumnName = "Name";
			fileFilter.OutputColumnList = "Item|+|text, Name|+|text, Folder, Dataset, Dataset_ID, *";
			fileFilter.FileNameSelector = ".raw"; // regex style filter for file names – blank means pass all

			SimpleSink sink = new SimpleSink();

			ProcessingPipeline pipeline = ProcessingPipeline.Assemble("test_pipeline", reader, fileFilter, sink);
			pipeline.RunRoot(null);

			if (sink.Rows == null || sink.Rows.Count != 1)
			{
				throw new InvalidOperationException("Mage returned invalid results.");
			}

			int folderColumnIndex = sink.ColumnIndex["Folder"];
			string folderName = sink.Rows[0].GetValue(folderColumnIndex).ToString();

			return folderName;
		}
	}
}
