using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Util;

namespace LiquidBackend.IO
{
    public class LipidGroupSearchResultWriter
    {
        public static void AddHeaderForScoring(LipidGroupSearchResult lipidGroupSearchResult, TextWriter textWriter)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Dataset,Lipid,");

            var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
            var cidResultList = spectrumSearchResult.CidSearchResultList;
            var hcdResultList = spectrumSearchResult.HcdSearchResultList;

            foreach (var cidResult in cidResultList)
            {
                stringBuilder.Append("CID-" + cidResult.TheoreticalPeak.Description + ",");
            }

            foreach (var hcdResult in hcdResultList)
            {
                stringBuilder.Append("HCD-" + hcdResult.TheoreticalPeak.Description + ",");
            }

            textWriter.WriteLine(stringBuilder.ToString());
        }

        public static void WriteToCsvForScoring(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, TextWriter textWriter, string datasetName)
        {
            foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
            {
                var lipidTarget = lipidGroupSearchResult.LipidTarget;
                var targetName = lipidTarget.StrippedDisplay;
                var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                var cidResultList = spectrumSearchResult.CidSearchResultList;
                var hcdResultList = spectrumSearchResult.HcdSearchResultList;

                var cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
                var hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(datasetName + ",");
                stringBuilder.Append(targetName + ",");

                foreach (var cidResult in cidResultList)
                {
                    //stringBuilder.Append(cidResult.TheoreticalPeak.Description + "***");
                    if (cidResult.ObservedPeak != null) stringBuilder.Append(Math.Log10(cidResult.ObservedPeak.Intensity) / Math.Log10(cidMaxValue));
                    else stringBuilder.Append("0");

                    stringBuilder.Append(",");
                }

                foreach (var hcdResult in hcdResultList)
                {
                    //stringBuilder.Append(hcdResult.TheoreticalPeak.Description + "***");
                    if (hcdResult.ObservedPeak != null) stringBuilder.Append(Math.Log10(hcdResult.ObservedPeak.Intensity) / Math.Log10(hcdMaxValue));
                    else stringBuilder.Append("0");

                    stringBuilder.Append(",");
                }

                textWriter.WriteLine(stringBuilder.ToString());
            }
        }

        public static void OutputResults(List<LipidGroupSearchResult> lipidGroupSearchResults, string fileLocation, string rawFileName, IProgress<int> progress = null, bool append = false, bool writeHeader = true,
            bool includeObservedAndTheoreticalPeaks = false)
        {
            if (File.Exists(fileLocation) && !append) File.Delete(fileLocation);

            if (Path.GetExtension(fileLocation) == ".tsv")
            {
                OutputResultsToTsv(lipidGroupSearchResults, fileLocation, rawFileName, progress, writeHeader, includeObservedAndTheoreticalPeaks);
            }
            else if (Path.GetExtension(fileLocation) == ".mzTab")
            {
                OutputResultsToMzTab(lipidGroupSearchResults, fileLocation, rawFileName, progress);
            }
            else if (Path.GetExtension(fileLocation) == ".msp")
            {
                OutputResultsToMspLibrary(lipidGroupSearchResults, fileLocation);
            }
        }

        public static void OutputTargetInfo(List<Lipid> lipidGroupSearchResults,
            string fileLocation, string rawFileName, IProgress<int> progress = null)
        {
            using (TextWriter textWriter = new StreamWriter(fileLocation))
            {
                var progressCounter = 0;
                textWriter.WriteLine("Common Name\tFormula\tm/z\tIonization\tAdduct\tCharge\tC\tH\tN\tO\tS");
                foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
                {
                    var line = new StringBuilder();
                    var target = lipidGroupSearchResult.LipidTarget;
                    line.Append(target.StrippedDisplay + "\t");
                    line.Append(target.EmpiricalFormula + "\t");
                    line.Append(target.MzRounded + "\t");
                    line.Append(target.FragmentationMode + "\t");
                    line.Append(target.AdductString + "\t");
                    line.Append(target.Charge + "\t");
                    line.Append(target.Composition.C + "\t");
                    line.Append(target.Composition.H + "\t");
                    line.Append(target.Composition.N + "\t");
                    line.Append(target.Composition.O + "\t");
                    line.Append(target.Composition.S + "\t");

                    textWriter.WriteLine(line.ToString());

                    if (progress != null)
                    {
                        progressCounter++;
                        var currentProgress = (int)(progressCounter / (double)lipidGroupSearchResults.Count * 100);
                        progress.Report(currentProgress);
                    }
                }
            }
        }

