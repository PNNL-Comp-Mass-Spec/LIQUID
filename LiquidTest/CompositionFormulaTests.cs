using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LiquidBackend.Domain;

namespace LiquidTest
{
	public class CompositionFormulaTests
	{
		[Test]
		public void TestCompositionFormulas()
		{
            Random rnd = new Random();
            int numCarbons = rnd.Next(1, 10);
            int numDoubleBonds = rnd.Next(1, 10);

            string formStr1 = "2X-2-2Y";
			CompositionFormula form1 = new CompositionFormula(formStr1);
            Assert.AreEqual(form1.Evaluate(numCarbons, numDoubleBonds), 2 * numCarbons - 2 - 2 * numDoubleBonds);

			string formStr2 = "X+15";
			CompositionFormula form2 = new CompositionFormula(formStr2);
            Assert.AreEqual(form2.Evaluate(numCarbons, numDoubleBonds), numCarbons + 15);

            string formStr3 = "-X+40+2Y";
			CompositionFormula form3 = new CompositionFormula(formStr3);
            Assert.AreEqual(form3.Evaluate(numCarbons, numDoubleBonds), - numCarbons + 40 + 2 * numDoubleBonds);

            string formStr4 = "2X-2Y-2";
            CompositionFormula form4 = new CompositionFormula(formStr4);
            Assert.AreEqual(form4.Evaluate(numCarbons, numDoubleBonds), 2 * numCarbons - 2 * numDoubleBonds - 2);

            string formStr5 = "10";
            CompositionFormula form5 = new CompositionFormula(formStr5);
            Assert.AreEqual(form5.Evaluate(numCarbons, numDoubleBonds), 10);
        }
	}
}
