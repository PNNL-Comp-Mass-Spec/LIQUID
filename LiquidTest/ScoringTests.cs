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
		public void TestCreatePositiveScoringModel()
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

			const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_5.txt";
			FileInfo positiveTargetsFileInfo = new FileInfo(positiveTargetsFileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(positiveTargetsFileInfo);

			ScoreModelCreator liquidScoreModelCreator = new ScoreModelCreator();
			//liquidScoreModelCreator.AddDmsDatasets(new List<string> {datasetNames[0]});
			liquidScoreModelCreator.AddDmsDatasets(datasetNames);
			liquidScoreModelCreator.AddLipidTargets(lipidList);
			ScoreModel scoreModel = liquidScoreModelCreator.CreateScoreModel(30, 500);
			//Console.WriteLine(scoreModel.ToString());

			ScoreModelSerialization.Serialize(scoreModel, "scoringTestPositive.xml");
			//ScoreModel deserializedScoreModel = ScoreModelSerialization.Deserialize("scoringTestPositive.xml");

			//Console.WriteLine(deserializedScoreModel);

			//foreach (string datasetName in datasetNames)
			//{
			//	// Setup workflow
			//	GlobalWorkflow globalWorkflow = new GlobalWorkflow(datasetName);

			//	// Run workflow
			//	List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

			//	foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
			//	{
			//		double score = scoreModel.ScoreLipid(lipidGroupSearchResult);

			//		//Console.WriteLine(score + "\t" + lipidGroupSearchResult.LipidTarget.StrippedDisplay);
			//	}
			//}
		}

		[Test]
		public void TestCreateNegativeScoringModel()
		{
			List<string> datasetNames = new List<string>();
			datasetNames.Add("XGA121_lipid_Calu3_1Neg");
			datasetNames.Add("XGA121_lipid_Calu3_2Neg");
			datasetNames.Add("XGA121_lipid_Calu3_3Neg");
			datasetNames.Add("XGA121_lipid_Skin_1Neg");
			datasetNames.Add("XGA121_lipid_Skin_2Neg");
			datasetNames.Add("XGA121_lipid_Skin_3Neg");
			datasetNames.Add("XGA121_lipid_plasma_1Neg");
			datasetNames.Add("XGA121_lipid_plasma_2Neg");
			datasetNames.Add("XGA121_lipid_plasma_3Neg");
			datasetNames.Add("Vero_01_CM_0d_3_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_2_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_3_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_CM_0d_1_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_4_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_3_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_2_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("Vero_01_MTBE_0d_1_Lipid_NEG_Gimli_16Jan14_13-07-01");
			datasetNames.Add("LCA_Atta_B_gar2_b_Reruns_Neg_6Jun13_Gimli_12-07-01");
			datasetNames.Add("LCA_Atta_T_gar1_a1_Reruns_Neg_6Jun13_Gimli_12-07-01");
			datasetNames.Add("LCA_Atta_M_gar3_a_Reruns_Neg_6Jun13_Gimli_12-07-01");
			//datasetNames.Add("Lipid_QC_1_14Jan_NEG_Gimli_14Jan14_13-07-01");
			//datasetNames.Add("Lipid_QC_1_14Jan_Neg_Gimli_19Jan14_13-07-01");
			datasetNames.Add("Daphnia_gut_TLE_NEG_Gimli_21Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_01_NEG_Gimli_26Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_02_NEG_Gimli_26Jan14_13-07-01");
			datasetNames.Add("OMICS_HH_CDT_Lip_108_03_NEG_Gimli_26Jan14_13-07-01");
			datasetNames.Add("Oscar_28days_TLE_NEG_06Feb14_13-07-01");
			datasetNames.Add("Oscar_21days_TLE_NEG_06Feb14_13-07-01");
			datasetNames.Add("Oscar_21days_dark_TLE_NEG_06Feb14_13-07-01");
			datasetNames.Add("Oscar_14day_TLE_NEG_06Feb14_13-07-01");

			const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_3.txt";
			FileInfo positiveTargetsFileInfo = new FileInfo(positiveTargetsFileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(positiveTargetsFileInfo);

			ScoreModelCreator liquidScoreModelCreator = new ScoreModelCreator();
			//liquidScoreModelCreator.AddDmsDatasets(new List<string> { datasetNames[0] });
			liquidScoreModelCreator.AddDmsDatasets(datasetNames);
			liquidScoreModelCreator.AddLipidTargets(lipidList);
			ScoreModel scoreModel = liquidScoreModelCreator.CreateScoreModel(30, 500);
			//Console.WriteLine(scoreModel.ToString());

			ScoreModelSerialization.Serialize(scoreModel, "scoringTestNegative.xml");
			//ScoreModel deserializedScoreModel = ScoreModelSerialization.Deserialize("scoringTestNegative.xml");

			//Console.WriteLine(deserializedScoreModel);

			//foreach (string datasetName in datasetNames)
			//{
			//	// Setup workflow
			//	GlobalWorkflow globalWorkflow = new GlobalWorkflow(datasetName);

			//	// Run workflow
			//	List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

			//	foreach (var lipidGroupSearchResult in lipidGroupSearchResults)
			//	{
			//		double score = scoreModel.ScoreLipid(lipidGroupSearchResult);

			//		//Console.WriteLine(score + "\t" + lipidGroupSearchResult.LipidTarget.StrippedDisplay);
			//	}
			//}
		}

		[Test]
		public void TestConvertXmlToTsv()
		{
			const string scoringModelXmlLocation = @"..\..\..\..\LiquidBackend\DefaultScoringModel.xml";
			ScoreModel deserializedScoreModel = ScoreModelSerialization.Deserialize("scoringTestNegative.xml");

			TextWriter textWriter = new StreamWriter("scoringModel.tsv");

			textWriter.WriteLine(deserializedScoreModel.GetTsvHeader());

			foreach (var scoreModelUnit in deserializedScoreModel.ScoreModelUnitList)
			{
				textWriter.WriteLine(scoreModelUnit.ToTsvString());
			}

			textWriter.Close();
		}
	}
}
