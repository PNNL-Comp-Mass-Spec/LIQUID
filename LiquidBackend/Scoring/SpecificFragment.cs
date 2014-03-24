using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiquidBackend.Domain;

namespace LiquidBackend.Scoring
{
	public class SpecificFragment : IComparable<SpecificFragment>
	{
		public LipidClass LipidClass { get; private set; }
		public LipidType LipidType { get; private set; }
		public string FragmentDescription { get; private set; }
		public FragmentationMode FragmentationMode { get; private set; }
		public FragmentationType FragmentationType { get; private set; }

		public SpecificFragment(LipidClass lipidClass, LipidType lipidType, string fragmentDescription, FragmentationMode fragmentationMode, FragmentationType fragmentationType)
		{
			LipidClass = lipidClass;
			LipidType = lipidType;
			FragmentDescription = fragmentDescription;
			FragmentationMode = fragmentationMode;
			FragmentationType = fragmentationType;
		}

		protected bool Equals(SpecificFragment other)
		{
			return LipidClass == other.LipidClass && LipidType == other.LipidType && string.Equals(FragmentDescription, other.FragmentDescription) && FragmentationMode == other.FragmentationMode && FragmentationType == other.FragmentationType;
		}

		public int CompareTo(SpecificFragment other)
		{
			if (!this.LipidClass.Equals(other.LipidClass)) return this.LipidClass.CompareTo(other.LipidClass);
			if (!this.LipidType.Equals(other.LipidType)) return this.LipidType.CompareTo(other.LipidType);
			if (!this.FragmentDescription.Equals(other.FragmentDescription)) return String.Compare(this.FragmentDescription, other.FragmentDescription, StringComparison.Ordinal);
			if (!this.FragmentationMode.Equals(other.FragmentationMode)) return this.FragmentationMode.CompareTo(other.FragmentationMode);
			return this.FragmentationType.CompareTo(other.FragmentationType);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SpecificFragment) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (int) LipidClass;
				hashCode = (hashCode*397) ^ (int) LipidType;
				hashCode = (hashCode*397) ^ (FragmentDescription != null ? FragmentDescription.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (int) FragmentationMode;
				hashCode = (hashCode*397) ^ (int) FragmentationType;
				return hashCode;
			}
		}
	}
}
