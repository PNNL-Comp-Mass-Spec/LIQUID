using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Scoring;
using LiquidBackend.Util;
using NUnit.Framework;

namespace LiquidTest
{
	public class ScoringTests
	{
		[Test]
		public void TestCreateScoringModel()
		{
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

			const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_v3.txt";
			FileInfo positiveTargetsFileInfo = new FileInfo(positiveTargetsFileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(positiveTargetsFileInfo);

			ScoreModelCreator liquidScoreModelCreator = new ScoreModelCreator();
			liquidScoreModelCreator.AddDmsDatasets(datasetNames);
			liquidScoreModelCreator.AddLipidTargets(lipidList);
			ScoreModel scoreModel = liquidScoreModelCreator.CreateScoreModel(30, 500);
			Console.WriteLine(scoreModel.ToString());

			// Setup workflow
			GlobalWorkflow globalWorkflow = new GlobalWorkflow(datasetNames[0]);

			// Run workflow
			List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

			foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
			{
				double score = scoreModel.ScoreLipid(lipidGroupSearchResult);

				//Console.WriteLine(score + "\t" + lipidGroupSearchResult.LipidTarget.StrippedDisplay);
			}
		}
	}
}
