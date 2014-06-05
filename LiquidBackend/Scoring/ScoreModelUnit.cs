using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

		private ScoreModelUnit()
		{
			
		}

		public ScoreModelUnit(SpecificFragment specificFragment, double intensityMax, double probability, double probabilityNoise)
		{
			this.FragmentationMode = specificFragment.FragmentationMode;
			this.FragmentationType = specificFragment.FragmentationType;
			this.FragmentDescription = specificFragment.FragmentDescription;
			this.LipidClass = specificFragment.LipidClass;
			this.LipidType = specificFragment.LipidType;
			this.IntensityMax = intensityMax;
			this.Probability = probability;
			this.ProbabilityNoise = probabilityNoise;
		}

		protected bool Equals(ScoreModelUnit other)
		{
			return LipidClass == other.LipidClass && LipidType == other.LipidType && string.Equals(FragmentDescription, other.FragmentDescription) && FragmentationMode == other.FragmentationMode && FragmentationType == other.FragmentationType && IntensityMax.Equals(other.IntensityMax);
		}

		public int CompareTo(ScoreModelUnit other)
		{
			if (!this.LipidClass.Equals(other.LipidClass)) return this.LipidClass.CompareTo(other.LipidClass);
			if (!this.LipidType.Equals(other.LipidType)) return this.LipidType.CompareTo(other.LipidType);
			if (!this.FragmentDescription.Equals(other.FragmentDescription)) return String.Compare(this.FragmentDescription, other.FragmentDescription, StringComparison.Ordinal);
			if (!this.FragmentationMode.Equals(other.FragmentationMode)) return this.FragmentationMode.CompareTo(other.FragmentationMode);
			if (!this.FragmentationType.Equals(other.FragmentationType)) return this.FragmentationType.CompareTo(other.FragmentationType);
			return this.IntensityMax.CompareTo(other.IntensityMax);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ScoreModelUnit) obj);
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
				hashCode = (hashCode*397) ^ IntensityMax.GetHashCode();
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
