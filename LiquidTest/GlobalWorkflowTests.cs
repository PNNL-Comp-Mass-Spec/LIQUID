using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
using NUnit.Framework;

namespace LiquidTest
{
	public class GlobalWorkflowTests
	{
		[Test]
		public void TestGlobalWorkflowPositive()
		{
			string rawFileLocation = @"../../../testFiles/Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05.raw";
			GlobalWorkflow globalWorkflow = new GlobalWorkflow(rawFileLocation);

			string fileLocation = @"../../../testFiles/Global_LipidMaps_Pos.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);

			List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

			List<LipidGroupSearchResult> filteredLipidGroupSearchResults = new List<LipidGroupSearchResult>();

			// Group results of same scan together
			var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

			// Grab the result(s) with the best score
			foreach (var group in resultsGroupedByScan)
			{
				var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

				for (int i = 0; i < 1 && i < groupOrdered.Count; i++)
				{
					LipidGroupSearchResult resultToAdd = groupOrdered[i];

					if (resultToAdd.LipidTarget.LipidClass == LipidClass.PC && resultToAdd.LipidTarget.AcylChainList.Count(x => x.NumCarbons > 0) == 2 && resultToAdd.LipidTarget.AcylChainList.Count(x => x.AcylChainType == AcylChainType.Standard) == 2)
					filteredLipidGroupSearchResults.Add(resultToAdd);
				}
			}

			if (File.Exists("fragmentOutput.csv")) File.Delete("fragmentOutput.csv");
			TextWriter textWriter = new StreamWriter("fragmentOutput.csv");

			LipidGroupSearchResultWriter.AddHeaderForScoring(filteredLipidGroupSearchResults[0], textWriter);
			LipidGroupSearchResultWriter.WriteToCsvForScoring(filteredLipidGroupSearchResults, textWriter, "Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05");

			textWriter.Close();
		}

		[Test]
		public void TestGlobalWorkflowNegative()
		{
			string rawFileLocation = @"../../../testFiles/Dey_Lipids_Top_2_3_rerun_Neg_05Jul13_Gimli_12-07-05.raw";
			GlobalWorkflow globalWorkflow = new GlobalWorkflow(rawFileLocation);

			string fileLocation = @"../../../testFiles/Global_LipidMaps_Neg.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);

			globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);
		}