        private static void OutputResultsToMspLibrary(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, string fileLocation)
        {
            using (TextWriter textWriter = new StreamWriter(fileLocation))
            {
                foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
                {
                    var lipidTarget = lipidGroupSearchResult.LipidTarget;
                    var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                    var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
                    var targetMz = lipidTarget.MzRounded;
                    var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
                    var hcdResults = spectrumSearchResult.HcdSearchResultList;
                    var cidResults = spectrumSearchResult.CidSearchResultList;
                    var hcdSpectrum = spectrumSearchResult.HcdSpectrum;
                    var cidSpectrum = spectrumSearchResult.CidSpectrum;
                    var hcdCount = hcdSpectrum.Peaks.Length;
                    var cidCount = cidSpectrum.Peaks.Length;

                    var name = lipidTarget.StrippedDisplay;

                    var firstLipid = lipidGroupSearchResult.LipidList.FirstOrDefault();
                    string adduct;
                    if (firstLipid == null)
                        adduct = string.Empty;
                    else
                        adduct = firstLipid.AdductFull;

                    var observedMz = closestPeak.Mz;
                    var formula = lipidTarget.EmpiricalFormula;
                    var RT = spectrumSearchResult.RetentionTime;
                    double MW = 0;

                    if (lipidTarget.FragmentationMode == FragmentationMode.Positive)
                    {
                        MW = lipidTarget.Composition.Mass - LipidUtil.GetCompositionOfAdduct(lipidTarget.Adduct).Mass;
                    }
                    else if (lipidTarget.FragmentationMode == FragmentationMode.Negative)
                    {
                        MW = lipidTarget.Composition.Mass + LipidUtil.GetCompositionOfAdduct(lipidTarget.Adduct).Mass;
                    }

                    textWriter.WriteLine("Name: {0}; {1}", name, adduct);
                    textWriter.WriteLine("MW: {0}", MW);

                    // ReSharper disable StringLiteralTypo
                    textWriter.WriteLine("PRECURSORMZ: {0}", observedMz);
                    textWriter.WriteLine("RETENTIONTIME: {0}", RT);
                    // ReSharper restore StringLiteralTypo

                    textWriter.WriteLine("FORMULA: {0}", formula);
                    textWriter.WriteLine("Comment: CID");
                    textWriter.WriteLine("Num Peaks: {0}", cidCount);
                    foreach (var peak in cidSpectrum.Peaks)
                    {
                        var mz = peak.Mz;
                        var intensity = peak.Intensity;
                        var match = cidResults.Where(x => x.ObservedPeak != null).FirstOrDefault(x => x.ObservedPeak.Mz.Equals(peak.Mz));
                        if (match != null)
                        {
                            var annotation = match.TheoreticalPeak.DescriptionForUi;
                            textWriter.WriteLine("{0} {1} \"{2}\"", mz, intensity, annotation);
                        }
                        else
                        {
                            textWriter.WriteLine("{0} {1}", mz, intensity);
                        }
                    }
                    textWriter.WriteLine();

                    textWriter.WriteLine("Name: {0}; {1}", name, adduct);
                    textWriter.WriteLine("MW: {0}", MW);

                    // ReSharper disable StringLiteralTypo
                    textWriter.WriteLine("PRECURSORMZ: {0}", observedMz);
                    textWriter.WriteLine("RETENTIONTIME: {0}", RT);
                    // ReSharper restore StringLiteralTypo

                    textWriter.WriteLine("FORMULA: {0}", formula);
                    textWriter.WriteLine("Comment: HCD");
                    textWriter.WriteLine("Num Peaks: {0}", hcdCount);
                    foreach (var peak in hcdSpectrum.Peaks)
                    {
                        var mz = peak.Mz;
                        var intensity = peak.Intensity;
                        var match = hcdResults.Where(x => x.ObservedPeak != null).FirstOrDefault(x => x.ObservedPeak.Mz.Equals(peak.Mz));
                        if (match != null)
                        {
                            var annotation = match.TheoreticalPeak.DescriptionForUi;
                            textWriter.WriteLine("{0} {1} \"{2}\"", mz, intensity, annotation);
                        }
                        else
                        {
                            textWriter.WriteLine("{0} {1}", mz, intensity);
                        }
                    }
                    textWriter.WriteLine();
                }
            }
        }

