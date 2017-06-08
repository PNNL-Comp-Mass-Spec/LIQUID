using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiquidBackend.Domain;
using LiquidBackend.Util;

namespace LiquidBackend.Scoring
{
    public class ScoreModelCreator
    {
        public HashSet<string> DatasetLocations { get; }
        public HashSet<Lipid> LipidList { get; }

        private readonly List<double> _binList;

        public ScoreModelCreator()
        {
            DatasetLocations = new HashSet<string>();
            LipidList = new HashSet<Lipid>();
            _binList = new List<double>{0, 0.25, 0.5, 0.75, 1};
        }

        public ScoreModel CreateScoreModel(double hcdError, double cidError)
        {
            const int numTopHitsToConsider = 1;

            var observationDictionary = new Dictionary<SpecificFragment, List<double>>();

            foreach (var datasetLocation in DatasetLocations)
            {
                // Setup workflow
                var globalWorkflow = new GlobalWorkflow(datasetLocation);

                // Run workflow
                var lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(LipidList, 30, 30, 500);

                // Group results of same scan together
                var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

                // Grab the result(s) with the best score
                foreach (var group in resultsGroupedByScan)
                {
                    var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

                    for (var i = 0; i < numTopHitsToConsider && i < groupOrdered.Count; i++)
                    {
                        var resultToAdd = groupOrdered[i];

                        var lipidTarget = resultToAdd.LipidTarget;
                        var lipidClass = lipidTarget.LipidClass;
                        var lipidType = lipidTarget.LipidType;
                        var fragmentationMode = lipidTarget.FragmentationMode;
                        var spectrumSearchResult = resultToAdd.SpectrumSearchResult;
                        var cidResultList = spectrumSearchResult.CidSearchResultList;
                        var hcdResultList = spectrumSearchResult.HcdSearchResultList;

                        var cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
                        var hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

                        // CID Results
                        foreach (var cidResult in cidResultList)
                        {
                            var fragment = cidResult.TheoreticalPeak.Description;
                            double intensity = 0;

                            if (cidResult.ObservedPeak != null)
                            {
                                intensity = Math.Log10(cidResult.ObservedPeak.Intensity)/Math.Log10(cidMaxValue);
                            }

                            var specificFragment = new SpecificFragment(lipidClass, lipidType, fragment, fragmentationMode, FragmentationType.CID);

                            // Either update the observation list or create a new one
                            List<double> observationList;
                            if (observationDictionary.TryGetValue(specificFragment, out observationList))
                            {
                                observationList.Add(intensity);
                            }
                            else
                            {
                                observationList = new List<double> {intensity};
                                observationDictionary.Add(specificFragment, observationList);
                            }
                        }

                        // HCD Results
                        foreach (var hcdResult in hcdResultList)
                        {
                            var fragment = hcdResult.TheoreticalPeak.Description;
                            double intensity = 0;

                            if (hcdResult.ObservedPeak != null)
                            {
                                intensity = Math.Log10(hcdResult.ObservedPeak.Intensity)/Math.Log10(hcdMaxValue);
                            }

                            var specificFragment = new SpecificFragment(lipidClass, lipidType, fragment, fragmentationMode, FragmentationType.HCD);

                            // Either update the observation list or create a new one
                            List<double> observationList;
                            if (observationDictionary.TryGetValue(specificFragment, out observationList))
                            {
                                observationList.Add(intensity);
                            }
                            else
                            {
                                observationList = new List<double> { intensity };
                                observationDictionary.Add(specificFragment, observationList);
                            }
                        }
                    }
                }

                // Assure that the source data file is closed
                globalWorkflow.LcMsRun.Close();
            }

            var liquidScoreModelUnitList = PartitionIntoModelUnits(observationDictionary);
            var liquidScoreModel = new ScoreModel(liquidScoreModelUnitList);

            return liquidScoreModel;
        }

        public void AddDatasets(IEnumerable<string> datasetLocations)
        {
            foreach (var datasetLocation in datasetLocations)
            {
                AddDataset(datasetLocation);
            }
        }

        public void AddDataset(string datasetLocation)
        {
            if (!File.Exists(datasetLocation))
            {
                throw new FileNotFoundException("Unable to load dataset at " + datasetLocation + ". File not found.");
            }

            var fileInfo = new FileInfo(datasetLocation);
            DatasetLocations.Add(fileInfo.FullName);
        }

        public void AddDmsDatasets(IEnumerable<string> datasetNames)
        {
            foreach (var datasetName in datasetNames)
            {
                AddDmsDataset(datasetName);
            }
        }

        public void AddDmsDataset(string datasetName)
        {
            var rawFileName = datasetName + ".raw";

            if (!File.Exists(rawFileName))
            {
                // Lookup in DMS via Mage
                var dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
                var dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
                var fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

                // Copy Locally
                // TODO: Handle files that are on MyEMSL
                File.Copy(fullPathToDmsFile, rawFileName);
            }

            AddDataset(rawFileName);
        }

        public void AddLipidTargets(IEnumerable<Lipid> lipidList)
        {
            foreach (var lipid in lipidList)
            {
                LipidList.Add(lipid);
            }
        }

        private List<ScoreModelUnit> PartitionIntoModelUnits(Dictionary<SpecificFragment, List<double>> observationDictionary)
        {
            var liquidScoreModelUnitList = new List<ScoreModelUnit>();

            foreach (var kvp in observationDictionary)
            {
                var specificFragment = kvp.Key;
                var intensityList = kvp.Value;

                // Create list to keep track of the number of observations for each bin
                var countList = new List<double>();
                for (var i = 0; i < _binList.Count; i++)
                {
                    countList.Add(0);
                }

                // Figure out which bin the intensity belongs to and update its observation count
                foreach (var intensity in intensityList)
                {
                    for (var i = 0; i < _binList.Count; i++)
                    {
                        var bin = _binList[i];
                        if (intensity <= bin)
                        {
                            countList[i]++;
                            break;
                        }
                    }
                }

                for (var i = 0; i < _binList.Count; i++)
                {
                    var intensityMax = _binList[i];
                    var count = countList[i];
                    var percentage = count > 0 ? count / intensityList.Count : 0.00001;

                    if (percentage >= 1) percentage = 0.99999;

                    // TODO: Actually calculate noise probabilities instead of using magic number
                    var percentageNoise = i == 0 ? 0.98 : 0.005;

                    var liquidScoreModelUnit = new ScoreModelUnit(specificFragment, intensityMax, percentage, percentageNoise);
                    liquidScoreModelUnitList.Add(liquidScoreModelUnit);
                }
            }

            return liquidScoreModelUnitList;
        }
    }
}
