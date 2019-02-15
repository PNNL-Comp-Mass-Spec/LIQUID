using LiquidBackend.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.Domain
{
    public class LipidRules
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
