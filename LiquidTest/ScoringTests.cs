using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			ScoreModel deserializedScoreModel = ScoreModelSerialization.Deserialize(scoringModelXmlLocation);

			TextWriter textWriter = new StreamWriter("scoringModel.tsv");

			textWriter.WriteLine(deserializedScoreModel.GetTsvHeader());

			foreach (var scoreModelUnit in deserializedScoreModel.ScoreModelUnitList)
			{
				textWriter.WriteLine(scoreModelUnit.ToTsvString());
			}

			textWriter.Close();
		}


		/// <summary>
		/// Create new positive score model
		/// Files for grants redo of the scoring
		/// </summary>
		[Test]
		public void CreateNewScoringModelPositive()
		{
			List<string> datasetNames = new List<string>();

			datasetNames.Add("BTL_MeOH_a_HF_POS_112414_150mm_12Dec14_Polaroid_14-06-12");
			datasetNames.Add("BTL_MeOH_a_HF_POS_120514m_12Dec14_Polaroid_14-06-12");
			datasetNames.Add("BTL_MeOH_a_HF_POS_120914m_12Dec14_Polaroid_14-06-12");
			datasetNames.Add("BTL_MeOH_a_HF_POS_120914V2m_12Dec14_Polaroid_14-06-12");
			datasetNames.Add("FECB_ATCC_29133_616_Dark_Lipid_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_616_Light_Lipid_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_819_Dark_Lipid_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_819_Light_Lipid_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("FSFA_Isolate_HL53_0700_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL53_1000_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_1_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_2_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_3_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_1_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_2_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_3_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_1_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_2_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_3_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_1_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_2_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_3_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02");
			datasetNames.Add("LCA_Ariadna_B_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Ariadna_M_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Ariadna_T_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_conc_mix_lipid_POS_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_B_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_B_lipid_POS_rr_150mm_30Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_M_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_M_lipid_POS_rr_150mm_30Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_T_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_T_lipid_POS_rr_150mm_30Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_B_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_B_lipid_POS_rr_150mm_30Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_M_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_T_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_leaf_lipid_POS_150mm_25Mar15_Polaroid_14-12-16");
			datasetNames.Add("MinT_Kans_Gly_A_NEG_rep1_10__lip_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_NEG_rep2_11__lip_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_NEG_rep3_12__lip_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep1_01__lip_POS_150mm_2Jun15_Polaroid_HSST3");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep2_02__lip_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep3_03__lip_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("mLM_CC_T2_011-014_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_012-015_P_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_013-016_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_018-019_P_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_020-023_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_022-024_Lipid_POS_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_Sub_Test_Mito_POS_150mm_01Sept15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_1__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_2__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_3__lipid_POS_150mm_12Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OMICS_ICL102_691_0hr_Lipid_4_056_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_0hr_Lipid_5_081_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_12hr_Lipid_4_053_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_12hr_Lipid_5_079_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_18hr_Lipid_4_094_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_18hr_Lipid_5_097_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_24hr_Lipid_4_114_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_24hr_Lipid_5_093_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_3hr_Lipid_4_044_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_3hr_Lipid_5_063_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_7hr_Lipid_4_057_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_7hr_Lipid_5_002_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_0hr_Lipid_4_088_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_0hr_Lipid_5_120_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_12hr_Lipid_4_065_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_12hr_Lipid_5_110_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_18hr_Lipid_4_030_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_18hr_Lipid_5_086_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_24hr_Lipid_4_078_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_24hr_Lipid_5_109_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_3hr_Lipid_4_117_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_3hr_Lipid_5_089_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_7hr_Lipid_4_047_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_7hr_Lipid_5_119_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_0hr_Lipid_4_112_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_0hr_Lipid_5_048_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_12hr_Lipid_4_061_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_12hr_Lipid_5_050_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_18hr_Lipid_4_072_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_18hr_Lipid_5_113_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_24hr_Lipid_4_046_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_24hr_Lipid_5_008_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_3hr_Lipid_4_020_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_3hr_Lipid_5_060_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_7hr_Lipid_4_115_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_7hr_Lipid_5_038_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_0hr_Lipid_4_107_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_0hr_Lipid_5_034_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_12hr_Lipid_4_023_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_12hr_Lipid_5_052_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_18hr_Lipid_4_070_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_18hr_Lipid_5_102_POS_27Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_24hr_Lipid_4_118_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_24hr_Lipid_5_064_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_3hr_Lipid_4_091_POS_22Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_3hr_Lipid_5_074_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_7hr_Lipid_4_111_POS_17Dec14_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_7hr_Lipid_5_068_POS_6Jan15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_1_PVD_test_lipids_POS_150mm_23Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_1_PVD_test_lipids_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_2_PVD_test_lipids_POS_150mm_23Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_2_PVD_test_lipids_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_3_PVD_test_lipids_POS_150mm_23Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_3_PVD_test_lipids_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_4_PVD_test_lipids_POS_150mm_23Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_4_PVD_test_lipids_POS_150mm_24Mar15_Polaroid_14-12-16");
			datasetNames.Add("SOM_LIPIDS_1C_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_2C_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-1_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-3_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-4_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_SOF_POS_150mm_8Jun15_Polaroid_HSST3-02");
			datasetNames.Add("Sullivan_Cbaltica_5E6_test_POS_150mm_2Jun15_Polaroid_HSST3-02");
			datasetNames.Add("Sullivan_Cbaltica_5E7_test_POS_150mm_2Jun15_Polaroid_HSST3-02");


			const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_7b.txt";
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
		}

		/// <summary>
		/// Create new negative score model
		/// Files for grants redo of the scoring
		/// </summary>
		[Test]
		public void CreateNewScoringModelNegative()
		{
			List<string> datasetNames = new List<string>();

			datasetNames.Add("FECB_ATCC_29133_616_Dark_Lipid_NEG_150mm_20Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_616_Light_Lipid_NEG_150mm_20Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_819_Dark_Lipid_NEG_150mm_20Mar15_Polaroid_14-12-16");
			datasetNames.Add("FECB_ATCC_29133_819_Light_Lipid_NEG_150mm_20Mar15_Polaroid_14-12-16");
			datasetNames.Add("FSFA_Isolate_HL53_0700_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL53_1000_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_1_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_2_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0050_3_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_1_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_2_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0200_3_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_1_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_2_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_0400_3_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_1_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_2_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("FSFA_Isolate_HL91_1000_3_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_11pt5Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_12Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_13Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_14Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_15Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_16Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_17Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_18Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_19Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("Kleb_20Hr_Lipid_Neg_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("LCA_Ariadna_B_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Ariadna_M_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Ariadna_T_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Ariadna_T_lipid_NEG_150mm_31Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_B_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_B_lipid_NEG_150mm_31Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_M_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_M_lipid_NEG_150mm_31Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_T_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Dora_T_lipid_NEG_150mm_31Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_B_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_B_lipid_NEG_150mm_31Mar15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_M_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_Emma_T_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("LCA_leaf_lipid_NEG_150mm_2Apr15_Polaroid_14-12-16");
			datasetNames.Add("MinT_Kans_Gly_A_Neg_rep1_10__lip_NEG_150mm_27May15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Neg_rep2_11__lip_NEG_150mm_27May15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Neg_rep3_12__lip_NEG_150mm_27May15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep1_01__lip_NEG_150mm_27May15_Polaroid_HSST3");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep2_02__lip_NEG_150mm_27May15_Polaroid_HSST3-02");
			datasetNames.Add("MinT_Kans_Gly_A_Plus_rep3_03__lip_NEG_150mm_27May15_Polaroid_HSST3-02");
			datasetNames.Add("mLM_CC_T2_002_lipid_19Dec14_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_010_lipid_19Dec14_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_011-014_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_012-015_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_013-016_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_018-019_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_020-023_P_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_CC_T2_022-024_P_Lipid_NEG_9Jan15_Polaroid_14-12-16");
			datasetNames.Add("mLM_Sub_Test_Mito_NEG_150mm_4Sept15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6573_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6716_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6764_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6882_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6927_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6935_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO6976_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7117_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7324_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_1__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_2__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OHSUblotter_PO7355_3__lipid_NEG_150mm_17Jun15_Polaroid_HSST3-02");
			datasetNames.Add("OMICS_ICL102_691_0hr_Lipid_4_056_Neg_rr_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_0hr_Lipid_5_081_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_12hr_Lipid_4_053_Neg_rr_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_12hr_Lipid_5_079_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_18hr_Lipid_4_094_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_18hr_Lipid_5_097_Neg_rr_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_24hr_Lipid_4_114_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_24hr_Lipid_5_093_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_3hr_Lipid_4_044_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_3hr_Lipid_5_063_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_7hr_Lipid_4_057_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_691_7hr_Lipid_5_002_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_0hr_Lipid_4_088_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_0hr_Lipid_5_120_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_12hr_Lipid_4_065_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_12hr_Lipid_5_110_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_18hr_Lipid_4_030_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_18hr_Lipid_5_086_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_24hr_Lipid_4_078_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_24hr_Lipid_5_109_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_3hr_Lipid_4_117_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_3hr_Lipid_5_089_Neg_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_7hr_Lipid_4_047_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_AH1_7hr_Lipid_5_119_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_0hr_Lipid_4_112_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_0hr_Lipid_5_048_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_12hr_Lipid_4_061_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_12hr_Lipid_5_050_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_18hr_Lipid_4_072_Neg_rr_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_18hr_Lipid_5_113_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_24hr_Lipid_4_046_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_24hr_Lipid_5_008_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_3hr_Lipid_4_020_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_3hr_Lipid_5_060_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_7hr_Lipid_4_115_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_FM_7hr_Lipid_5_038_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_0hr_Lipid_4_107_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_0hr_Lipid_5_034_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_12hr_Lipid_4_023_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_12hr_Lipid_5_052_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_18hr_Lipid_4_070_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_18hr_Lipid_5_102_Neg_23Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_24hr_Lipid_4_118_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_24hr_Lipid_5_064_Neg_rr_28Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_3hr_Lipid_4_091_NEG_19Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_3hr_Lipid_5_074_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_7hr_Lipid_4_111_NEG_15Jan15_Polaroid_14-12-16");
			datasetNames.Add("OMICS_ICL102_Mock_7hr_Lipid_5_068_Neg_30Jan15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_1_PVD_test_lipids_NEG_150mm_22Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_2_PVD_test_lipids_NEG_150mm_22Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_3_PVD_test_lipids_NEG_150mm_22Mar15_Polaroid_14-12-16");
			datasetNames.Add("PVD_Test_lipid_4_PVD_test_lipids_NEG_150mm_22Mar15_Polaroid_14-12-16");
			datasetNames.Add("SOM_LIPIDS_1C_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_2C_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-1_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-3_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_Hol-4_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("SOM_LIPIDS_SOF_NEG_150mm_10Jun15_Polaroid_HSST3-02");
			datasetNames.Add("Sullivan_Cbaltica_5E6_test_NEG_150mm_28May15_Polaroid_HSST3-02");
			datasetNames.Add("Sullivan_Cbaltica_5E7_test_NEG_150mm_28May15_Polaroid_HSST3-02");


			Stopwatch x = new Stopwatch();
			x.Start();

			const string negativeTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_4.txt";
			FileInfo negativeTargetsFileInfo = new FileInfo(negativeTargetsFileLocation);
			LipidMapsDbReader<Lipid> lipidReader = new LipidMapsDbReader<Lipid>();
			List<Lipid> lipidList = lipidReader.ReadFile(negativeTargetsFileInfo);

			ScoreModelCreator liquidScoreModelCreator = new ScoreModelCreator();
			//liquidScoreModelCreator.AddDmsDatasets(new List<string> {datasetNames[0]});
			liquidScoreModelCreator.AddDmsDatasets(datasetNames);
			liquidScoreModelCreator.AddLipidTargets(lipidList);
			ScoreModel scoreModel = liquidScoreModelCreator.CreateScoreModel(30, 500);
			//Console.WriteLine(scoreModel.ToString());

			ScoreModelSerialization.Serialize(scoreModel, "scoringTestNegative_Grant.xml");

			Console.WriteLine("Time Elapsed: " + x.Elapsed);
		}

	}
}
