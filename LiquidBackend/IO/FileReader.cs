using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.IO
{
	/// <summary>
	/// Parses a delimited file to create a list of data objects.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	public abstract class FileReader<T> : IFileReader<T> where T : class
	{
		/// <summary>
		/// Reads in a file to produce a list of objects.
		/// </summary>
		/// <param name="fileInfo">A delimited file where the first line contains the headers.</param>
		/// <returns>A list of objects generated from the data in the file.</returns>
		public List<T> ReadFile(FileInfo fileInfo)
		{
			var list = new List<T>();

			using (TextReader textReader = new StreamReader(fileInfo.FullName))
			{
				string columnHeaders = textReader.ReadLine();
				Dictionary<string, int> columnMapping = CreateColumnMapping(columnHeaders);

				string line;
				while ((line = textReader.ReadLine()) != null)
				{
					T createdObject = ParseLine(line, columnMapping);
					if (createdObject != null) list.Add(createdObject);
				}
			}

			return list;
		}

		protected abstract Dictionary<string, int> CreateColumnMapping(String columnString);
		protected abstract T ParseLine(String line, IDictionary<string, int> columnMapping);
	}
}
