using System;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
    public class AcylChain : IComparable<AcylChain>
    {
        public int NumCarbons { get; }
        public int NumDoubleBonds { get; }
        public int HydroxyPosition { get; }
        public int HydroxyCount { get; }
        public AcylChainType AcylChainType { get; }

        public AcylChain(string acylChainString)
        {
            AcylChainType = AcylChainType.Standard;
            HydroxyPosition = -1;
            HydroxyCount = 0;
            var hydroxyMatch = Regex.Match(acylChainString, @"\((\d+)?(OH|\(OH\))\)");

            if (acylChainString.Contains("m"))
            {
                AcylChainType = AcylChainType.Monohydro;
                acylChainString = acylChainString.Substring(1);
            }
            else if (acylChainString.Contains("d"))
            {
                AcylChainType = AcylChainType.Dihydro;
                acylChainString = acylChainString.Substring(1);
            }
            else if (acylChainString.Contains("t"))
            {
                AcylChainType = AcylChainType.Trihydro;
                acylChainString = acylChainString.Substring(1);
                //throw new SystemException("Unable to process Trihydro acyl chain. Please use the 2OH format e.g. Cer(d18:0/20:0(2OH))");
            }
            else if (acylChainString.Contains("O-"))
            {
                AcylChainType = AcylChainType.Ether;
                acylChainString = acylChainString.Substring(2);
            }
            else if (acylChainString.Contains("P-"))
            {
                AcylChainType = AcylChainType.Plasmalogen;
                acylChainString = acylChainString.Substring(2);
            }

            if (hydroxyMatch.Success)
            {
                AcylChainType = AcylChainType.Hydroxy;
                if (Regex.IsMatch(hydroxyMatch.Value, @"\d+\(OH\)"))
                {
                    var hydroxy = new Regex(@"\(\d+\(OH\)\)");
                    var x = Regex.Match(hydroxyMatch.Value, @"\d+");
                    int hydroxyCount;
                    var successfulParse = Int32.TryParse(x.Value, out hydroxyCount);
                    if (successfulParse) HydroxyCount = hydroxyCount;
                    acylChainString = hydroxy.Replace(acylChainString, "");
                }
                else
                {
                    var hydroxy = new Regex(@"\(\d+OH\)");
                    var x = Regex.Match(hydroxyMatch.Value, @"\d+");
                    int hydroxyPos;
                    var successfulParse = Int32.TryParse(x.Value, out hydroxyPos);
                    if (successfulParse)
                    {
                        HydroxyPosition = hydroxyPos;
                        HydroxyCount = 1;
                    }
                    acylChainString = hydroxy.Replace(acylChainString, "");
                }

            }

            else if (acylChainString.Contains("(CHO)"))
            {
                AcylChainType = AcylChainType.OxoCHO;
                acylChainString = acylChainString.Replace("(CHO)", "");
            }
            else if (acylChainString.Contains("(COOH)"))
            {
                AcylChainType = AcylChainType.OxoCOOH;
                acylChainString = acylChainString.Replace("(COOH)", "");
            }

            var splitString = acylChainString.Split(':');

            NumCarbons = int.Parse(splitString[0]);
            NumDoubleBonds = int.Parse(splitString[1]);
        }

        public override string ToString()
        {
            var carbonDoubleBond = NumCarbons + ":" + NumDoubleBonds;

            if (AcylChainType == AcylChainType.Standard) return carbonDoubleBond;
            if (AcylChainType == AcylChainType.Plasmalogen) return "P-" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Ether) return "O-" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.OxoCHO) return carbonDoubleBond + "(CHO)";
            if (AcylChainType == AcylChainType.OxoCOOH) return carbonDoubleBond + "(COOH)";
            if (AcylChainType == AcylChainType.Monohydro) return "m" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Dihydro) return "d" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Trihydro) return "t" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Hydroxy) return carbonDoubleBond + string.Format("({0}OH)",HydroxyPosition);

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
            if (obj.GetType() != GetType()) return false;
            return Equals((AcylChain) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = NumCarbons;
                hashCode = (hashCode*397) ^ NumDoubleBonds;
                hashCode = (hashCode*397) ^ (int) AcylChainType;
                return hashCode;
            }
        }

        public int CompareTo(AcylChain other)
        {
            if (NumCarbons != other.NumCarbons) return NumCarbons.CompareTo(other.NumCarbons);
            if (NumDoubleBonds != other.NumDoubleBonds) return NumDoubleBonds.CompareTo(other.NumDoubleBonds);
            if (HydroxyPosition != other.HydroxyPosition) return HydroxyPosition.CompareTo(other.HydroxyPosition);
            return AcylChainType.CompareTo(AcylChainType);
        }
    }
}
