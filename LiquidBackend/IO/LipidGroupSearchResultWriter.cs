using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.MassSpecData;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using MathNet.Numerics.Interpolation;

namespace LiquidBackend.IO
{
	public class LipidGroupSearchResultWriter
	{
		public static void AddHeaderForScoring(LipidGroupSearchResult lipidGroupSearchResult, TextWriter textWriter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Dataset,Lipid,");

			SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
			List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
			List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

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
				LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
				string targetName = lipidTarget.StrippedDisplay;
				SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
				List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
				List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

				double cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
				double hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

				StringBuilder stringBuilder = new StringBuilder();
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

	    public static void OutputResults(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, string fileLocation, string rawFileName, IProgress<int> progress = null, bool append = false, bool writeHeader = true)
        {
            if (File.Exists(fileLocation) && !append) File.Delete(fileLocation);

	        if (Path.GetExtension(fileLocation) == ".tsv")
	        {
	            OutputResultsToTsv(lipidGroupSearchResults, fileLocation, rawFileName, progress, writeHeader);
	        }
            else if (Path.GetExtension(fileLocation) == ".mzTab")
            {
                OutputResultsToMzTab(lipidGroupSearchResults, fileLocation, rawFileName, progress);
            }
            else if (Path.GetExtension(fileLocation) == ".msp")
            {
                OutputResultsToMspLibrary(lipidGroupSearchResults, fileLocation, rawFileName, progress);
            }
	    }

	    public static void OutputTargetInfo(List<Lipid> lipidGroupSearchResults,
	        string fileLocation, string rawFileName, IProgress<int> progress = null)
	    {
	        using (TextWriter textWriter = new StreamWriter(fileLocation))
	        {
	            int progressCounter = 0;
                textWriter.WriteLine("Common Name\tFormula\tm/z\tIonization\tAdduct\tCharge\tC\tH\tN\tO\tS");
	            foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
	            {
                    StringBuilder line = new StringBuilder();
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
                        int currentProgress = (int)((progressCounter / (double)lipidGroupSearchResults.Count()) * 100);
                        progress.Report(currentProgress);
                    }
	            }

	        }
	    }

	    private static void OutputResultsToMspLibrary(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults,
	        string fileLocation, string rawFileName, IProgress<int> progress = null)
	    {
	        using (TextWriter textWriter = new StreamWriter(fileLocation))
	        {
	            int progressCounter = 0;
	            foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
	            {
                    LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
                    SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                    var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
                    var targetMz = lipidTarget.MzRounded;
                    var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
                    var hcd = spectrumSearchResult.HcdSearchResultList;
	                var cid = spectrumSearchResult.CidSearchResultList;
	                var hcdCount = hcd.Count(x => x.ObservedPeak != null);
                    var cidCount = cid.Count(x => x.ObservedPeak != null);
                    
	                var name = lipidTarget.StrippedDisplay;
	                var adduct = lipidTarget.AdductString;
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
	                

                    if (cidCount > 0)
                    {
                        textWriter.WriteLine("Name: {0}; {1}", name, adduct);
                        textWriter.WriteLine("MW: {0}", MW);
                        textWriter.WriteLine("PRECURSORMZ: {0}", observedMz);
                        textWriter.WriteLine("RETENTIONTIME: {0}", RT);
                        textWriter.WriteLine("FORMULA: {0}", formula);
                        textWriter.WriteLine("Comment: CID");
                        textWriter.WriteLine("Num Peaks: {0}", cidCount);
                        foreach (var peak in cid)
                        {
                            if (peak.ObservedPeak == null) continue;
                            var mz = peak.ObservedPeak.Mz;
                            var intensity = peak.ObservedPeak.Intensity;
                            var annotation = peak.TheoreticalPeak.DescriptionForUi;
                            textWriter.WriteLine("{0} {1} \"{2}\"", mz, intensity, annotation);
                        }
                        textWriter.WriteLine();
                    }
	                if (hcdCount > 0)
	                {
	                    textWriter.WriteLine("Name: {0}; {1}", name, adduct);
	                    textWriter.WriteLine("MW: {0}", MW);
	                    textWriter.WriteLine("PRECURSORMZ: {0}", observedMz);
	                    textWriter.WriteLine("RETENTIONTIME: {0}", RT);
	                    textWriter.WriteLine("FORMULA: {0}", formula);
	                    textWriter.WriteLine("Comment: HCD");
	                    textWriter.WriteLine("Num Peaks: {0}", hcdCount);
	                    foreach (var peak in hcd)
	                    {
                            if (peak.ObservedPeak == null) continue;
	                        var mz = peak.ObservedPeak.Mz;
	                        var intensity = peak.ObservedPeak.Intensity;
	                        var annotation = peak.TheoreticalPeak.DescriptionForUi;
	                        textWriter.WriteLine("{0} {1} \"{2}\"", mz, intensity, annotation);
	                    }
                        textWriter.WriteLine();
	                }
	            }
	        }
	    }


