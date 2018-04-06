using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.Domain
{
	public class LipidCompositionRule
	{

		public string CommonName { get; set; }
		public string Category { get; set; }
		public string MainClass { get; set; }
		public string SubClass { get; set; }
		public string C { get; set; }
		public string H { get; set; }
		public string N { get; set; }
		public string O { get; set; }
		public string S { get; set; }
		public string P { get; set; }
		public string Example { get; set; }
		public string Formula { get; set; }
		public string IonizationMode { get; set; }
		public int NumChains { get; set; }
		public int ContainsEther { get; set; }
		public int ContainsDiether { get; set; }
		public int ContainsPlasmalogen { get; set; }
		public int ContainsLCB { get; set; }
		public int ContainsLCBOH { get; set; }
		public int ContainsOH { get; set; }
		public int ContainsDeoxy { get; set; }
		public bool IsOxoCHO { get; set; }
		public bool IsOxoCOOH { get; set; }

	}
}