        private static void OutputResultsToMzTab(IReadOnlyCollection<LipidGroupSearchResult> lipidGroupSearchResults,
            string fileLocation, string rawFileName, IProgress<int> progress = null)
        {
            using (TextWriter textWriter = new StreamWriter(fileLocation))
            {
                var progressCounter = 0;
                var mods = new List<string>();
                foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
                {
                    foreach (var lipid in lipidGroupSearchResult.LipidList)
                    {
                        if (!mods.Contains(lipid.AdductFull))
                        {
                            mods.Add(lipid.AdductFull);
                        }
                    }
                }

                // Write meta-data
                textWriter.WriteLine("MTD\tmzTabVersion\t1.0 rc5");
                textWriter.WriteLine("MTD\tmzTab-mode\tComplete");
                textWriter.WriteLine("MTD\tmzTab-type\tQuantification");
                textWriter.WriteLine("MTD\tsoftware[1]\t[, , LIQUID, ]");

                // ReSharper disable once StringLiteralTypo
                textWriter.WriteLine("MTD\tsmallmolecule_search_engine_score[1]\t[, , LIQUID_Score_Analyzer, ]");

                textWriter.WriteLine("MTD\tfixed_mod[1]\t[MS, MS:1002038, unlabeled sample, ]");
                foreach (var variableMod in mods)
                {
                    textWriter.WriteLine("MTD\tvariable_mod[1]\t[, , {0}]", variableMod);
                }
                textWriter.WriteLine("MTD\tquantification_method\t[, , LIQUID_Analysis, ]");
                textWriter.WriteLine("MTD\tsmall_molecule-quantification_unit\t[PRIDE, PRIDE:0000330, Arbitrary quantification unit, ]");

                // Get the raw/mzml location
                textWriter.WriteLine("MTD\tms_run[1]-location\t{0}", rawFileName); //TODO:
                textWriter.WriteLine("MTD\tassay[1]-quantification_reagent\t[MS, MS:1002038, unlabeled sample, ]");
                textWriter.WriteLine("MTD\tassay[1]-ms_run_ref\tms_run[1]");
                textWriter.WriteLine("MTD\tstudy_variable[1]-assay_refs\tassay[1]");
                textWriter.WriteLine("MTD\tstudy_variable[1]-description\tLIQUID Quantification");

                // ReSharper disable once StringLiteralTypo
                textWriter.WriteLine("MTD\tcolunit-small_molecule\tretention_time=[UO, UO:0000031, minute, ]");
                textWriter.WriteLine("");

                // Write small molecule section headers
                // ReSharper disable StringLiteralTypo
                textWriter.WriteLine("SMH\tidentifier\tchemical_formula\tsmiles\tinchi_key\tdescription\texp_mass_to_charge\tcalc_mass_to_charge\tcharge\tretention_time\ttaxid\tspecies\tdatabase\tdatabase_version\tspectra_ref\tsearch_engine\tbest_search_engine_score[1]\tsearch_engine_score[1]_ms_run[1]\tmodification\tsmallmolecule_abundance_assay[1]\tsmallmolecule_abundance_study_variable[1]\tsmallmolecule_abundance_stdev_study_variable[1]\tsmallmolecule_abundance_std_error_study_variable[1]");
                // ReSharper restore StringLiteralTypo

                // Write small molecule section data
                foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
                {
                    var lipidTarget = lipidGroupSearchResult.LipidTarget;
                    var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                    var targetMz = lipidTarget.MzRounded;
                    var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
                    var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
                    var observedMz = closestPeak.Mz;
                    var ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
                    var score = lipidGroupSearchResult.Score;
                    var msmsScan = spectrumSearchResult.HcdSpectrum?.ScanNum ?? spectrumSearchResult.CidSpectrum.ScanNum;

                    //TODO: var charge = calculatedFromMZ

                    foreach (var lipid in lipidGroupSearchResult.LipidList)
                    {
                        var line = new StringBuilder();
                        line.Append("SML\t");

                        //var indexToStartRemove = lipid.LipidTarget.StrippedDisplay.IndexOf("/");
                        //var id = lipid.LipidTarget.StrippedDisplay.Substring(0, indexToStartRemove);
                        //id = id.Replace("(", "");
                        var id = lipid.LipidTarget.StrippedDisplay;
                        line.Append(id + "\t");
                        //if (!string.IsNullOrWhiteSpace(lipid.LipidMapsId))
                        //    line.Append(lipid.LipidMapsId + "\t"); // identifier
                        //else
                        //    line.Append("null" + "\t");
                        line.Append(lipidTarget.EmpiricalFormula + "\t");   // chemical_formula
                        line.Append("null" + "\t");                         // smiles
                        if (!string.IsNullOrWhiteSpace(lipid.InchiKey))
                            line.Append(lipid.InchiKey + "\t"); // inchi_key
                        else
                            line.Append("null" + "\t");
                        line.Append(lipid.SubClass + " : " + lipidTarget.FragmentationMode.ToString() + " charge" + "\t");                 // description
                        line.Append(observedMz + "\t");                     // exp_mass_to_charge
                        line.Append(lipidTarget.MzRounded + "\t");          // calc_mass_to_charge
                        line.Append("1" + "\t");                 // charge
                        line.Append(spectrumSearchResult.RetentionTime + "\t"); // retention_time
                        line.Append("null" + "\t");                         // taxid
                        line.Append("null" + "\t");                         // species
                        line.Append("null" + "\t");                         // database
                        line.Append("null" + "\t");                         // database_version
                        //line.Append(spectrumSearchResult.PrecursorSpectrum.ScanNum + "\t"); // spectra_ref
                        line.Append(msmsScan + "\t"); // spectra_ref
                        line.Append("[, , Liquid, ]" + "\t");                         // search_engine
                        line.Append(score + "\t");                          // best_search_engine_score[1]
                        line.Append(score + "\t");                          // search_engine_score[1]_ms_run[1]
                        line.Append(lipid.AdductFull + "\t"); // FROM ADDUCTFULL
                        line.Append(spectrumSearchResult.ApexIntensity + "\t");  // small_molecule_abundance_assay[1]
                        line.Append("null" + "\t");                         // ^^Study_variable[1]
                        line.Append("null" + "\t");                         // stdev_study_variable[1]
                        line.Append("null" + "\t");                         // std_err_study_variable[1]

                        textWriter.WriteLine(line.ToString());
                    }
                    if (progress != null)
                    {
                        progressCounter++;
                        var currentProgress = (int)(progressCounter / (double)lipidGroupSearchResults.Count * 100);
                        progress.Report(currentProgress);
                    }
                }
            }
        }

