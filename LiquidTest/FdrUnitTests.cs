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

        /// <summary>
        /// Run the files for verification of new scoring modelb
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
            const string positiveTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_7b.txt";
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
            const string negativeTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_4.txt";
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
            const string positiveDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_7b_Decoys.txt";
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
            const string negativeDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_4_Decoys.txt";
            RunWorkflowAndOutput(negativeDecoyTargetsFileLocation, "NegativeDecoyTargets.tsv", datasetNamesNegative);

        }

        /// <summary>
        /// Main functionality for running the LIQUID workflow and outputting the results
        /// </summary>
        /// <param name="targetsFilePath"></param>
        /// <param name="outputFileName"></param>
        /// <param name="datasetNamesList"></param>
        private void RunWorkflowAndOutput(string targetsFilePath, string outputFileName, List<string> datasetNamesList)
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

                    // Lookup in DMS via Mage
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
                var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(lipidList, 30, 30, 500);

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
            var subclassCol = 5;
            //string inFilename = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            //string inFilename = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeTrueTargets.tsv";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveTrueTargets.tsv";
            var inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeDecoyTargets.tsv";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoyTargets.tsv";

            var subclasses = new Dictionary<string, int>();
            var total = 0;

            using (var reader = new StreamReader(new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                //Read the header
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    //Split read in line so we can get the common name column
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var splitLine = line.Split('\t');

                    //Get the common name for the lipid
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
            var subclassCol = 5;
            //string inFilename = "../../../testFiles/Global_LipidMaps_NEG_4.txt";
            //string inFilename = "../../../testFiles/Global_LipidMaps_POS_7b.txt";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeTrueTargets.tsv";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveTrueTargets.tsv";
            //string inFilename = @"C:\Users\fuji510\Desktop\LiquidData\NegativeDecoyTargets.tsv";
            var inFilename = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoyTargets.tsv";

            var outputDirectory = @"C:\Users\fuji510\Desktop\LiquidData\PositiveDecoy";

            string header;

            var subclasses = new Dictionary<string, List<string>>();

            using (var reader = new StreamReader(new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                //Read the header
                header = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    //Split read in line so we can get the common name column
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var splitLine = line.Split('\t');

                    //Get the common name for the lipid
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
            }

            foreach (var subclass in subclasses)
            {
                using (var writer = new StreamWriter(outputDirectory + "//" + subclass.Key + ".txt" ))
                {
                    writer.WriteLine(header);
                    foreach (var entry in subclass.Value)
                    {
                        writer.WriteLine(entry);
                    }

                }
            }
        }

        [Test]
        public void FillCompAndMassForTargetsFile()
        {
            const string targetsFile = @"E:\Source\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys.txt";
            const string outputfile = @"E:\Source\Liquid\trunk\LiquidTest\testFiles\Global_LipidMaps_POS_7b_Decoys_test.txt";

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
                            rebuilt.Append("\t" + splitTarget[i]);
                        }

                        output.Add(rebuilt.ToString());
                    }
                    catch (Exception)
                    {
                        // Ignore the error
                    }
                }
            }

            using (var outstream = new StreamWriter(outputfile))
            {
                foreach (var x in output)
                {
                    outstream.WriteLine(x);
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
            const string positiveDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_7b_Decoys.txt";
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
            const string positiveDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_4_Decoys.txt";
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
            const string positiveDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_POS_7b.txt";
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
            const string positiveDecoyTargetsFileLocation = @"../../../testFiles/Global_LipidMaps_NEG_4.txt";
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

    }

}
