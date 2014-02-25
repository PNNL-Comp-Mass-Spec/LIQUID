using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

			globalWorkflow.RunGlobalWorkflow(lipidList, 30, 500);
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
	}
}
