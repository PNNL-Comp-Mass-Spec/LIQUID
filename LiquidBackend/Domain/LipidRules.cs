using LiquidBackend.IO;
using System.Collections.Generic;
using System.IO;

namespace LiquidBackend.Domain
{
    public static class LipidRules
    {
        public static List<LipidCompositionRule> LipidCompositionRules { get; set; }
        public static List<LipidFragmentationRule> LipidFragmentationRules { get; set; }
        public static void LoadLipidRules(string fileForCompositionRules, string fileForFragmentationRules)
        {
            LoadLipidCompositionRules(fileForCompositionRules);
            LoadLipidFragmentationRules(fileForFragmentationRules);
        }

        public static void LoadLipidCompositionRules(string fileLocation)
        {
            var fileInfo = new FileInfo(fileLocation);
            var lipidCompositionRuleReader = new LipidCompositionRuleReader<LipidCompositionRule>();
            LipidCompositionRules = lipidCompositionRuleReader.ReadFile(fileInfo);
        }

        public static void LoadLipidFragmentationRules(string fileLocation)
        {
            var fileInfo = new FileInfo(fileLocation);
            var lipidFragmentationRulesReader = new LipidFragmentationRuleReader<LipidFragmentationRule>();
            LipidFragmentationRules = lipidFragmentationRulesReader.ReadFile(fileInfo);
        }
    }
}
