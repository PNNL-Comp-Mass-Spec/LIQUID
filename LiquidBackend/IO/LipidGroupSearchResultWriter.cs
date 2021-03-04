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
                stringBuilder.AppendFormat("CID-{0},", cidResult.TheoreticalPeak.Description);
            }

            foreach (var hcdResult in hcdResultList)
            {
                stringBuilder.AppendFormat("HCD-{0},", hcdResult.TheoreticalPeak.Description);
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

                var cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Length > 0 ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
                var hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Length > 0 ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("{0},", datasetName);
                stringBuilder.AppendFormat("{0},", targetName);

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
                    line.AppendFormat("{0}\t", target.StrippedDisplay);
                    line.AppendFormat("{0}\t", target.EmpiricalFormula);
                    line.AppendFormat("{0}\t", target.MzRounded);
                    line.AppendFormat("{0}\t", target.FragmentationMode);
                    line.AppendFormat("{0}\t", target.AdductString);
                    line.AppendFormat("{0}\t", target.Charge);
                    line.AppendFormat("{0}\t", target.Composition.C);
                    line.AppendFormat("{0}\t", target.Composition.H);
                    line.AppendFormat("{0}\t", target.Composition.N);
                    line.AppendFormat("{0}\t", target.Composition.O);
                    line.AppendFormat("{0}", target.Composition.S);

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
                        line.AppendFormat("{0}\t", id);
                        //if (!string.IsNullOrWhiteSpace(lipid.LipidMapsId))
                        //    line.AppendFormat("{0}\t", lipid.LipidMapsId); // identifier
                        //else
                        //    line.Append("null\t");
                        line.AppendFormat("{0}\t", lipidTarget.EmpiricalFormula);   // chemical_formula
                        line.Append("null\t");                         // smiles
                        if (!string.IsNullOrWhiteSpace(lipid.InchiKey))
                            line.AppendFormat("{0}\t", lipid.InchiKey); // inchi_key
                        else
                            line.Append("null\t");
                        line.AppendFormat("{0} charge\t", lipid.SubClass + " : " + lipidTarget.FragmentationMode);                 // description
                        line.AppendFormat("{0}\t", observedMz);                     // exp_mass_to_charge
                        line.AppendFormat("{0}\t", lipidTarget.MzRounded);          // calc_mass_to_charge
                        line.Append("1\t");                 // charge
                        line.AppendFormat("{0}\t", spectrumSearchResult.RetentionTime); // retention_time
                        line.Append("null\t");                         // taxid
                        line.Append("null\t");                         // species
                        line.Append("null\t");                         // database
                        line.Append("null\t");                         // database_version
                        //line.AppendFormat("{0}\t", spectrumSearchResult.PrecursorSpectrum.ScanNum); // spectra_ref
                        line.AppendFormat("{0}\t", msmsScan); // spectra_ref
                        line.Append("[, , Liquid, ]\t");                         // search_engine
                        line.AppendFormat("{0}\t", score);                          // best_search_engine_score[1]
                        line.AppendFormat("{0}\t", score);                          // search_engine_score[1]_ms_run[1]
                        line.AppendFormat("{0}\t", lipid.AdductFull); // FROM ADDUCTFULL
                        line.AppendFormat("{0}\t", spectrumSearchResult.ApexIntensity);  // small_molecule_abundance_assay[1]
                        line.Append("null\t");                         // ^^Study_variable[1]
                        line.Append("null\t");                         // stdev_study_variable[1]
                        line.Append("null");                         // std_err_study_variable[1]

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
                        const string delim = ";;";

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
                        line.AppendFormat("{0}\t", rawFileName);
                        line.AppendFormat("{0}\t", lipid.LipidMapsId);
                        line.AppendFormat("{0}\t", lipidTarget.StrippedDisplay);
                        line.AppendFormat("{0}\t", lipid.AdductFull);
                        line.AppendFormat("{0}\t", lipid.Category);
                        line.AppendFormat("{0}\t", lipid.MainClass);
                        line.AppendFormat("{0}\t", lipid.SubClass);
                        line.AppendFormat("{0}\t", lipidTarget.MzRounded);
                        line.AppendFormat("{0}\t", lipidTarget.EmpiricalFormula);
                        line.AppendFormat("{0}\t", observedMz);
                        line.AppendFormat("{0}\t", ppmError);
                        line.AppendFormat("{0}\t", spectrumSearchResult.RetentionTime);

                        if (Precursor)
                            line.AppendFormat("{0}\t", spectrumSearchResult.PrecursorSpectrum.ElutionTime);
                        else
                            line.Append("\t");

                        line.AppendFormat("{0}\t", spectrumSearchResult.NormalizedElutionTime);
                        line.AppendFormat("{0}\t", spectrumSearchResult.ApexIntensity);
                        line.AppendFormat("{0}\t", spectrumSearchResult.PeakArea);
                        line.AppendFormat("{0}\t", score);
                        line.AppendFormat("{0}\t", msmsScan);

                        if (Precursor)
                            line.AppendFormat("{0}\t", spectrumSearchResult.PrecursorSpectrum.ScanNum);
                        else
                            line.Append("\t");

                        line.AppendFormat("{0}\t", spectrumSearchResult.ApexScanNum);
                        line.AppendFormat("{0}\t", lipid.PubChemSid);
                        line.AppendFormat("{0}\t", lipid.PubChemCid);
                        line.AppendFormat("{0}\t", lipid.InchiKey);
                        line.AppendFormat("{0}\t", lipid.KeggId);
                        line.AppendFormat("{0}\t", lipid.HmdbId);
                        line.AppendFormat("{0}\t", lipid.ChebiId);
                        line.AppendFormat("{0}\t", lipid.LipidatId);
                        line.AppendFormat("{0}\t", lipid.LipidBankId);

                        if (includeObservedAndTheoreticalPeaks)
                        {
                            line.AppendFormat("{0}\t", observedPeaks);
                            line.AppendFormat("{0}\t", theoreticalPeaks);
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
                    var query = FragmentSearchList.Aggregate("", (i, j) => i + (j.Mz + "(" + j.Description + ");"));
                    var hcdIons = result.HcdSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ");"));
                    var cidIons = result.CidSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ");"));

                    var line = new StringBuilder();
                    line.AppendFormat("{0}\t", rawFileName);
                    line.AppendFormat("{0}\t", targetAdduct);
                    line.AppendFormat("{0}\t", mz);
                    line.AppendFormat("{0}\t", apexRt);
                    line.AppendFormat("{0}\t", precursorRT);
                    line.AppendFormat("{0}\t", net);
                    line.AppendFormat("{0}\t", intensity);
                    line.AppendFormat("{0}\t", msmsScan);
                    line.AppendFormat("{0}\t", precursorScan);
                    line.AppendFormat("{0}\t", apex);
                    line.AppendFormat("{0}\t", query);
                    line.AppendFormat("{0}\t", cidIons);
                    line.AppendFormat("{0}", hcdIons);
                    textWriter.WriteLine(line.ToString());
                }
            }
        }
    }
}