	    private static void OutputResultsToMzTab(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults,
	        string fileLocation, string rawFileName, IProgress<int> progress = null)
	    {
	        using (TextWriter textWriter = new StreamWriter(fileLocation))
	        {
	            int progressCounter = 0;
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

	            //Write meta-data
                textWriter.WriteLine("MTD\tmzTabVersion\t1.0 rc5");
                textWriter.WriteLine("MTD\tmzTab-mode\tComplete");
                textWriter.WriteLine("MTD\tmzTab-type\tQuantification");
                textWriter.WriteLine("MTD\tsoftware[1]\t[, , LIQUID, ]");
	            textWriter.WriteLine("MTD\tsmallmolecule_search_engine_score[1]\t[, , LIQUID_Score_Analyzer, ]");
                textWriter.WriteLine("MTD\tfixed_mod[1]\t[MS, MS:1002038, unlabeled sample, ]");
	            foreach (var variableMod in mods)
	            {
	                textWriter.WriteLine(string.Format("MTD\tvariable_mod[1]\t[, , {0}]", variableMod)); 
	            }
	            textWriter.WriteLine("MTD\tquantification_method\t[, , LIQUID_Analysis, ]");
                textWriter.WriteLine("MTD\tsmall_molecule-quantification_unit\t[PRIDE, PRIDE:0000330, Arbitrary quantification unit, ]");
                //Get the raw/mzml location
                
                textWriter.WriteLine(string.Format("MTD\tms_run[1]-location\t{0}", rawFileName)); //TODO:
	            textWriter.WriteLine("MTD\tassay[1]-quantification_reagent\t[MS, MS:1002038, unlabeled sample, ]");
	            textWriter.WriteLine("MTD\tassay[1]-ms_run_ref\tms_run[1]");
                textWriter.WriteLine("MTD\tstudy_variable[1]-assay_refs\tassay[1]");
                textWriter.WriteLine("MTD\tstudy_variable[1]-description\tLIQUID Quantification");
                textWriter.WriteLine("MTD\tcolunit-small_molecule\tretention_time=[UO, UO:0000031, minute, ]");
	            textWriter.WriteLine("");

	            //Write small molecule section headers
                textWriter.WriteLine("SMH\tidentifier\tchemical_formula\tsmiles\tinchi_key\tdescription\texp_mass_to_charge\tcalc_mass_to_charge\tcharge\tretention_time\ttaxid\tspecies\tdatabase\tdatabase_version\tspectra_ref\tsearch_engine\tbest_search_engine_score[1]\tsearch_engine_score[1]_ms_run[1]\tmodification\tsmallmolecule_abundance_assay[1]\tsmallmolecule_abundance_study_variable[1]\tsmallmolecule_abundance_stdev_study_variable[1]\tsmallmolecule_abundance_std_error_study_variable[1]");

                //Write small molecule section datas
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
	                var msmsScan = spectrumSearchResult.HcdSpectrum != null
	                    ? spectrumSearchResult.HcdSpectrum.ScanNum
	                    : spectrumSearchResult.CidSpectrum.ScanNum;

                    //TODO: var charge = calculatedFromMZ

                    foreach (Lipid lipid in lipidGroupSearchResult.LipidList)
                    {
                        StringBuilder line = new StringBuilder();
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
				        int currentProgress = (int)((progressCounter/(double)lipidGroupSearchResults.Count())*100);
                        progress.Report(currentProgress);
				    }

	            }
	        }
	    }