        private static void OutputResultsToTsv(
            IReadOnlyCollection<LipidGroupSearchResult> lipidGroupSearchResults,
            string fileLocation,
            string rawFileName,
            IProgress<int> progress = null,
            bool writeHeader = true,
            bool includeObservedAndTheoreticalPeaks = false)
        {
            using (TextWriter textWriter = new StreamWriter(fileLocation, true))
            {
                if (writeHeader)
                {
                    var header = "Raw Data File\tLM_ID\tCommon Name\tAdduct\tCategory\tMain Class\tSub Class\tExact m/z\tFormula\tObserved m/z\tppm Error\tApex RT\tPrecursor RT\tApex NET\tIntensity\tPeak Area\tScore\tMS/MS Scan\tPrecursor Scan\tApex Scan\tPUBCHEM_SID\tPUBCHEM_CID\tINCHI_KEY\tKEGG_ID\tHMDBID\tCHEBI_ID\tLIPIDAT_ID\tLIPIDBANK_ID";
                    if (includeObservedAndTheoreticalPeaks)
                        header += "\tObserved MS/MS Peaks\tTheoretical MS/MS Peaks";
                    textWriter.WriteLine(header);
                }
                var progressCounter = 0;

                foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
                {
                    var lipidTarget = lipidGroupSearchResult.LipidTarget;
                    var spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                    var Precursor = spectrumSearchResult.PrecursorSpectrum != null;

                    var targetMz = lipidTarget.MzRounded;
                    var observedMz = spectrumSearchResult.HcdSpectrum?.IsolationWindow.IsolationWindowTargetMz ??
                        spectrumSearchResult.CidSpectrum.IsolationWindow.IsolationWindowTargetMz;

                    if (Precursor)
                    {
                        var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
                        var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
                        observedMz = closestPeak.Mz;
                    }

                    var score = lipidGroupSearchResult.Score;
                    var msmsScan = spectrumSearchResult.HcdSpectrum?.ScanNum ?? spectrumSearchResult.CidSpectrum.ScanNum;
                    var ppmError = LipidUtil.PpmError(targetMz, observedMz);

                    var observedPeaks = string.Empty;
                    var theoreticalPeaks = string.Empty;

                    if (includeObservedAndTheoreticalPeaks)
                    {
                        var delim = ";;";

                        var allObservedPeaks = new List<string>();
                        foreach (var cidPeak in spectrumSearchResult.MatchingCidResults())
                        {
                            allObservedPeaks.Add(string.Format("{0},{1},{2},{3}",
                                FragmentationType.CID,
                                Math.Round(cidPeak.ObservedPeak.Mz, 4),
                                Math.Round(cidPeak.ObservedPeak.Intensity, 2),
                                cidPeak.TheoreticalPeak.DescriptionForUi));
                        }

                        foreach (var hcdPeak in spectrumSearchResult.MatchingHcdResults())
                        {
                            allObservedPeaks.Add(string.Format("{0},{1},{2},{3}",
                                FragmentationType.HCD,
                                Math.Round(hcdPeak.ObservedPeak.Mz, 4),
                                Math.Round(hcdPeak.ObservedPeak.Intensity, 2),
                                hcdPeak.TheoreticalPeak.DescriptionForUi));
                        }

                        var allTheoreticalPeaks = lipidTarget.SortedMsMsSearchUnits.Select(x => string.Format("{0},{1}", Math.Round(x.Mz, 4), x.DescriptionForUi));

                        observedPeaks = string.Join(delim, allObservedPeaks);
                        theoreticalPeaks = string.Join(delim, allTheoreticalPeaks);
                    }

                    foreach (var lipid in lipidGroupSearchResult.LipidList)
                    {
                        var line = new StringBuilder();
                        line.Append(rawFileName + "\t");
                        line.Append(lipid.LipidMapsId + "\t");
                        line.Append(lipidTarget.StrippedDisplay + "\t");
                        line.Append(lipid.AdductFull + "\t");
                        line.Append(lipid.Category + "\t");
                        line.Append(lipid.MainClass + "\t");
                        line.Append(lipid.SubClass + "\t");
                        line.Append(lipidTarget.MzRounded + "\t");
                        line.Append(lipidTarget.EmpiricalFormula + "\t");
                        line.Append(observedMz + "\t");
                        line.Append(ppmError + "\t");
                        line.Append(spectrumSearchResult.RetentionTime + "\t");
                        if (Precursor) line.Append(spectrumSearchResult.PrecursorSpectrum.ElutionTime + "\t"); else line.Append("\t");
                        line.Append(spectrumSearchResult.NormalizedElutionTime + "\t");
                        line.Append(spectrumSearchResult.ApexIntensity + "\t");
                        line.Append(spectrumSearchResult.PeakArea + "\t");
                        line.Append(score + "\t");
                        line.Append(msmsScan + "\t");
                        if (Precursor) line.Append(spectrumSearchResult.PrecursorSpectrum.ScanNum + "\t"); else line.Append("\t");
                        line.Append(spectrumSearchResult.ApexScanNum + "\t");
                        line.Append(lipid.PubChemSid + "\t");
                        line.Append(lipid.PubChemCid + "\t");
                        line.Append(lipid.InchiKey + "\t");
                        line.Append(lipid.KeggId + "\t");
                        line.Append(lipid.HmdbId + "\t");
                        line.Append(lipid.ChebiId + "\t");
                        line.Append(lipid.LipidatId + "\t");
                        line.Append(lipid.LipidBankId + "\t");

                        if (includeObservedAndTheoreticalPeaks)
                        {
                            line.Append(observedPeaks + "\t");
                            line.Append(theoreticalPeaks + "\t");
                        }

                        textWriter.WriteLine(line.ToString());
                    }
                    if (progress != null)
                    {
                        progressCounter++;
                        var currentProgress = (int)(progressCounter / (double)lipidGroupSearchResults.Count * 100);
                        progress.Report(currentProgress);
                    }
                }
            }
        }

