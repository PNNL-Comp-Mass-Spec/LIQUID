using System;
using LiquidBackend.Domain;

namespace LiquidBackend.Scoring
{
    public class SpecificFragment : IComparable<SpecificFragment>
    {
        public LipidClass LipidClass { get; }
        public LipidType LipidType { get; }
        public string FragmentDescription { get; }
        public FragmentationMode FragmentationMode { get; }
        public FragmentationType FragmentationType { get; }

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
            if (!LipidClass.Equals(other.LipidClass))
                return LipidClass.CompareTo(other.LipidClass);

            if (!LipidType.Equals(other.LipidType))
                return LipidType.CompareTo(other.LipidType);

            if (!FragmentDescription.Equals(other.FragmentDescription))
                return string.Compare(FragmentDescription, other.FragmentDescription, StringComparison.Ordinal);

            if (!FragmentationMode.Equals(other.FragmentationMode))
                return FragmentationMode.CompareTo(other.FragmentationMode);

            return FragmentationType.CompareTo(other.FragmentationType);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpecificFragment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)LipidClass;
                hashCode = (hashCode * 397) ^ (int)LipidType;
                hashCode = (hashCode * 397) ^ (FragmentDescription?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)FragmentationMode;
                hashCode = (hashCode * 397) ^ (int)FragmentationType;
                return hashCode;
            }
        }
    }
}
