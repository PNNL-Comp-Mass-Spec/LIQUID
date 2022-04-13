using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using LiquidBackend.Util;
using NUnit.Framework;

namespace LiquidTest
{
    public class FdrUnitTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            LipidRules.LoadLipidRules("DefaultCompositionRules.txt", "DefaultFragmentationRules.txt");
        }

        /// <summary>
        /// Run the files for verification of new scoring model
        /// Positive LIQUID Targets
        /// </summary>
        [Test]
        public void PositiveTrueValidation()
        {
            var datasetNamesPositive = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_Late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_early_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_late_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_POS_150mm_2Jun15_Polaroid_HSST3-10",
                "SOM_LIPIDS_3C_POS_150mm_8Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep1_10__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep2_11__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep3_12__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_POS_150mm_2Jun15_Polaroid_HSST3",
                "MinT_Kans_Gly_A_Plus_rep2_02__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep3_03__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0100_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02"
            };
            const string positiveTargetsFileLocation = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            RunWorkflowAndOutput(positiveTargetsFileLocation, "PositiveTrueTargets.tsv", datasetNamesPositive);
        }

        /// <summary>
        /// Run the files for verification of new scoring model
        /// Positive LIQUID Targets
        /// </summary>
        [Test]
        public void NegativeTrueValidation()
        {
            var datasetNamesNegative = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_early_pooled_rr_NEG_150mm_5Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_neg_150mm_17Apr15_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_NEG_150mm_10Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep1_10__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep2_11__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep3_12__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_NEG_150mm_27May15_Polaroid_HSST3",
                "MinT_Kans_Gly_A_Plus_rep2_02__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep3_03__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0100_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02"
            };
            const string negativeTargetsFileLocation = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            RunWorkflowAndOutput(negativeTargetsFileLocation, "NegativeTrueTargets.tsv", datasetNamesNegative);
        }

        /// <summary>
        /// Run the files for verification of new scoring model
        /// Positive LIQUID Targets
        /// </summary>
        [Test]
        public void PositiveDecoyValidation()
        {
            var datasetNamesPositive = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_Late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_early_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_late_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_17Apr15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_POS_150mm_2Jun15_Polaroid_HSST3-10",
                "SOM_LIPIDS_3C_POS_150mm_8Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep1_10__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep2_11__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep3_12__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_POS_150mm_2Jun15_Polaroid_HSST3",
                "MinT_Kans_Gly_A_Plus_rep2_02__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep3_03__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0100_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02"
            };
            const string positiveDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_POS_7b_Decoys.txt";
            RunWorkflowAndOutput(positiveDecoyTargetsFileLocation, "PositiveDecoyTargets.tsv", datasetNamesPositive);
        }

        /// <summary>
        /// Run the files for verification of new scoring model
        /// Positive LIQUID Targets
        /// </summary>
        [Test]
        public void NegativeDecoyValidation()
        {
            var datasetNamesNegative = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_early_pooled_rr_NEG_150mm_5Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_1d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_neg_150mm_17Apr15_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_2d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_4d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_7d_Lipid_pooled_NEG_150mm_23Apr15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_NEG_150mm_10Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep1_10__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep2_11__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Neg_rep3_12__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_NEG_150mm_27May15_Polaroid_HSST3",
                "MinT_Kans_Gly_A_Plus_rep2_02__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep3_03__lip_NEG_150mm_27May15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0100_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02"
            };
            const string negativeDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_NEG_4_Decoys.txt";
            RunWorkflowAndOutput(negativeDecoyTargetsFileLocation, "NegativeDecoyTargets.tsv", datasetNamesNegative);
        }

        /// <summary>
        /// Main functionality for running the LIQUID workflow and outputting the results
        /// </summary>
        /// <param name="targetsFilePath"></param>
        /// <param name="outputFileName"></param>
        /// <param name="datasetNamesList">Dataset names</param>
        private void RunWorkflowAndOutput(string targetsFilePath, string outputFileName, IEnumerable<string> datasetNamesList)
        {
            var targetsFileInfo = new FileInfo(targetsFilePath);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(targetsFileInfo);
            var headerWritten = false;

            foreach (var datasetName in datasetNamesList)
            {
                var rawFileName = datasetName + ".raw";

                var rawFilePath = Path.Combine(@"D:\Data\Liquid\Original", rawFileName);

                Console.WriteLine(DateTime.Now + ": Processing " + datasetName);

                if (File.Exists(rawFilePath))
                {
                    Console.WriteLine(DateTime.Now + ": Dataset already exists");
                }
                else
                {
                    Console.WriteLine(DateTime.Now + ": Dataset does not exist locally, so we will go get it");

                    // Lookup the dataset directory in DMS
                    var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
                    var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                    var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

                    // Copy Locally
                    // TODO: Handle files that are on MyEMSL
                    Console.WriteLine(DateTime.Now + ": Copying dataset from " + dmsDirectoryInfo.FullName);
                    File.Copy(fullPathToDmsFile, rawFilePath);
                    Console.WriteLine(DateTime.Now + ": Copy complete");
                }

                // Setup workflow
                var globalWorkflow = new GlobalWorkflow(rawFilePath);

                // Run workflow
                var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);

                if (!headerWritten)
                {
                    LipidGroupSearchResultWriter.OutputResults(lipidGroupSearchResults, outputFileName, rawFileName, null, true, true);
                    headerWritten = true;
                }
                else
                {
                    LipidGroupSearchResultWriter.OutputResults(lipidGroupSearchResults, outputFileName, rawFileName, null, true, false);
                }

                // Assure that the source data file is closed
                globalWorkflow.LcMsRun.Close();
            }
        }

        [Test]
        public void SubclassStats()
        {
            const int subclassCol = 5;
            // string inFilename = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            // string inFilename = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeTrueTargets.tsv";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveTrueTargets.tsv";
            const string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeDecoyTargets.tsv";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoyTargets.tsv";

            var subclasses = new Dictionary<string, int>();
            var total = 0;

            using var reader = new StreamReader(new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // Read the header
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                // Split read in line so we can get the common name column
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var splitLine = line.Split('\t');

                // Get the common name for the lipid
                var name = splitLine[subclassCol];

                if (!subclasses.ContainsKey(name))
                {
                    subclasses.Add(name, 1);
                }
                else
                {
                    subclasses[name]++;
                }

                total++;
            }

            Console.WriteLine("Subclass Stats");

            foreach (var subclass in subclasses)
            {
                var percent = (((double)subclass.Value/total) * 100);
                Console.WriteLine(subclass.Key + "\t" + subclass.Value + "\t" + percent.ToString("##.000"));
            }
        }

        [Test]
        public void SubclassDivider()
        {
            const int subclassCol = 5;
            // string inFilename = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            // string inFilename = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeTrueTargets.tsv";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveTrueTargets.tsv";
            // string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeDecoyTargets.tsv";
            const string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoyTargets.tsv";

            const string outputDirectory = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoy";

            var subclasses = new Dictionary<string, List<string>>();

            using var reader = new StreamReader(new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // Read the header
            var header = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                // Split read in line so we can get the common name column
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var splitLine = line.Split('\t');

                // Get the common name for the lipid
                var name = splitLine[subclassCol];

                if (!subclasses.ContainsKey(name))
                {
                    subclasses.Add(name, new List<string>());
                    subclasses[name].Add(line);
                }
                else
                {
                    subclasses[name].Add(line);
                }
            }

            foreach (var subclass in subclasses)
            {
                using var writer = new StreamWriter(outputDirectory + "//" + subclass.Key + ".txt" );

                writer.WriteLine(header);

                foreach (var entry in subclass.Value)
                {
                    writer.WriteLine(entry);
                }
            }
        }

        [Test]
        public void FillCompAndMassForTargetsFile()
        {
            const string targetsFile = @"E:\Source\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys.txt";
            const string outputFile = @"E:\Source\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys_test.txt";

            const int massCol = 6;
            const int compCol = 7;

            var output = new List<string>();

            using (var targets = new StreamReader(new FileStream(targetsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var header = targets.ReadLine();
                output.Add(header);
                while (!targets.EndOfStream)
                {
                    var target = targets.ReadLine();
                    if (string.IsNullOrWhiteSpace(target))
                        continue;

                    var splitTarget = target.Split('\t');

                    try
                    {
                        if (string.IsNullOrEmpty(splitTarget[massCol]) || string.IsNullOrEmpty(splitTarget[compCol]))
                        {
                            var lipid = new Lipid { CommonName = splitTarget[1], AdductFull = splitTarget[2] };
                            var newTarget = lipid.CreateLipidTarget();
                            splitTarget[massCol] = newTarget.Composition.Mass.ToString(CultureInfo.InvariantCulture);
                            splitTarget[compCol] = newTarget.Composition.ToPlainString();
                        }

                        var rebuilt = new StringBuilder();
                        rebuilt.Append(splitTarget[0]);

                        for (var i = 1; i < splitTarget.Length; i++)
                        {
                            rebuilt.AppendFormat("\t{0}", splitTarget[i]);
                        }

                        output.Add(rebuilt.ToString());
                    }
                    catch (Exception ex)
                    {
                        // Ignore the error
                        Console.WriteLine("Exception in FillCompAndMassForTargetsFile: " + ex.Message);
                    }
                }
            }

            using (var writer = new StreamWriter(outputFile))
            {
                foreach (var x in output)
                {
                    writer.WriteLine(x);
                }
            }
        }

        [Test]
        public void RunPositiveDecoysForSvm()
        {
            var datasetNamesPositive = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_Late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_early_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_late_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_POS_150mm_8Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep1_10__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_POS_150mm_2Jun15_Polaroid_HSST3",
                "FSFA_Isolate_HL53_0100_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02"
            };
            const string positiveDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_POS_7b_Decoys.txt";
            RunWorkflowAndOutput(positiveDecoyTargetsFileLocation, "PositiveDecoyTargets.tsv", datasetNamesPositive);
        }

        [Test]
        public void RunNegativeDecoysForSvm()
        {
            var datasetNamesNegative = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_2d_Lipid_pooled_neg_150mm_17Apr15_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_NEG_150mm_10Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_NEG_150mm_27May15_Polaroid_HSST3",
                "FSFA_Isolate_HL53_0100_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02"
            };
            const string positiveDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_NEG_4_Decoys.txt";
            RunWorkflowAndOutput(positiveDecoyTargetsFileLocation, "NegativeDecoyTargets.tsv", datasetNamesNegative);
        }

        [Test]
        public void RunPositiveTargetsForSvm()
        {
            var datasetNamesPositive = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__POS_150mm_12Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_POS_150mm_23Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_POS_9Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_Mock_Late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_early_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_late_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_POS_150mm_5July15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_late_pooled_rand_POS_150mm_14July15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_691_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_AH1_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_2d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_FM_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_1d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_2d_Lipid_pooled_POS_150mm_17Apr15_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_4d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "OMICS_IM102_mock_7d_Lipid_pooled_POS_150mm_06May15_Polaroid_14-12-16",
                "LungMap_embedded_left_lobe_lung2_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_POS_150mm_02Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_POS_150mm_8Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_NEG_rep1_10__lip_POS_150mm_2Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_POS_150mm_2Jun15_Polaroid_HSST3",
                "FSFA_Isolate_HL53_0100_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_POS_150mm_21Aug15_Polaroid_HSST3-02"
            };
            const string positiveDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            RunWorkflowAndOutput(positiveDecoyTargetsFileLocation, "PositiveTargetsOutput.tsv", datasetNamesPositive);
        }

        [Test]
        public void RunNegativeTargetsForSvm()
        {
            var datasetNamesNegative = new List<string>
            {
                "OHSUblotter_case_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUblotter_control_lipid_pooled__NEG_150mm_17Jun15_Polaroid_HSST3-02",
                "OHSUserum_case_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OHSUserum_control_lipid_pooled_NEG_150mm_28Jun15_Polaroid_HSST3-02",
                "OMICS_ICL102_691_pooled_0_3_7_Lipid_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_691_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_AH1_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_FM_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_0_3_7_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL102_mock_pooled_12_18_24_NEG_12Jan15_Polaroid_14-12-16",
                "OMICS_ICL103_Mock_early_pooled_rand_NEG_150mm_23July15_Polaroid_HSST3-02",
                "OMICS_ICL103_NS1_Late_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_ICL103_VN1203_early_pooled_rand_NEG_150mm_15Aug15_Polaroid_HSST3-02",
                "OMICS_IM102_691_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_691_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_AH1_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_2d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_FM_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_1d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_2d_Lipid_pooled_neg_150mm_17Apr15_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_4d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "OMICS_IM102_mock_7d_Lipid_pooled_neg_150mm_22May15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "LungMap_embedded_left_lobe_lung2b_NEG_150mm_4Sept15_Polaroid_HSST3-02",
                "SOM_LIPIDS_3C_NEG_150mm_10Jun15_Polaroid_HSST3-02",
                "MinT_Kans_Gly_A_Plus_rep1_01__lip_NEG_150mm_27May15_Polaroid_HSST3",
                "FSFA_Isolate_HL53_0100_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02",
                "FSFA_Isolate_HL53_0400_lipid_NEG_150mm_24Aug15_Polaroid_HSST3-02"
            };
            const string positiveDecoyTargetsFileLocation = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            RunWorkflowAndOutput(positiveDecoyTargetsFileLocation, "NegativeTargetsOutput.tsv", datasetNamesNegative);
        }

        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\TrainingData\NegativeDecoyTargets.tsv")]
        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\TrainingData\NegativeVerified.tsv")]
        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\TrainingData\PositiveDecoyTargets.tsv")]
        [TestCase(@"\\protoapps\userdata\Wilkins\LiquidTestFiles\TrainingData\PositiveVerified.tsv")]
        public void TruncateFileColumns(string filePath)
        {
            // The headers we care about
            var columnHeaders = new []
                                {
                                    "Sub Class", "RT", "ppm Error", "Score", "Pearson Corr Score", "Pearson Corr M-1 Score",
                                    "Cosine Score", "Cosine M-1 Score"
                                };

            // construct output path
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);
            var extension = Path.GetExtension(filePath);
            var outputPath = Path.Combine(directory, string.Format("{0}_truncated{1}", fileName, extension));

            using (var writer = new StreamWriter(outputPath))
            {
                // write headers
                foreach (var header in columnHeaders)
                {
                    writer.Write(header+"\t");
                }

                writer.WriteLine();

                var headerDict = new Dictionary<string, int>();
                var lineCount = 0;
                foreach (var line in File.ReadLines(filePath))
                {
                    var parts = line.Split('\t');

                    if (lineCount++ == 0)
                    {   // store indices of headers in file
                        for (var i = 0; i < parts.Length; i++)
                        {
                            headerDict.Add(parts[i], i);
                        }
                        continue;
                    }

                    foreach (var header in columnHeaders)
                    {
                        var value = parts[headerDict[header]];
                        writer.Write(value+"\t");
                    }

                    writer.WriteLine();
                }
            }
        }

        public enum TargetsType
        {
            // environmental
            ENV,

            // human
            HUM
        }

        [Test]
        public void RunAndAggregateResultsPositive()
        {
            var posDatasets = new Dictionary<string, TargetsType>
            {
                #region positive_datasets
                    { "UDN_BG_B_P_QC-NIST_L_011_POS_22Sep16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_014_POS_22Sep16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_P1_A_UDN743456_L_024_POS_04Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_P2_A_UDN676680_L_026_POS_04Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_A_QC_NIST_L_022_POS_04Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_QC-NIST_L_058_POS_24Sep16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_105_POS_26Sep16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_216_POS_02Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_RVR0146_L_123_POS_28Sep16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_VU-036911139_L_210_POS_01Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "NASA_ISS_F_TP3_D20_50_R1_90_Pos_17May18_Lola-WCSH7804", TargetsType.ENV },
                    { "NASA_EC_F_TP1_D5_10_R1_17_Pos_17May18_Lola-WCSH7804", TargetsType.ENV },
                    { "NASA_EC_F_TP3_D20_50_R1_47_Pos_17May18_Lola-WCSH7804", TargetsType.ENV },
                    { "NASA_ISS_F_TP1_D5_10_R1_60_Pos_17May18_Lola-WCSH7804", TargetsType.ENV },
                    { "Agile_Rhodo_BIS3_18h_cell_R1_L_POS_17Oct17_Lola-WCSH5805", TargetsType.ENV },
                    { "Agile_Rhodo_GB2_18h_cell_R3_L_POS_17Oct17_Lola-WCSH5805", TargetsType.ENV },
                    { "Agile_Rhodo_bis14_48h_cell_R2_L_POS_17Oct17_Lola-WCSH5805", TargetsType.ENV },
                    { "Agile_Rhodo_WT_18h_cell_R3_L_POS_17Oct17_Lola-WCSH5805", TargetsType.ENV },
                    { "BetaMarker_Cyto_24h_1622_Lipid_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "BetaMarker_Cyto_24h_1693_Lipid_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "BetaMarker_noCyto_24h_1622_Lipid_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "BetaMarker_noCyto_24h_1693_Lipid_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "49259_Ncrassa_L00_3_Lip_45_Pos_27Mar18_Lola-WCSH7804", TargetsType.ENV },
                    { "49259_Ncrassa_D04_2_Lip_44_Pos_27Mar18_Lola-WCSH7804", TargetsType.ENV },
                    { "49259_Ncrassa_D10_3_Lip_03_Pos_27Mar18_Brandi-WCSH7804", TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_7_POS_25Apr17_Lola-WCSH7906", TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_8_POS_25Apr17_Lola-WCSH7906", TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_10_POS_25Apr17_Lola-WCSH7906", TargetsType.ENV },
                    { "49483_Harv_Organic_1_L_POS_11Sep17_Lola-WCSH7909", TargetsType.ENV },
                    { "49483_Harv_Mineral_3_L_POS_11Sep17_Lola-WCSH7909", TargetsType.ENV },
                    { "ExtTest_ACHN_MPLEx_1_L_Pos_29May18_Lola-WCSH7804", TargetsType.HUM },
                    { "ExtTest_TK10_MPLEx_1_L_Pos_29May18_Lola-WCSH7804", TargetsType.HUM },
                    { "hLM_Biop_D002_L_POS_22June17_Lola-WCSH7914", TargetsType.HUM },
                    { "hLM_Biop_D015_L_POS_22June17_Lola-WCSH7914", TargetsType.HUM },
                    { "hLM_Biop_D027_L_POS_22June17_Lola-WCSH7914", TargetsType.HUM },
                    { "hLM_Biop_D047_L_POS_22June17_Lola-WCSH7914", TargetsType.HUM },
                    { "hLM_Biop_D087_L_POS_22June17_Lola-WCSH7914", TargetsType.HUM },
                    { "hLm_HTC_D036_EPI_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D036_MES_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D036_MIC_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D036_PMX_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D019_END_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D019_EPI_2_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D019_MES_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D019_MIC_2_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "hLm_HTC_D019_PMX_1_L_POS_30Jul17_Lola-WCSH7909", TargetsType.HUM },
                    { "Marco_Soils_Lipids_12_POS_13Feb18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Marco_Soils_Lipids_17_POS_13Feb18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Marco_Soils_Lipids_31_POS_13Feb18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Marco_Soils_Lipids_44_POS_13Feb18_Brandi-WCSH5801", TargetsType.ENV },
                    { "KidneyAtlas_Pilot_Human_01_Pos_11Apr18_Lola-WCSH7804", TargetsType.HUM },
                    { "KidneyAtlas_Pilot_Human_02_Pos_11Apr18_Lola-WCSH7804", TargetsType.HUM },
                    { "KidneyAtlas_Pilot_Human_03_Pos_11Apr18_Lola-WCSH7804", TargetsType.HUM },
                    { "BIDMC-51-Lipid-Velos-POS_25Jan17_Lola-WCSH7905", TargetsType.HUM },
                    { "BIDMC-Lipid-P3-Velos-POS_25Jan17_Lola-WCSH7905", TargetsType.HUM },
                    { "BIDMC-Lipid-P4-Velos-POS_25Jan17_Lola-WCSH7905", TargetsType.HUM },
                    { "BIDMC-82-Lipid-Velos-POS_25Jan17_Lola-WCSH7905", TargetsType.HUM },
                    { "NICHD_PG_D5_02_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "NICHD_PG_D5_07_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "NICHD_PG_D6_03_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "NICHD_PG_D7_02_POS_09May17_Lola-WCSH7914", TargetsType.HUM },
                    { "K-Con-21-2_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "K-Con-128-2_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "K-post-30-3_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "K-post-68-1_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "K-pre-150-1_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "K-pre-176-2_Lipids_POS_24Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7018_Mut_F_B_L_021_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7018_Mut_M_B_L_008_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Ctl_F_B_L_009_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Ctl_M_B_L_003_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Mut_F_B_L_014_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Mut_M_B_L_016_POS_03Mar17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_delta174_L_Run1_POS_15Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_GR_L_Run1_POS_15Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_PE8_L_Run1_POS_15Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_Vps26-delta174_L_Run1_POS_15Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "Sporid_Cell_A_40H_2_L_025_POS_10Jan18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Sporid_Cell_G_24H_1_L_023_POS_10Jan18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Sporid_Cell_G-X_66H_3_L_011_POS_10Jan18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Sporid_Cell_pCA_90H_1_L_024_POS_10Jan18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Sporid_Cell_X_90H_2_L_006_POS_10Jan18_Brandi-WCSH5801", TargetsType.ENV },
                    { "Paraquat_Brain_34_C1_POS_24Feb17_Lola-WCSH7905", TargetsType.HUM },
                    { "Paraquat_Brain_401_D1_POS_24Feb17_Lola-WCSH7905", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0002410003_L_007_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0079790013_L_044_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0209440010_L_032_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0228220011_L_008_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0168270013_L_063_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0162060004_L_077_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204340004_L_055_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204420004_L_023_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204380005_L_057_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204400004_L_068_POS_03Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_Neff_L_2_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_Neff_L_3_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_T4_L_2_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_T4_L_1_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_Neff_L_1_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_Neff_L_3_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_T4_L_1_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_T4_L_3_Pos_04Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "DARPA_Lip_P1_T30_L1T_064_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P1_T30_L3M_070_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_L2B_027_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_W3T_051_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_W1M_024_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P3_T30_W1B_031_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P6_T60_L3T_052_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_L3M_015_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P6_T60_L2B_005_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P5_T60_W2T_008_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_W3M_105_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_W2T_086_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P5_T60_W2B_023_Pos_24Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "mLM_Elinav_BALF_GF_2_L_Pos_21Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_BALF_GF_3_L_Pos_21Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_BALF_SPF_1_L_Pos_21Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_LL_GF_3_L_Pos_22Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_LL_SPF_5_L_Pos_22Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_ROL_GF_3_L_Pos_22Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_ROL_SPF_4_L_Pos_22Feb19_Brandi-WCSH7811", TargetsType.HUM },
                #endregion
            };

            RunWorkflowAndOutputDifferentTargets(
                @"C:\Users\gibe617\Documents\liquid\TargetDatabase\Global_ENV_Dec2018_POS_v13.txt",
                @"C:\Users\gibe617\Documents\liquid\TargetDatabase\Global_Dec2017_POS_v12.txt",
                @"C:\Data\Liquid\Original\POS_ENV",
                @"C:\Data\Liquid\Original\POS",
                 posDatasets);
        }

        [Test]
        public void RunAndAggregateResultsNegative()
        {
            var negDatasets = new Dictionary<string, TargetsType>
            {
                #region negative_datasets
                    { "UDN_BG_B_P_QC-NIST_L_011_NEG_07Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_014_NEG_07Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_P1_A_UDN743456_L_024_NEG_19Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_P1_A_UDN676680_L_018_NEG_19Oct16_Lola-WCSH8005", TargetsType.HUM },
                    { "UDN_Pilot2_P_A_QC_NIST_L_022_NEG_19Oct16_Lola-WCSH8005",         TargetsType.HUM },
                    { "UDN_BG_B_P_QC-NIST_L_058_NEG_09Oct16_Lola-WCSH8005",             TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_105_NEG_11Oct16_Lola-WCSH8005",             TargetsType.HUM },
                    { "UDN_BG_B_P_QC-Pool_L_216_NEG_17Oct16_Lola-WCSH8005",             TargetsType.HUM },
                    { "UDN_BG_B_P_RVR0146_L_123_NEG_13Oct16_Lola-WCSH8005",             TargetsType.HUM },
                    { "UDN_BG_B_P_VU-036911139_L_210_NEG_16Oct16_Lola-WCSH8005",        TargetsType.HUM },
                    { "NASA_ISS_F_TP3_D20_50_R1_90_Neg_18May18_Lola-WCSH7804",          TargetsType.ENV },
                    { "NASA_EC_F_TP1_D5_10_R1_17_Neg_18May18_Lola-WCSH7804",            TargetsType.ENV },
                    { "NASA_EC_F_TP3_D20_50_R1_47_Neg_18May18_Lola-WCSH7804",           TargetsType.ENV },
                    { "NASA_ISS_F_TP1_D5_10_R1_60_Neg_18May18_Lola-WCSH7804",           TargetsType.ENV },
                    { "Agile_Rhodo_BIS3_18h_cell_R1_L_NEG_21Oct17_Lola-WCSH5805",       TargetsType.ENV },
                    { "Agile_Rhodo_GB2_18h_cell_R3_L_NEG_21Oct17_Lola-WCSH5805",        TargetsType.ENV },
                    { "Agile_Rhodo_bis14_48h_cell_R2_L_NEG_21Oct17_Lola-WCSH5805",      TargetsType.ENV },
                    { "Agile_Rhodo_WT_18h_cell_R3_L_NEG_21Oct17_Lola-WCSH5805",         TargetsType.ENV },
                    { "BetaMarker_Cyto_24h_1622_Lipid_NEG_08May17_Lola-WCSH7914",       TargetsType.HUM },
                    { "BetaMarker_Cyto_24h_1693_Lipid_NEG_08May17_Lola-WCSH7914",       TargetsType.HUM },
                    { "BetaMarker_noCyto_24h_1622_Lipid_NEG_08May17_Lola-WCSH7914",     TargetsType.HUM },
                    { "BetaMarker_noCyto_24h_1693_Lipid_NEG_08May17_Lola-WCSH7914",     TargetsType.HUM },
                    { "49259_Ncrassa_L00_3_Lip_45_NEG_29Mar18_Lola-WCSH7804",           TargetsType.ENV },
                    { "49259_Ncrassa_D04_2_Lip_44_NEG_29Mar18_Lola-WCSH7804",           TargetsType.ENV },
                    { "49259_Ncrassa_D10_3_Lip_03_NEG_29Mar18_Lola-WCSH7804",           TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_7_NEG_01May17_Lola-WCSH7906",      TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_8_NEG_01May17_Lola-WCSH7906",      TargetsType.ENV },
                    { "EMSL49444_Rich_Isogenie_Lipid_10_NEG_01May17_Lola-WCSH7906",     TargetsType.ENV },
                    { "49483_Harv_Organic_1_L_NEG_18Sep17_Lola-WCSH7909",               TargetsType.ENV },
                    { "49483_Harv_Mineral_3_L_NEG_18Sep17_Lola-WCSH7909",               TargetsType.ENV },
                    { "ExtTest_ACHN_MPLEx_1_L_Neg_29May18_Lola-WCSH7804",               TargetsType.HUM },
                    { "ExtTest_TK10_MPLEx_1_L_Neg_29May18_Lola-WCSH7804",               TargetsType.HUM },
                    { "hLM_Biop_D002_L_NEG_23June17_Lola-WCSH7914",                     TargetsType.HUM },
                    { "hLM_Biop_D015_L_NEG_23June17_Lola-WCSH7914",                     TargetsType.HUM },
                    { "hLM_Biop_D027_L_NEG_23June17_Lola-WCSH7914",                     TargetsType.HUM },
                    { "hLM_Biop_D047_L_NEG_23June17_Lola-WCSH7914",                     TargetsType.HUM },
                    { "hLM_Biop_D087_L_NEG_23June17_Lola-WCSH7914",                     TargetsType.HUM },
                    { "hLm_HTC_D036_END_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D036_EPI_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D036_MES_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D036_MIC_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D036_PMX_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D019_END_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D019_EPI_2_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D019_MES_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D019_MIC_2_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "hLm_HTC_D019_PMX_1_L_NEG_01Aug17_Lola-WCSH7909",                 TargetsType.HUM },
                    { "Marco_Soils_Lipids_12_NEG_18Feb18_Brandi-WCSH5801",          TargetsType.ENV },
                    { "Marco_Soils_Lipids_17_NEG_18Feb18_Brandi-WCSH5801",          TargetsType.ENV },
                    { "Marco_Soils_Lipids_31_NEG_18Feb18_Brandi-WCSH5801",          TargetsType.ENV },
                    { "Marco_Soils_Lipids_44_NEG_18Feb18_Brandi-WCSH5801",          TargetsType.ENV },
                    { "KidneyAtlas_Pilot_Human_01_Neg_12Apr18_Lola-WCSH7804",           TargetsType.HUM },
                    { "KidneyAtlas_Pilot_Human_02_Neg_12Apr18_Lola-WCSH7804",           TargetsType.HUM },
                    { "KidneyAtlas_Pilot_Human_03_Neg_12Apr18_Lola-WCSH7804",           TargetsType.HUM },
                    { "BIDMC-51-Lipid-Velos-NEG_03Feb17_Lola-WCSH7905",                 TargetsType.HUM },
                    { "BIDMC-Lipid-P3-Velos-NEG_03Feb17_Lola-WCSH7905",                 TargetsType.HUM },
                    { "BIDMC-Lipid-P4-Velos-NEG_03Feb17_Lola-WCSH7905",                 TargetsType.HUM },
                    { "BIDMC-82-Lipid-Velos-NEG_03Feb17_Lola-WCSH7905",                 TargetsType.HUM },
                    { "NICHD_PG_D5_02_NEG_08May17_Lola-WCSH7914",                       TargetsType.HUM },
                    { "NICHD_PG_D5_07_NEG_08May17_Lola-WCSH7914",                       TargetsType.HUM },
                    { "NICHD_PG_D6_03_NEG_08May17_Lola-WCSH7914",                       TargetsType.HUM },
                    { "NICHD_PG_D7_02_NEG_08May17_Lola-WCSH7914",                       TargetsType.HUM },
                    { "K-Con-21-2_Lipids_NEG_17Feb17_Lola-WCSH7905",                        TargetsType.ENV },
                    { "K-Con-128-2_Lipids_NEG_17Feb17_Lola-WCSH7905",                       TargetsType.ENV },
                    { "K-post-30-3_Lipids_NEG_17Feb17_Lola-WCSH7905",                       TargetsType.ENV },
                    { "K-post-68-1_Lipids_NEG_17Feb17_Lola-WCSH7905",                       TargetsType.ENV },
                    { "K-pre-150-1_Lipids_NEG_17Feb17_Lola-WCSH7905",                       TargetsType.ENV },
                    { "K-pre-176-2_Lipids_NEG_17Feb17_Lola-WCSH7905",                       TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7018_Mut_F_B_L_021_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7018_Mut_M_B_L_008_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Ctl_F_B_L_009_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Ctl_M_B_L_003_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Mut_F_B_L_014_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_ATP5D_7019_Mut_M_B_L_016_NEG_05Mar17_Lola-WCSH7905",  TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_delta174_L_Run1_NEG_17Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_GR_L_Run1_NEG_17Feb17_Lola-WCSH7905",       TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_PE8_L_Run1_NEG_17Feb17_Lola-WCSH7905",      TargetsType.ENV },
                    { "UDN_MOSC_Flies_iPLA2-VIA_Vps26-delta174_L_Run1_NEG_17Feb17_Lola-WCSH7905", TargetsType.ENV },
                    { "Sporid_Cell_A_40H_2_L_025_NEG_13Jan18_Brandi-WCSH5801",              TargetsType.ENV },
                    { "Sporid_Cell_G_24H_1_L_023_NEG_13Jan18_Brandi-WCSH5801",              TargetsType.ENV },
                    { "Sporid_Cell_G-X_66H_3_L_011_NEG_13Jan18_Brandi-WCSH5801",            TargetsType.ENV },
                    { "Sporid_Cell_pCA_90H_1_L_024_NEG_13Jan18_Brandi-WCSH5801",            TargetsType.ENV },
                    { "Sporid_Cell_X_90H_2_L_006_NEG_13Jan18_Brandi-WCSH5801",              TargetsType.ENV },
                    { "Paraquat_Brain_34_C1_NEG_22Feb17_Lola-WCSH7905",                      TargetsType.HUM },
                    { "Paraquat_Brain_401_D1_NEG_22Feb17_Lola-WCSH7905",                    TargetsType.HUM },
                    { "CPTAC_GBM_CPT0002410003_L_007_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0079790013_L_044_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0209440010_L_032_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0228220011_L_008_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0168270013_L_063_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0162060004_L_077_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204340004_L_055_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204420004_L_023_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204380005_L_057_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "CPTAC_GBM_CPT0204400004_L_068_NEG_06Dec18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_Neff_L_2_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_Neff_L_3_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_T4_L_2_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_pel_T4_L_1_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_Neff_L_1_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_Neff_L_3_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_T4_L_1_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "A_castellanii_ves_T4_L_3_Neg_05Sep18_Brandi-WCSH7803", TargetsType.HUM },
                    { "DARPA_Lip_P1_T30_L1T_064_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P1_T30_L3M_070_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_L2B_027_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_W3T_051_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P2_T30_W1M_024_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P3_T30_W1B_031_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P6_T60_L3T_052_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_L3M_015_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P6_T60_L2B_005_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P5_T60_W2T_008_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_W3M_105_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P4_T60_W2T_086_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "DARPA_Lip_P5_T60_W2B_023_Neg_29Oct18_Brandi-WCSH7803", TargetsType.ENV },
                    { "mLM_Elinav_BALF_GF_2_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_BALF_GF_3_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_BALF_SPF_1_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_LL_GF_3_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_LL_SPF_5_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_ROL_GF_3_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                    { "mLM_Elinav_ROL_SPF_4_L_Neg_27Feb19_Brandi-WCSH7811", TargetsType.HUM },
                #endregion
            };

            RunWorkflowAndOutputDifferentTargets(
                @"C:\Users\gibe617\Documents\liquid\TargetDatabase\Global_ENV_Dec2018_NEG_v9.txt",
                @"C:\Users\gibe617\Documents\liquid\TargetDatabase\Global_Aug2018_NEG_v10.txt",
                @"C:\Data\Liquid\Original\NEG_ENV",
                @"C:\Data\Liquid\Original\NEG",
                negDatasets);
        }

        /// <summary>
        /// Processing raw files similiar to RunWorkflowAndOutput but expects datasets to have already been copied locally
        /// </summary>
        /// <param name="targetsFilePath"></param>
        /// <param name="outputFileName"></param>
        /// <param name="datasetNamesList"></param>
        private void RunWorkflowAndOutputDifferentTargets(string envList, string humList, string envDir, string humDir, Dictionary<string, TargetsType> datasetNamesList)
        {
            var envTargetsFileInfo = new FileInfo(envList);
            var envLipidReader = new LipidMapsDbReader<Lipid>();
            var envLipidList = envLipidReader.ReadFile(envTargetsFileInfo);

            var humTargetsFileInfo = new FileInfo(humList);
            var humLipidReader = new LipidMapsDbReader<Lipid>();
            var humLipidList = humLipidReader.ReadFile(humTargetsFileInfo);

            // Parallel.ForEach(datasetNamesList, datasetNameKvp =>
            foreach (var datasetNameKvp in datasetNamesList)
            {
                var rawFileName = datasetNameKvp.Key + ".raw";
                var rawFilePath = Path.Combine(@"C:\Data\Liquid\Original", rawFileName);

                var lipidList = datasetNameKvp.Value == TargetsType.ENV ? envLipidList : humLipidList;
                var outputDirectory = datasetNameKvp.Value == TargetsType.ENV ? envDir : humDir;

                Console.WriteLine(DateTime.Now + ": Processing " + datasetNameKvp.Key);

                var globalWorkflow = new GlobalWorkflow(rawFilePath);
                var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);
                LipidGroupSearchResultWriter.OutputResults(lipidGroupSearchResults, $"{outputDirectory}/{datasetNameKvp.Key}.tsv", rawFileName, null, false, true, true);

                globalWorkflow.LcMsRun.Close();
            }
            //);
        }
    }
}
