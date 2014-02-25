using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Util;

namespace LiquidBackend.Domain
{
	public class Lipid
	{
		private LipidTarget _lipidTarget;

		public string LipidMapsId { get; set; }
		public string CommonName { get; set; }
		public string AdductFull { get; set; }
		public string Category { get; set; }
		public string MainClass { get; set; }
		public string SubClass { get; set; }
		public string PubChemSid { get; set; }
		public string PubChemCid { get; set; }
		public string InchiKey { get; set; }
		public string KeggId { get; set; }
		public string HmdbId { get; set; }
		public int ChebiId { get; set; }
		public int LipidatId { get; set; }
		public string LipidBankId { get; set; }

		public LipidTarget LipidTarget
		{
			get { return _lipidTarget ?? (_lipidTarget = CreateLipidTarget()); }
		}

		public LipidTarget CreateLipidTarget()
		{
			Adduct adduct;
			FragmentationMode fragmentationMode;

			if (this.AdductFull == "[M+H]+")
			{
				adduct = Adduct.Hydrogen;
				fragmentationMode = FragmentationMode.Positive;
			}
			else if (this.AdductFull == "[M+NH4]+")
			{
				adduct = Adduct.Ammonium;
				fragmentationMode = FragmentationMode.Positive;
			}
			else if (this.AdductFull == "[M+Oac]-")
			{
				adduct = Adduct.Acetate;
				fragmentationMode = FragmentationMode.Negative;
			}
			else if (this.AdductFull == "[M-H]-")
			{
				adduct = Adduct.Hydrogen;
				fragmentationMode = FragmentationMode.Negative;
			}
			else
			{
				throw new SystemException("Unknown adduct: " + this.AdductFull);
			}

			LipidTarget lipidTarget = LipidUtil.CreateLipidTarget(this.CommonName, fragmentationMode, adduct);
			return lipidTarget;
		}
	}
}
