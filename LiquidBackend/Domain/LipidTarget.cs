using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Composition;
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
		public Adduct Adduct { get; private set; }

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

		public string StrippedDisplay
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(this.LipidClass);

				List<AcylChain> acylChainList = this.AcylChainList.ToList();
				if (acylChainList.Count > 0)
				{
					stringBuilder.Append("(");

					for (int i = 0; i < acylChainList.Count; i++)
					{
						AcylChain acylChain = acylChainList[i];
						stringBuilder.Append(acylChain);
						if (i < acylChainList.Count - 1) stringBuilder.Append("/");
					}
					stringBuilder.Append(")");
				}

				return stringBuilder.ToString();
			}
		}

		public string AdductString
		{
			get { return this.Adduct != null ? this.Adduct.ToString() : "Unknown"; }
		}

		public LipidTarget(string commonName, LipidClass lipidClass, FragmentationMode fragmentationMode, Composition composition, IEnumerable<AcylChain> acylChainList, Adduct adduct = Adduct.Hydrogen)
		{
			CommonName = commonName;
			LipidClass = lipidClass;
			FragmentationMode = fragmentationMode;
			Composition = composition;
			AcylChainList = acylChainList;
			Adduct = adduct;
		}

		public List<MsMsSearchUnit> GetMsMsSearchUnits()
		{
			return LipidUtil.CreateMsMsSearchUnits(this.Composition.Mass, this.LipidClass, this.FragmentationMode, this.AcylChainList);
		}

		protected bool Equals(LipidTarget other)
		{
			return LipidClass == other.LipidClass && FragmentationMode == other.FragmentationMode && Equals(Composition, other.Composition) && AcylChainList.OrderBy(x => x.NumCarbons).ThenBy(x => x.NumDoubleBonds).ThenBy(x => x.AcylChainType).SequenceEqual(other.AcylChainList.OrderBy(x => x.NumCarbons).ThenBy(x => x.NumDoubleBonds).ThenBy(x => x.AcylChainType));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((LipidTarget) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (int) LipidClass;
				hashCode = (hashCode*397) ^ (int) FragmentationMode;
				hashCode = (hashCode*397) ^ (Composition != null ? Composition.GetHashCode() : 0);
				if (AcylChainList != null) hashCode = AcylChainList.OrderBy(x => x.NumCarbons).ThenBy(x => x.NumDoubleBonds).ThenBy(x => x.AcylChainType).Aggregate(hashCode, (current, acylChain) => (current * 397) ^ acylChain.GetHashCode());
				return hashCode;
			}
		}
	}
}
