using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using LiquidBackend.Domain;

namespace LiquidBackend.Scoring
{
    [DataContract]
    public class ScoreModelUnit : IComparable<ScoreModelUnit>
    {
        [DataMember(Name = "LipidClass", Order = 0)]
        public LipidClass LipidClass { get; private set; }

        [DataMember(Name = "LipidType", Order = 1)]
        public LipidType LipidType { get; private set; }

        [DataMember(Name = "FragmentDescription", Order = 2)]
        public string FragmentDescription { get; private set; }

        [DataMember(Name = "IonizationMode", Order = 3)]
        public FragmentationMode FragmentationMode { get; private set; }

        [DataMember(Name = "FragmentationType", Order = 4)]
        public FragmentationType FragmentationType { get; private set; }

        [DataMember(Name = "IntensityMax", Order = 5)]
        public double IntensityMax { get; private set; }

        [DataMember(Name = "Probability", Order = 6)]
        public double Probability { get; private set; }

        [DataMember(Name = "ProbabilityNoise", Order = 7)]
        public double ProbabilityNoise { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Empty because items are initialized via ScoreModelSerialization</remarks>
        private ScoreModelUnit()
        {
        }

        public ScoreModelUnit(SpecificFragment specificFragment, double intensityMax, double probability, double probabilityNoise)
        {
            FragmentationMode = specificFragment.FragmentationMode;
            FragmentationType = specificFragment.FragmentationType;
            FragmentDescription = specificFragment.FragmentDescription;
            LipidClass = specificFragment.LipidClass;
            LipidType = specificFragment.LipidType;
            IntensityMax = intensityMax;
            Probability = probability;
            ProbabilityNoise = probabilityNoise;
        }

        protected bool Equals(ScoreModelUnit other)
        {
            return LipidClass == other.LipidClass && LipidType == other.LipidType && string.Equals(FragmentDescription, other.FragmentDescription) && FragmentationMode == other.FragmentationMode && FragmentationType == other.FragmentationType && IntensityMax.Equals(other.IntensityMax);
        }

        public int CompareTo(ScoreModelUnit other)
        {
            if (!LipidClass.Equals(other.LipidClass)) return LipidClass.CompareTo(other.LipidClass);
            if (!LipidType.Equals(other.LipidType)) return LipidType.CompareTo(other.LipidType);
            if (!FragmentDescription.Equals(other.FragmentDescription)) return string.Compare(FragmentDescription, other.FragmentDescription, StringComparison.Ordinal);
            if (!FragmentationMode.Equals(other.FragmentationMode)) return FragmentationMode.CompareTo(other.FragmentationMode);
            if (!FragmentationType.Equals(other.FragmentationType)) return FragmentationType.CompareTo(other.FragmentationType);
            return IntensityMax.CompareTo(other.IntensityMax);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ScoreModelUnit)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)LipidClass;
                hashCode = (hashCode * 397) ^ (int)LipidType;
                hashCode = (hashCode * 397) ^ (FragmentDescription?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)FragmentationMode;
                hashCode = (hashCode * 397) ^ (int)FragmentationType;
                hashCode = (hashCode * 397) ^ IntensityMax.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("LipidClass: {0}, LipidType: {1}, FragmentDescription: {2}, FragmentationMode: {3}, FragmentationType: {4}, IntensityMax: {5}, Probability: {6}, ProbabilityNoise: {7}", LipidClass, LipidType, FragmentDescription, FragmentationMode, FragmentationType, IntensityMax, Probability, ProbabilityNoise);
        }

        public string ToTsvString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", LipidClass, LipidType, FragmentDescription, FragmentationMode, FragmentationType, IntensityMax, Probability, ProbabilityNoise);
        }
    }
}
