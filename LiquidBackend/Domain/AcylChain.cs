using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
	public class AcylChain : IComparable<AcylChain>
	{
		public int NumCarbons { get; private set; }
		public int NumDoubleBonds { get; private set; }
        public int HydroxyPosition { get; private set; }
        public int HydroxyCount { get; private set; }
		public AcylChainType AcylChainType { get; private set; }

		public AcylChain(string acylChainString)
		{
			this.AcylChainType = AcylChainType.Standard;
		    this.HydroxyPosition = -1;
		    this.HydroxyCount = 0;
            Match hydroxyMatch = Regex.Match(acylChainString, @"\((\d+)?(OH|\(OH\))\)");

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
				//throw new SystemException("Unable to process Trihydro acyl chain. Please use the 2OH format e.g. Cer(d18:0/20:0(2OH))");
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

			if (hydroxyMatch.Success)
			{
				this.AcylChainType = AcylChainType.Hydroxy;
			    int hydroxyPos;
			    int hydroxyCount;
			    if (Regex.IsMatch(hydroxyMatch.Value, @"\d+\(OH\)"))
			    {
                    Regex hydroxy = new Regex(@"\(\d+\(OH\)\)");
			        var x = Regex.Match(hydroxyMatch.Value, @"\d+");
			        bool successfulParse = Int32.TryParse(x.Value, out hydroxyCount);
                    if (successfulParse) this.HydroxyCount = hydroxyCount;
                    acylChainString = hydroxy.Replace(acylChainString, "");
			    }
			    else
			    {
                    Regex hydroxy = new Regex(@"\(\d+OH\)");
                    var x = Regex.Match(hydroxyMatch.Value, @"\d+");
			        bool successfulParse = Int32.TryParse(x.Value, out hydroxyPos);
			        if (successfulParse)
			        {
			            this.HydroxyPosition = hydroxyPos;
			            this.HydroxyCount = 1;
			        }
                    acylChainString = hydroxy.Replace(acylChainString, "");
			    }
			    
			}

            else if (acylChainString.Contains("(CHO)"))
            {
                this.AcylChainType = AcylChainType.OxoCHO;
                acylChainString = acylChainString.Replace("(CHO)", "");
            }
            else if (acylChainString.Contains("(COOH)"))
            {
                this.AcylChainType = AcylChainType.OxoCOOH;
                acylChainString = acylChainString.Replace("(COOH)", ""); 
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
		    if (AcylChainType == AcylChainType.OxoCHO) return carbonDoubleBond + "(CHO)";
            if (AcylChainType == AcylChainType.OxoCOOH) return carbonDoubleBond + "(COOH)";
			if (AcylChainType == AcylChainType.Monohydro) return "m" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Dihydro) return "d" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Trihydro) return "t" + carbonDoubleBond;
			if (AcylChainType == AcylChainType.Hydroxy) return carbonDoubleBond + String.Format("({0}OH)",this.HydroxyPosition);

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

		public int CompareTo(AcylChain other)
		{
			if (this.NumCarbons != other.NumCarbons) return this.NumCarbons.CompareTo(other.NumCarbons);
			if (this.NumDoubleBonds != other.NumDoubleBonds) return this.NumDoubleBonds.CompareTo(other.NumDoubleBonds);
            if (this.HydroxyPosition != other.HydroxyPosition) return this.HydroxyPosition.CompareTo(other.HydroxyPosition);
			return this.AcylChainType.CompareTo(this.AcylChainType);
		}
	}
}