		private static void OutputResultsToTsv(IEnumerable<LipidGroupSearchResult> lipidGroupSearchResults, string fileLocation, string rawFileName, IProgress<int> progress = null, bool writeHeader = true)
		{

			using (TextWriter textWriter = new StreamWriter(fileLocation, true))
			{
				if (writeHeader) 
				{
                    textWriter.WriteLine("Raw Data File\tLM_ID\tCommon Name\tAdduct\tCategory\tMain Class\tSub Class\tExact m/z\tFormula\tObserved m/z\tppm Error\tApex RT\tPrecursor RT\tApex NET\tIntensity\tPeak Area\tScore\tPearson Corr Score\tPearson Corr M-1 Score\tCosine Score\tCosine M-1 Score\tMS/MS Scan\tPrecursor Scan\tApex Scan\tPUBCHEM_SID\tPUBCHEM_CID\tINCHI_KEY\tKEGG_ID\tHMDBID\tCHEBI_ID\tLIPIDAT_ID\tLIPIDBANK_ID");
			    }
				int progressCounter = 0;

				foreach (LipidGroupSearchResult lipidGroupSearchResult in lipidGroupSearchResults)
				{
					LipidTarget lipidTarget = lipidGroupSearchResult.LipidTarget;
					SpectrumSearchResult spectrumSearchResult = lipidGroupSearchResult.SpectrumSearchResult;
                    
					double targetMz = lipidTarget.MzRounded;
					var massSpectrum = spectrumSearchResult.PrecursorSpectrum.Peaks;
					var closestPeak = massSpectrum.OrderBy(x => Math.Abs(x.Mz - targetMz)).First();
					double observedMz = closestPeak.Mz;
					double ppmError = LipidUtil.PpmError(targetMz, closestPeak.Mz);
					double score = lipidGroupSearchResult.Score;
					int msmsScan = spectrumSearchResult.HcdSpectrum != null ? spectrumSearchResult.HcdSpectrum.ScanNum : spectrumSearchResult.CidSpectrum.ScanNum;

					foreach (Lipid lipid in lipidGroupSearchResult.LipidList)
					{
						StringBuilder line = new StringBuilder();
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
                        line.Append(spectrumSearchResult.PrecursorSpectrum.ElutionTime + "\t");
                        line.Append(spectrumSearchResult.NormalizedElutionTime + "\t");			    
						line.Append(spectrumSearchResult.ApexIntensity + "\t");
						line.Append(spectrumSearchResult.PeakArea + "\t");
						line.Append(score + "\t");
					    line.Append(lipidGroupSearchResult.PearsonCorrScore + "\t");
					    line.Append(lipidGroupSearchResult.PearsonCorrScoreMinus1 + "\t");
					    line.Append(lipidGroupSearchResult.CosineScore + "\t");
					    line.Append(lipidGroupSearchResult.CosineScoreMinus1 + "\t");
						line.Append(msmsScan + "\t");
						line.Append(spectrumSearchResult.PrecursorSpectrum.ScanNum + "\t");
						line.Append(spectrumSearchResult.ApexScanNum + "\t");
						line.Append(lipid.PubChemSid + "\t");
						line.Append(lipid.PubChemCid + "\t");
						line.Append(lipid.InchiKey + "\t");
						line.Append(lipid.KeggId + "\t");
						line.Append(lipid.HmdbId + "\t");
						line.Append(lipid.ChebiId + "\t");
						line.Append(lipid.LipidatId + "\t");
						line.Append(lipid.LipidBankId + "\t");

						textWriter.WriteLine(line.ToString());
					}
				    if (progress != null)
				    {
				        progressCounter++;
				        int currentProgress = (int)((progressCounter/(double)lipidGroupSearchResults.Count())*100);
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
	                var msmsScan = hcd != null ? hcd.ScanNum : cid.ScanNum;
	                var precursorScan = lcmsRun.GetPrecursorScanNum(msmsScan);
	                var apexRt = result.RetentionTime;
	                var precRT = lcmsRun.GetElutionTime(precursorScan);
	                var net = result.NormalizedElutionTime;
	                var mz = hcd == null
	                    ? cid.IsolationWindow.IsolationWindowTargetMz
	                    : hcd.IsolationWindow.IsolationWindowTargetMz;

	                var intensity = result.ApexIntensity;
                    var query = FragmentSearchList.Aggregate("",(i, j) => i + (j.Mz + "("+ j.Description + ")" + ";"));
                    var hcdIons = result.HcdSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ")" + ";"));
                    var cidIons = result.CidSearchResultList.Where(x => x.ObservedPeak != null).Aggregate("", (current, temp) => current + (temp.ObservedPeak.Mz + "(" + temp.TheoreticalPeak.Description + ")" + ";"));

	               
                    StringBuilder line = new StringBuilder();
	                line.Append(rawFileName + "\t");
                    line.Append(targetAdduct + "\t");
                    line.Append(mz + "\t");
                    line.Append(apexRt + "\t");
                    line.Append(precRT + "\t");
                    line.Append(net + "\t");
                    line.Append(intensity + "\t");
                    line.Append(msmsScan + "\t");
                    line.Append(precursorScan + "\t");
                    line.Append(apex + "\t");
                    line.Append(query + "\t");
                    line.Append(hcdIons + "\t");
                    line.Append(cidIons + "\t");
	                textWriter.WriteLine(line.ToString());
	            }
	        }

	    }
	}
}
