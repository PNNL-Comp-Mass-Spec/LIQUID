using System;

namespace LiquidBackend.Domain
{
	public class MsMsSearchUnit
	{
		public double Mz { get; }
		public string Description { get; }
		public AcylChain AcylChain { get; }
		public bool IsDiagnostic { get; }

		public string DescriptionForUi
		{
			get
			{
				if (AcylChain == null) return Description;
				return Description + " (" + AcylChain + ")";
			}
		}

		public MsMsSearchUnit(double mz, string description, bool isDiagnostic = false)
		{
			Mz = mz;
			Description = description;
			AcylChain = null;
			IsDiagnostic = isDiagnostic;
		}

		public MsMsSearchUnit(double mz, string description, AcylChain acylChain, bool isDiagnostic = false)
		{
			Mz = mz;
			Description = description;
			AcylChain = acylChain;
			IsDiagnostic = isDiagnostic;
		}

		public override string ToString()
		{
			return string.Format("Mz: {0}, Description: {1}, AcylChain: {2}, IsDiagnostic: {3}", Mz, Description, AcylChain, IsDiagnostic);
		}

		protected bool Equals(MsMsSearchUnit other)
		{
			return Math.Abs(Mz - other.Mz) < 1e-9 && string.Equals(Description, other.Description);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((MsMsSearchUnit) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Mz.GetHashCode()*397) ^ (Description != null ? Description.GetHashCode() : 0);
			}
		}
	}
}
