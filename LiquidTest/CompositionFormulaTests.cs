using System;
using NUnit.Framework;
using LiquidBackend.Domain;

namespace LiquidTest
{
    public class CompositionFormulaTests
    {
        [Test]
        public void TestCompositionFormulas()
        {
            var rnd = new Random();
            var numCarbons = rnd.Next(1, 10);
            var numDoubleBonds = rnd.Next(1, 10);

            const string formStr1 = "2X-2-2Y";
            var form1 = new CompositionFormula(formStr1);
            Assert.AreEqual(form1.Evaluate(numCarbons, numDoubleBonds), 2 * numCarbons - 2 - 2 * numDoubleBonds);

            const string formStr2 = "X+15";
            var form2 = new CompositionFormula(formStr2);
            Assert.AreEqual(form2.Evaluate(numCarbons, numDoubleBonds), numCarbons + 15);

            const string formStr3 = "-X+40+2Y";
            var form3 = new CompositionFormula(formStr3);
            Assert.AreEqual(form3.Evaluate(numCarbons, numDoubleBonds), - numCarbons + 40 + 2 * numDoubleBonds);

            const string formStr4 = "2X-2Y-2";
            var form4 = new CompositionFormula(formStr4);
            Assert.AreEqual(form4.Evaluate(numCarbons, numDoubleBonds), 2 * numCarbons - 2 * numDoubleBonds - 2);

            const string formStr5 = "10";
            var form5 = new CompositionFormula(formStr5);
            Assert.AreEqual(form5.Evaluate(numCarbons, numDoubleBonds), 10);
        }
    }
}
