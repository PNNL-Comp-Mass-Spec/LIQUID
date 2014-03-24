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
		public LipidType LipidType { get; private set; }

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

			this.LipidType = FigureOutLipidType();
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

		private LipidType FigureOutLipidType()
		{
			if (this.LipidClass == LipidClass.Ubiquitones || this.LipidClass == LipidClass.Cholesterol) return LipidType.Standard;

			int chainCount = 0;
			int standardChainCount = 0;
			int plasmogenChainCount = 0;
			int etherChainCount = 0;
			int dihydroxyChainCount = 0;
			int trihydroChainCount = 0;

			foreach (AcylChain acylChain in AcylChainList)
			{
				if (acylChain.NumCarbons < 1) continue;

				AcylChainType chainType = acylChain.AcylChainType;
				if (chainType == AcylChainType.Standard) standardChainCount++;
				else if (chainType == AcylChainType.Plasmalogen) plasmogenChainCount++;
				else if (chainType == AcylChainType.Ether) etherChainCount++;
				else if (chainType == AcylChainType.Dihydro || chainType == AcylChainType.Dihydroxy) dihydroxyChainCount++;
				else if (chainType == AcylChainType.Trihydro) trihydroChainCount++;

				chainCount++;
			}

			if (chainCount == 1)
			{
				if (standardChainCount == 1) return LipidType.SingleChain;
				if (plasmogenChainCount == 1) return LipidType.SingleChainPlasmogen;
				if (etherChainCount == 1) return LipidType.SingleChainEther;
			}
			if (chainCount == 2)
			{
				if (standardChainCount == 2) return LipidType.TwoChains;
				if (plasmogenChainCount == 1) return LipidType.TwoChainsPlasmogen;
				if (etherChainCount == 1) return LipidType.TwoChainsEther;
				if (trihydroChainCount == 1)
				{
					if (dihydroxyChainCount == 1) return LipidType.TwoChainsDihidroxyPhyto;
					else return LipidType.TwoChainsPhyto;
				}
				if (dihydroxyChainCount == 1) return LipidType.TwoChainsDihidroxy;
				if (dihydroxyChainCount == 2) return LipidType.TwoChainsTwoDihidroxy;
			}
			if (chainCount == 3)
			{
				return LipidType.ThreeChains;
			}

			throw new SystemException("Unable to determine LipidType for LipidTarget: " + this.ToString());
		}
	}
}
