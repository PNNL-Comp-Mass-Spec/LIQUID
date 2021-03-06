using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LiquidBackend.Domain
{
    public class AcylChain : IComparable<AcylChain>
    {
        // Ignore Spelling: acyl, Cer, trihydro

        public int NumCarbons { get; }
        public int NumDoubleBonds { get; }
        public int HydroxyPosition { get; }
        public List<int> MethylPositions { get; }
        public int HydroxyCount { get; }
        public int MethylCount { get; }
        public int HydroPeroxideCount { get; }
        public AcylChainType AcylChainType { get; }

        public AcylChain(string acylChainString)
        {
            AcylChainType = AcylChainType.Standard;
            HydroxyPosition = -1;
            MethylPositions = new List<int>();
            HydroxyCount = 0;
            MethylCount = 0;
            var hydroxyMatch = Regex.Match(acylChainString, @"\((\d+)?(OH|\(OH\))\)");
            var hydroPeroxideMatch = Regex.Match(acylChainString, @"\((\d+)?(OOH|OOHOH)\)");
            var methylMatch = Regex.Match(acylChainString, @"\(((\d+(Me|OH)\,?)+)\)");

            if (acylChainString.Contains("F2IsoP-"))
            {
                AcylChainType = AcylChainType.F2IsoP;
                acylChainString = acylChainString.Substring(7);
                HydroxyCount = 3;
            }
            else if(acylChainString.Contains("m"))
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
                // throw new SystemException("Unable to process Trihydro acyl chain. Please use the 2OH format e.g. Cer(d18:0/20:0(2OH))");
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

            if (hydroPeroxideMatch.Success)
            {
                if (Regex.IsMatch(hydroPeroxideMatch.Value, @"\(OOH\)"))
                {
                    AcylChainType = AcylChainType.OOH;
                    HydroPeroxideCount = 1;
                }
                else if (Regex.IsMatch(hydroPeroxideMatch.Value, @"\(OOHOH\)"))
                {
                    AcylChainType = AcylChainType.OOHOH;
                    HydroxyCount = 1;
                    HydroPeroxideCount = 1;
                }
                var regexp = new Regex(@"\((OOH|OOHOH)\)");
                acylChainString = regexp.Replace(acylChainString, "");
            }
            else if (hydroxyMatch.Success)
            {
                AcylChainType = AcylChainType.Hydroxy;
                if (Regex.IsMatch(hydroxyMatch.Value, @"\d+\(OH\)"))
                {
                    var hydroxy = new Regex(@"\(\d+\(OH\)\)");
                    var x = Regex.Match(hydroxyMatch.Value, @"\d+");
                    var successfulParse = int.TryParse(x.Value, out var hydroxyCount);
                    if (successfulParse) HydroxyCount = hydroxyCount;
                    acylChainString = hydroxy.Replace(acylChainString, "");
                }
                else if(Regex.IsMatch(hydroxyMatch.Value, @"\d+OH"))
                {
                    var hydroxy = new Regex(@"\(\d+OH\)");
                    var x = Regex.Match(hydroxyMatch.Value, @"\d+");
                    var successfulParse = int.TryParse(x.Value, out var hydroxyPos);
                    if (successfulParse)
                    {
                        HydroxyPosition = hydroxyPos;
                        HydroxyCount = 1;
                    }
                    acylChainString = hydroxy.Replace(acylChainString, "");
                }
                else
                {
                    var hydroxy = new Regex(@"\(OH\)");
                    HydroxyCount = 1;
                    acylChainString = hydroxy.Replace(acylChainString, "");
                }
            }
            else if (methylMatch.Success)
            {
                var methylGroups = methylMatch.Groups[1].Value.Split(',');
                foreach (var m in methylGroups)
                {
                    var x = Regex.Match(m, @"\d+");
                    var successfulParse = int.TryParse(x.Value, out var pos);
                    if (successfulParse)
                    {
                        if (Regex.IsMatch(m, @"\d+Me"))
                        {
                            MethylPositions.Add(pos);
                            MethylCount++;
                        }
                        else if (Regex.IsMatch(m, @"\d+OH"))
                        {
                            HydroxyPosition = pos;
                            HydroxyCount++;
                        }
                    }
                }
                acylChainString = acylChainString.Replace(methylMatch.Groups[0].Value, "");
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

            NumCarbons = int.Parse(splitString[0]) + MethylCount;
            NumDoubleBonds = int.Parse(splitString[1]);
        }

        public override string ToString()
        {
            var carbonDoubleBond = NumCarbons + ":" + NumDoubleBonds;

            if (AcylChainType == AcylChainType.Plasmalogen) return "P-" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Ether) return "O-" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.OxoCHO) return carbonDoubleBond + "(CHO)";
            if (AcylChainType == AcylChainType.OxoCOOH) return carbonDoubleBond + "(COOH)";
            if (AcylChainType == AcylChainType.Monohydro) return "m" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Dihydro) return "d" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.Trihydro) return "t" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.F2IsoP) return "F2IsoP" + carbonDoubleBond;
            if (AcylChainType == AcylChainType.OOH) return carbonDoubleBond + "(OOH)";
            if (AcylChainType == AcylChainType.OOHOH) return carbonDoubleBond + "(OOHOH)";
            if (AcylChainType == AcylChainType.Hydroxy)
            {
                if (HydroxyPosition < 0) return carbonDoubleBond + "(OH)";
                return carbonDoubleBond + string.Format("({0}OH)", HydroxyPosition);
            }
            if (MethylCount > 0)
            {
                var end = "(";
                foreach (var pos in MethylPositions)
                {
                    end += string.Format("{0}Me,", pos);
                }
                if (HydroxyPosition > 0)
                {
                    end += string.Format("{0}OH,", HydroxyPosition);
                }
                end += ")";
                return carbonDoubleBond + end;
            }
            if (AcylChainType == AcylChainType.Standard) return carbonDoubleBond;
            throw new SystemException("Unknown AcylChainType for given AcylChain");
        }

        protected bool Equals(AcylChain other)
        {
            return NumCarbons == other.NumCarbons && NumDoubleBonds == other.NumDoubleBonds && AcylChainType == other.AcylChainType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((AcylChain) obj);
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
