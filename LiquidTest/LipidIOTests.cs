using System;
using System.IO;
using LiquidBackend.Domain;
using LiquidBackend.IO;
using NUnit.Framework;

namespace LiquidTest
{
    public class LipidIOTests
    {
        [Test]
        public void TestReadLipidMaps()
        {
            const string fileLocation = "../../../testFiles/Global_LipidMaps_Pos.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);
            Console.WriteLine(lipidList.Count);
        }

        [Test]
        public void TestReadLipidMapsAndCreateTargets()
        {
            const string fileLocation = "../../../testFiles/Global_LipidMaps_Pos.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);
            Console.WriteLine(lipidList.Count);

            foreach (var lipid in lipidList)
            {
                var lipidTarget = lipid.LipidTarget;
                Console.WriteLine(lipidTarget.CommonName);
            }
        }

        [Test]
        public void TestReadLipidMapsNegativeAndCreateTargets()
        {
            const string fileLocation = "../../../testFiles/Global_LipidMaps_Neg.txt";
            var fileInfo = new FileInfo(fileLocation);
            var lipidReader = new LipidMapsDbReader<Lipid>();
            var lipidList = lipidReader.ReadFile(fileInfo);
            Console.WriteLine(lipidList.Count);

            foreach (var lipid in lipidList)
            {
                var lipidTarget = lipid.LipidTarget;
                Console.WriteLine(lipidTarget.CommonName);
            }
        }
    }
}