		[Test]
		public void TestMassCalibration()
		{
			string rawFileLocation = @"../../../testFiles/synaptosome_lipid_rafts_lipidomics_synlr_1_bottom__NEG_Polaroid_17Mar14_14-02-04.raw";
			GlobalWorkflow globalWorkflow = new GlobalWorkflow(rawFileLocation);

			string fileLocation = @"../../../testFiles/Global_LipidMaps_NEG_3.txt";
			FileInfo fileInfo = new FileInfo(fileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(fileInfo);

			MassCalibrationResults massCalibrationResults = globalWorkflow.RunMassCalibration(lipidList, 50);
			Console.WriteLine(massCalibrationResults.PpmError);
			Console.WriteLine(massCalibrationResults.ErrorWidth);
		}

		[Test]
		public void TestCreateScoringOutput()
		{
			const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_v3.txt";
			FileInfo positiveTargetsFileInfo = new FileInfo(positiveTargetsFileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(positiveTargetsFileInfo);

			if (File.Exists("fragmentOutput.csv")) File.Delete("fragmentOutput.csv");
			TextWriter textWriter = new StreamWriter("fragmentOutput.csv");

			List<string> datasetNames = new List<string>();
			//datasetNames.Add("Dey_lipids_Top_1_1_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Top_1_2_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Top_1_3_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_1_1_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_1_2_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_1_3_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Top_2_1_pos_dil_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Top_2_2_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Top_2_3_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_2_1_pos_dil_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_2_2_pos_Gimli_RZ-12-07-05");
			//datasetNames.Add("Dey_lipids_Bottom_2_3_pos_Gimli_RZ-12-07-05");
			datasetNames.Add("XGA121_lipid_Calu3_1");
			datasetNames.Add("XGA121_lipid_Calu3_2");
			datasetNames.Add("XGA121_lipid_Calu3_3");
			datasetNames.Add("XGA121_lipid_Skin_1");
			datasetNames.Add("XGA121_lipid_Skin_2");
			datasetNames.Add("XGA121_lipid_Skin_3");
			datasetNames.Add("XGA121_lipid_plasma_1");
			datasetNames.Add("XGA121_lipid_plasma_2");
			datasetNames.Add("XGA121_lipid_plasma_3");
			datasetNames.Add("Vero_01_CM_0d_4_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_2_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_3_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_1_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_4_Lipid_POS_Gimli_15Jan14_13-07-04");
			datasetNames.Add("Vero_01_MTBE_0d_3_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_2_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_1_Lipid_POS_Gimli_15Jan14_13-07-01");
			datasetNames.Add("LCA_Atta_B_gar2_b_Reruns_31May13_Gimli_12-07-01");
			datasetNames.Add("LCA_Atta_T_gar1_a1_Reruns_31May13_Gimli_12-07-01");
			datasetNames.Add("LCA_Atta_M_gar3_a_Reruns_31May13_Gimli_12-07-01");
			datasetNames.Add("Da_12_1_POS_3K_Gimli_9Oct13_13-07-01");
			datasetNames.Add("Da_24_1_POS_3K_Gimli_9Oct13_13-07-01");
			//datasetNames.Add("Lipid_QC_1_14Jan_POS_Gimli_14Jan14_13-07-01");
			//datasetNames.Add("Lipid_QC_1_14Jan_POS_Gimli_17JAN_13-07-01");
			datasetNames.Add("Daphnia_gut_TLE_POS_Gimli_21Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_01_POS_Gimli_24Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_02_POS_Gimli_24Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_03_POS_Gimli_24Jan14_13-07-01");
			datasetNames.Add("Oscar_28days_TLE__POS_04Feb14_13-07-01");
			datasetNames.Add("Oscar_21days_TLE__POS_04Feb14_13-07-01");
			datasetNames.Add("Oscar_21days_dark_TLE__POS_04Feb14_13-07-01");
			datasetNames.Add("Oscar_14day_TLE__POS_04Feb14_13-07-01");

			for (int datasetIndex = 0; datasetIndex < datasetNames.Count; datasetIndex++)
			{
				string datasetName = datasetNames[datasetIndex];
				string rawFileName = datasetName + ".raw";
				
				Console.WriteLine(DateTime.Now + ": Processing " + datasetName);

				if (File.Exists(rawFileName))
				{
					Console.WriteLine(DateTime.Now + ": Dataset already exists");
				}
				else
				{
					Console.WriteLine(DateTime.Now + ": Dataset does not exist locally, so we will go get it");

					// Lookup in DMS via Mage
					string dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
					DirectoryInfo dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
					string fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

					// Copy Locally
					// TODO: Handle files that are on MyEMSL
					Console.WriteLine(DateTime.Now + ": Copying dataset from " + dmsDirectoryInfo.FullName);
					File.Copy(fullPathToDmsFile, rawFileName);
					Console.WriteLine(DateTime.Now + ": Copy complete");
				}

				// Setup workflow
				GlobalWorkflow globalWorkflow = new GlobalWorkflow(rawFileName);

				// Run workflow
				List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

				List<LipidGroupSearchResult> filteredLipidGroupSearchResults = new List<LipidGroupSearchResult>();

				// Group results of same scan together
				var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

				// Grab the result(s) with the best score
				foreach (var group in resultsGroupedByScan)
				{
					var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

					for (int i = 0; i < 1 && i < groupOrdered.Count; i++)
					{
						LipidGroupSearchResult resultToAdd = groupOrdered[i];

						if (resultToAdd.LipidTarget.LipidClass == LipidClass.PC &&
						    resultToAdd.LipidTarget.AcylChainList.Count(x => x.NumCarbons > 0) == 2 &&
						    resultToAdd.LipidTarget.AcylChainList.Count(x => x.AcylChainType == AcylChainType.Standard) == 2)
						{
							filteredLipidGroupSearchResults.Add(resultToAdd);
						}
					}
				}

				// Output results
				if (datasetIndex == 0) LipidGroupSearchResultWriter.AddHeaderForScoring(filteredLipidGroupSearchResults[0], textWriter);
				LipidGroupSearchResultWriter.WriteToCsvForScoring(filteredLipidGroupSearchResults, textWriter, datasetName);
			}

			textWriter.Close();
		}
	}
}
