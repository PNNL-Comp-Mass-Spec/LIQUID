using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;
using LiquidBackend.Util;
using Mage;

namespace LiquidBackend.Scoring
{
	public class ScoreModelCreator
	{
		public HashSet<string> DatasetLocations { get; private set; } 
		public HashSet<Lipid> LipidList { get; private set; }

		private List<double> _binList;

		public ScoreModelCreator()
		{
			this.DatasetLocations = new HashSet<string>();
			this.LipidList = new HashSet<Lipid>();
			_binList = new List<double>{0, 0.25, 0.5, 0.75, 1};
		}

		public ScoreModel CreateScoreModel(double hcdError, double cidError)
		{
			const int numTopHitsToConsider = 1;

			Dictionary<SpecificFragment, List<double>> observationDictionary = new Dictionary<SpecificFragment, List<double>>();

			foreach (string datasetLocation in this.DatasetLocations)
			{
				// Setup workflow
				GlobalWorkflow globalWorkflow = new GlobalWorkflow(datasetLocation);

				// Run workflow
				List<LipidGroupSearchResult> lipidGroupSearchResults = globalWorkflow.RunGlobalWorkflow(this.LipidList, 30, 500);

				// Group results of same scan together
				var resultsGroupedByScan = lipidGroupSearchResults.GroupBy(x => x.SpectrumSearchResult.HcdSpectrum.ScanNum);

				// Grab the result(s) with the best score
				foreach (var group in resultsGroupedByScan)
				{
					var groupOrdered = group.OrderByDescending(x => x.SpectrumSearchResult.Score).ToList();

					for (int i = 0; i < numTopHitsToConsider && i < groupOrdered.Count; i++)
					{
						LipidGroupSearchResult resultToAdd = groupOrdered[i];

						LipidTarget lipidTarget = resultToAdd.LipidTarget;
						LipidClass lipidClass = lipidTarget.LipidClass;
						LipidType lipidType = lipidTarget.LipidType;
						FragmentationMode fragmentationMode = lipidTarget.FragmentationMode;
						SpectrumSearchResult spectrumSearchResult = resultToAdd.SpectrumSearchResult;
						List<MsMsSearchResult> cidResultList = spectrumSearchResult.CidSearchResultList;
						List<MsMsSearchResult> hcdResultList = spectrumSearchResult.HcdSearchResultList;

						double cidMaxValue = spectrumSearchResult.CidSpectrum.Peaks.Any() ? spectrumSearchResult.CidSpectrum.Peaks.Max(x => x.Intensity) : 1;
						double hcdMaxValue = spectrumSearchResult.HcdSpectrum.Peaks.Any() ? spectrumSearchResult.HcdSpectrum.Peaks.Max(x => x.Intensity) : 1;

						// CID Results
						foreach (var cidResult in cidResultList)
						{
							string fragment = cidResult.TheoreticalPeak.Description;
							double intensity = 0;
						
							if (cidResult.ObservedPeak != null)
							{
								intensity = Math.Log10(cidResult.ObservedPeak.Intensity)/Math.Log10(cidMaxValue);
							}

							SpecificFragment specificFragment = new SpecificFragment(lipidClass, lipidType, fragment, fragmentationMode, FragmentationType.CID);

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
							string fragment = hcdResult.TheoreticalPeak.Description;
							double intensity = 0;
							
							if (hcdResult.ObservedPeak != null)
							{
								intensity = Math.Log10(hcdResult.ObservedPeak.Intensity)/Math.Log10(hcdMaxValue);
							}

							SpecificFragment specificFragment = new SpecificFragment(lipidClass, lipidType, fragment, fragmentationMode, FragmentationType.HCD);

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

			List<ScoreModelUnit> liquidScoreModelUnitList = PartitionIntoModelUnits(observationDictionary);
			ScoreModel liquidScoreModel = new ScoreModel(liquidScoreModelUnitList);
            
            return liquidScoreModel;
		}

		public void AddDatasets(IEnumerable<string> datasetLocations)
		{
			foreach (string datasetLocation in datasetLocations)
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

			FileInfo fileInfo = new FileInfo(datasetLocation);
			this.DatasetLocations.Add(fileInfo.FullName);
		}

		public void AddDmsDatasets(IEnumerable<string> datasetNames)
		{
			foreach (string datasetName in datasetNames)
			{
				AddDmsDataset(datasetName);
			}
		}

		public void AddDmsDataset(string datasetName)
		{
			string rawFileName = datasetName + ".raw";

			if (!File.Exists(rawFileName))
			{
				// Lookup in DMS via Mage
				string dmsFolder = DmsDatasetFinder.FindLocationOfDataset(datasetName);
				DirectoryInfo dmsDirectoryInfo = new DirectoryInfo(dmsFolder);
				string fullPathToDmsFile = Path.Combine(dmsDirectoryInfo.FullName, rawFileName);

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
				this.LipidList.Add(lipid);
			}
		}

		private List<ScoreModelUnit> PartitionIntoModelUnits(Dictionary<SpecificFragment, List<double>> observationDictionary)
		{
			List<ScoreModelUnit> liquidScoreModelUnitList = new List<ScoreModelUnit>();

			foreach (var kvp in observationDictionary)
			{
				SpecificFragment specificFragment = kvp.Key;
				List<double> intensityList = kvp.Value;

				// Create list to keep track of the number of observations for each bin
				List<double> countList = new List<double>();
				for (int i = 0; i < _binList.Count; i++)
				{
					countList.Add(0);
				}

				// Figure out which bin the intensity belongs to and update its observation count
				foreach (double intensity in intensityList)
				{
					for (int i = 0; i < _binList.Count; i++)
					{
						double bin = _binList[i];
						if (intensity <= bin)
						{
							countList[i]++;
							break;
						}
					}
				}

				for (int i = 0; i < _binList.Count; i++)
				{
					double intensityMax = _binList[i];
					double count = countList[i];
					double percentage = count > 0 ? count / intensityList.Count : 0.00001;

					if (percentage >= 1) percentage = 0.99999;

					// TODO: Actually calculate noise probabilities instead of using magic number
					double percentageNoise = i == 0 ? 0.98 : 0.005;

					ScoreModelUnit liquidScoreModelUnit = new ScoreModelUnit(specificFragment, intensityMax, percentage, percentageNoise);
					liquidScoreModelUnitList.Add(liquidScoreModelUnit);
				}
			}

			return liquidScoreModelUnitList;
		}
	}
}
