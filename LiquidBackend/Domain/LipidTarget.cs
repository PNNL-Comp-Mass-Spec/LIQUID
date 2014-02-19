using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Sequence;
using LiquidBackend.Util;

namespace LiquidBackend.Domain
{
	public class LipidTarget
	{
		public string CommonName { get; private set; }
		public LipidClass LipidClass { get; private set; }
		public FragmentationMode FragmentationMode { get; private set; }
		public Composition Composition { get; private set; }
		public IEnumerable<AcylChain> AcylChainList { get; private set; }

		public double MzRounded
		{
			get { return Math.Round(this.Composition.Mass, 4); }
		}

		public List<MsMsSearchUnit> SortedMsMsSearchUnits
		{
			get { return this.GetMsMsSearchUnits().OrderBy(x => x.Mz).ToList(); }
		}

		public string EmpiricalFormula
		{
			get { return this.Composition.ToPlainString(); }
		}

		public LipidTarget(string commonName, LipidClass lipidClass, FragmentationMode fragmentationMode, Composition composition, IEnumerable<AcylChain> acylChainList)
		{
			CommonName = commonName;
			LipidClass = lipidClass;
			FragmentationMode = fragmentationMode;
			Composition = composition;
			AcylChainList = acylChainList;
		}

		public List<MsMsSearchUnit> GetMsMsSearchUnits()
		{
			return LipidUtil.CreateMsMsSearchUnits(this.Composition.Mass, this.LipidClass, this.FragmentationMode, this.AcylChainList);
		}
	}
}
