using System;
using System.Collections.Generic;
using LiquidBackend.Domain;

namespace LiquidBackend.IO
{
    public class LipidMapsDbReader<T> : FileReader<T> where T : Lipid, new()
    {
        private const string LM_ID = "LM_ID";
        private const string COMMON_NAME = "COMMON_NAME";
        private const string ADDUCT = "ADDUCT";
        private const string CATEGORY = "CATEGORY";
        private const string MAIN_CLASS = "MAIN_CLASS";
        private const string SUB_CLASS = "SUB_CLASS";
        private const string PUBCHEM_SID = "PUBCHEM_SID";
        private const string PUBCHEM_CID = "PUBCHEM_CID";
        private const string INCHI_KEY = "INCHI_KEY";
        private const string KEGG_ID = "KEGG_ID";
        private const string HMDBID = "HMDBID";
        private const string CHEBI_ID = "CHEBI_ID";
        private const string LIPIDAT_ID = "LIPIDAT_ID";
        private const string LIPIDBANK_ID = "LIPIDBANK_ID";

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
                    case LM_ID:
                        columnMap.Add(LM_ID, i);
                        break;
                    case COMMON_NAME:
                        columnMap.Add(COMMON_NAME, i);
                        break;
                    case ADDUCT:
                        columnMap.Add(ADDUCT, i);
                        break;
                    case PUBCHEM_SID:
                        columnMap.Add(PUBCHEM_SID, i);
                        break;
                    case PUBCHEM_CID:
                        columnMap.Add(PUBCHEM_CID, i);
                        break;
                    case CATEGORY:
                        columnMap.Add(CATEGORY, i);
                        break;
                    case MAIN_CLASS:
                        columnMap.Add(MAIN_CLASS, i);
                        break;
                    case SUB_CLASS:
                        columnMap.Add(SUB_CLASS, i);
                        break;
                    case INCHI_KEY:
                        columnMap.Add(INCHI_KEY, i);
                        break;
                    case KEGG_ID:
                        columnMap.Add(KEGG_ID, i);
                        break;
                    case HMDBID:
                        columnMap.Add(HMDBID, i);
                        break;
                    case CHEBI_ID:
                        columnMap.Add(CHEBI_ID, i);
                        break;
                    case LIPIDAT_ID:
                        columnMap.Add(LIPIDAT_ID, i);
                        break;
                    case LIPIDBANK_ID:
                        columnMap.Add(LIPIDBANK_ID, i);
                        break;
                }
            }

            return columnMap;
        }

        /// <summary>
        /// Parses a line to create a LipidMapsEntry object.
        /// </summary>
        /// <param name="line">A line containing data representing a LipidMapsEntry object.</param>
        /// <param name="columnMapping">The mapping of column titles to their indices.</param>
        /// <returns>A LipidMapsEntry object.</returns>
        protected override T ParseLine(String line, IDictionary<string, int> columnMapping)
        {
            var columns = line.Split('\t', '\n');

            var lipidEntry = new T();

            if (columnMapping.ContainsKey(COMMON_NAME)) lipidEntry.CommonName = columns[columnMapping[COMMON_NAME]];
            else throw new SystemException("Common name is required for lipid import.");

            if (columnMapping.ContainsKey(ADDUCT)) lipidEntry.AdductFull = columns[columnMapping[ADDUCT]];
            else throw new SystemException("Adduct is required for lipid import.");

            // Create the lipid target
            try
            {
                lipidEntry.CreateLipidTarget();
            }
            catch (SystemException)
            {
                // If unable to create target, then just ignore this target
                return null;
            }

            if (columnMapping.ContainsKey(LM_ID)) lipidEntry.LipidMapsId = columns[columnMapping[LM_ID]];
            if (columnMapping.ContainsKey(PUBCHEM_SID)) lipidEntry.PubChemSid = columns[columnMapping[PUBCHEM_SID]];
            if (columnMapping.ContainsKey(PUBCHEM_CID)) lipidEntry.PubChemCid = columns[columnMapping[PUBCHEM_CID]];
            if (columnMapping.ContainsKey(CATEGORY)) lipidEntry.Category = columns[columnMapping[CATEGORY]];
            if (columnMapping.ContainsKey(MAIN_CLASS)) lipidEntry.MainClass = columns[columnMapping[MAIN_CLASS]];
            if (columnMapping.ContainsKey(SUB_CLASS)) lipidEntry.SubClass = columns[columnMapping[SUB_CLASS]];
            if (columnMapping.ContainsKey(INCHI_KEY)) lipidEntry.InchiKey = columns[columnMapping[INCHI_KEY]];
            if (columnMapping.ContainsKey(KEGG_ID)) lipidEntry.KeggId = columns[columnMapping[KEGG_ID]];
            if (columnMapping.ContainsKey(HMDBID)) lipidEntry.HmdbId = columns[columnMapping[HMDBID]];
            if (columnMapping.ContainsKey(CHEBI_ID))
            {
                var value = columns[columnMapping[CHEBI_ID]];
                if (!value.Equals("")) lipidEntry.ChebiId = int.Parse(value);
            }
            if (columnMapping.ContainsKey(LIPIDAT_ID))
            {
                var value = columns[columnMapping[LIPIDAT_ID]];
                if (!value.Equals("")) lipidEntry.LipidatId = int.Parse(value);
            }
            if (columnMapping.ContainsKey(LIPIDBANK_ID)) lipidEntry.LipidBankId = columns[columnMapping[LIPIDBANK_ID]];

            return lipidEntry;
        }
    }
}
