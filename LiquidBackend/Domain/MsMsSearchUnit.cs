using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidBackend.Domain
{
	public class MsMsSearchUnit
	{
		public double Mz { get; private set; }
		public string Description { get; private set; }
		public AcylChain AcylChain { get; private set; }
		public bool IsDiagnostic { get; private set; }

		public string DescriptionForUi
		{
			get
			{
				if (this.AcylChain == null) return this.Description;
				return this.Description + " (" + this.AcylChain.ToString() + ")";
			}
		}

		public MsMsSearchUnit(double mz, string description, bool isDiagnostic = false)
		{
			this.Mz = mz;
			this.Description = description;
			this.AcylChain = null;
			this.IsDiagnostic = isDiagnostic;
		}

		public MsMsSearchUnit(double mz, string description, AcylChain acylChain, bool isDiagnostic = false)
		{
			this.Mz = mz;
			this.Description = description;
			this.AcylChain = acylChain;
			this.IsDiagnostic = isDiagnostic;
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
			if (obj.GetType() != this.GetType()) return false;
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
