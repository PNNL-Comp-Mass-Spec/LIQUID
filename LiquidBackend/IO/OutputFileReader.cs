using System;
using System.Collections.Generic;

namespace LiquidBackend.IO
{
    public class OutputFileReader<T> : FileReader<T> where T : Tuple<string, int>
    {
        private const string COMMON_NAME = "COMMON NAME";
        private const string MSMS_SCAN = "MS/MS SCAN";

        /// <summary>
        /// Creates a mapping of column titles to their indices.
        /// </summary>
        /// <param name="columnString">The line containing the headers.</param>
        /// <returns>A Dictionary mapping column titles to their indices.</returns>
        protected override Dictionary<string, int> CreateColumnMapping(String columnString)
        {
            var columnMap = new Dictionary<string, int>();
            var columnTitles = columnString.Split('\t', '\n');

            for (var i = 0; i < columnTitles.Length; i++)
            {
                var columnTitle = columnTitles[i].ToUpper();

                switch (columnTitle)
                {
                    case COMMON_NAME:
                        columnMap.Add(COMMON_NAME, i);
                        break;
                    case MSMS_SCAN:
                        columnMap.Add(MSMS_SCAN, i);
                        break;
                }
            }
            return columnMap;
        }

        /// <summary>
        /// Parses a line to create a Name, Scan map.
        /// </summary>
        /// <param name="line">A line containing data representing a Lipid Identification.</param>
        /// <param name="columnMapping">The mapping of column titles to their indices.</param>
        /// <returns>A dictionary pairing common name with the scan the lipid was detected in.</returns>
        protected override T ParseLine(String line, IDictionary<string, int> columnMapping)
        {
            var columns = line.Split('\t', '\n');

            if (!columnMapping.ContainsKey(COMMON_NAME)) throw new SystemException("Common Name is required for lipid import.");
            if (!columnMapping.ContainsKey(MSMS_SCAN)) throw new SystemException("MS/MS Scan is required for lipid import.");
            var name = columns[columnMapping[COMMON_NAME]];
            var scan = Int32.Parse(columns[columnMapping[MSMS_SCAN]]);
            //var Id = new T();
            var ID = new Tuple<string, int>(name, scan);
            //Id.Add(name,scan);
            return (T)ID;
        }

    }
}