        public static void OutputFragmentInfo(List<SpectrumSearchResult> SearchResultsList, Adduct targetAdduct, ObservableCollection<MsMsSearchUnit> FragmentSearchList, LcMsRun lcmsRun, string fileLocation, string rawFileName, IProgress<int> progress, bool writeHeader = true)
        {
            using (TextWriter textWriter = new StreamWriter(fileLocation, true))
            {
                if (writeHeader)
                {
                    textWriter.WriteLine("Raw Data File\tAdduct\tObserved m/z\tApex RT\tPrecursor RT\tApex NET\tIntensity\tMS/MS Scan\tPrecursor Scan\tApex Scan\tQuery Ions\tCID Matched Ions\tHCD Matched Ions");
                }

                foreach (var result in SearchResultsList)
                {
                    var hcd = result.HcdSpectrum;
                    var cid = result.CidSpectrum;
                    var apex = result.ApexScanNum;
                    var msmsScan = hcd?.ScanNum ?? cid.ScanNum;
                    var precursorScan = lcmsRun.GetPrecursorScanNum(msmsScan);
                    var apexRt = result.RetentionTime;
                    var precursorRT = lcmsRun.GetElutionTime(precursorScan);
                    var net = result.NormalizedElutionTime;
                    var mz = hcd?.IsolationWindow.IsolationWindowTargetMz ?? cid.IsolationWindow.IsolationWindowTargetMz;

                    var intensity = result.ApexIntensity;
                    var query = FragmentSearchList.Aggregate("", (i, j) => i + (j.Mz + "(" + j.Description + ")" + ";"));
                    var hcdIons = result.HcdSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ")" + ";"));
                    var cidIons = result.CidSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ")" + ";"));

                    var line = new StringBuilder();
                    line.Append(rawFileName + "\t");
                    line.Append(targetAdduct + "\t");
                    line.Append(mz + "\t");
                    line.Append(apexRt + "\t");
                    line.Append(precursorRT + "\t");
                    line.Append(net + "\t");
                    line.Append(intensity + "\t");
                    line.Append(msmsScan + "\t");
                    line.Append(precursorScan + "\t");
                    line.Append(apex + "\t");
                    line.Append(query + "\t");
                    line.Append(cidIons + "\t");
                    line.Append(hcdIons + "\t");
                    textWriter.WriteLine(line.ToString());
                }
            }
        }
    }
}
