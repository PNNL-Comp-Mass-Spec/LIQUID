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

		public MsMsSearchUnit(double mz, string description)
		{
			this.Mz = mz;
			this.Description = description;
		}

		public override string ToString()
		{
			return string.Format("Mz: {0}, Description: {1}", Mz, Description);
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
