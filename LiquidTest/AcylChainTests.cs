using LiquidBackend.Domain;
using NUnit.Framework;

namespace LiquidTest
{
	public class AcylChainTests
	{
		[Test]
		public void TestCreateAcylChain()
		{
			var chainString = "16:0";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(16, chain.NumCarbons);
			Assert.AreEqual(0, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Standard, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}

		[Test]
		public void TestCreateAcylChainPlasmalogen()
		{
			var chainString = "P-18:1";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(18, chain.NumCarbons);
			Assert.AreEqual(1, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Plasmalogen, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}

		[Test]
		public void TestCreateAcylChainEther()
		{
			var chainString = "O-20:4";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(20, chain.NumCarbons);
			Assert.AreEqual(4, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Ether, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}

		[Test]
		public void TestCreateAcylChainMonohydro()
		{
			var chainString = "m12:2";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(12, chain.NumCarbons);
			Assert.AreEqual(2, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Monohydro, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}

		[Test]
		public void TestCreateAcylChainDinohydro()
		{
			var chainString = "d8:3";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(8, chain.NumCarbons);
			Assert.AreEqual(3, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Dihydro, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}

		[Test]
		public void TestCreateAcylChainTrinohydro()
		{
			var chainString = "t31:6";
			var chain = new AcylChain(chainString);

			Assert.AreEqual(31, chain.NumCarbons);
			Assert.AreEqual(6, chain.NumDoubleBonds);
			Assert.AreEqual(AcylChainType.Trihydro, chain.AcylChainType);
			Assert.AreEqual(chainString, chain.ToString());
		}
	}
}
