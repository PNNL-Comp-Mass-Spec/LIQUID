﻿using System;

namespace LiquidBackend.Domain
{
	public class AcylChain
	{
		public int NumCarbons { get; private set; }
		public int NumDoubleBonds { get; private set; }
		public AcylChainType AcylChainType { get; private set; }

		public AcylChain(string acylChainString)
		{
			this.AcylChainType = AcylChainType.Standard;

			if (acylChainString.Contains("m"))
			{
				this.AcylChainType = AcylChainType.Monohydro;
				acylChainString = acylChainString.Substring(1);
			}
			else if (acylChainString.Contains("d"))
			{
				this.AcylChainType = AcylChainType.Dihydro;
				acylChainString = acylChainString.Substring(1);
			}
			else if (acylChainString.Contains("t"))
			{
				this.AcylChainType = AcylChainType.Trihydro;
				acylChainString = acylChainString.Substring(1);
			}
			else if (acylChainString.Contains("O-"))
			{
				this.AcylChainType = AcylChainType.Ether;
				acylChainString = acylChainString.Substring(2);
			}
			else if (acylChainString.Contains("P-"))
			{
				this.AcylChainType = AcylChainType.Plasmalogen;
				acylChainString = acylChainString.Substring(2);
			}

			string[] splitString = acylChainString.Split(':');

			this.NumCarbons = int.Parse(splitString[0]);
			this.NumDoubleBonds = int.Parse(splitString[1]);
		}

		public override string ToString()
		{
			string carbonDoubleBond = NumCarbons + ":" + NumDoubleBonds;

			if (AcylChainType == AcylChainType.Standard) return carbonDoubleBond;
			if (AcylChainType == AcylChainType.Plasmalogen) return "P-" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Ether) return "O-" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Monohydro) return "m" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Dihydro) return "d" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Trihydro) return "t" + carbonDoubleBond;

			throw new SystemException("Unknown AcylChainType for given AcylChain");
		}

		protected bool Equals(AcylChain other)
		{
			return NumCarbons == other.NumCarbons && NumDoubleBonds == other.NumDoubleBonds && AcylChainType == other.AcylChainType;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((AcylChain) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = NumCarbons;
				hashCode = (hashCode*397) ^ NumDoubleBonds;
				hashCode = (hashCode*397) ^ (int) AcylChainType;
				return hashCode;
			}
		}
	}
}
