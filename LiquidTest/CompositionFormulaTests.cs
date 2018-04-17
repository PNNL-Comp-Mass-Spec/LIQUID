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
			string formStr1 = "2X-2-2Y";
			CompositionFormula form1 = new CompositionFormula(formStr1);

			string formStr2 = "X+15";
			CompositionFormula form2 = new CompositionFormula(formStr2);

			string formStr3 = "-X+40+2Y";
			CompositionFormula form3 = new CompositionFormula(formStr3);
		}
	}
}
