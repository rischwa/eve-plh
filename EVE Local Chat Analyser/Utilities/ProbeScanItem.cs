using System;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Utilities
{
    public interface IProbeScanItem : IDScanItem
    {
        string ScanGroup { get; }
        string Group { get;}
        double Percentage { get; }
        bool IsCosmicSignature { get; }
    }
    public class ProbeScanItem : IProbeScanItem
    {
        public ProbeScanItem(string scanLine)
        {
            if (String.IsNullOrWhiteSpace(scanLine))
            {
                throw new ArgumentException();
            }
            var cells = scanLine.Split('\t');
            if (cells.Length != 6)
            {
                throw new ArgumentException();
            }

            if (!cells[4].EndsWith("%"))
            {
                throw new ArgumentException();
            }
            Percentage = cells[4].Substring(0, cells[4].Length - 1).ToDoubleNormalized();
            ScanGroup = cells[1];
            Group = cells[2];
            Type = cells[3];
            Name = cells[0];
            Distance = new Distance(cells[5]);
        }

        public Distance Distance { get; private set; }
        public string Type { get; private set; }
        public string ScanGroup { get; private set; }
        public string Group { get; private set; }
        public string Name { get; private set; }

        public string ToDScanString()
        {
            throw new NotImplementedException();
        }

        public double Percentage { get; private set; }
        public bool IsCosmicSignature { get { return ScanGroup == "Cosmic Signature" || ScanGroup == "Kosmische Signatur"; } }

        public override string ToString()
        {
            return Name + " (" + Type + ")";
        }

        public bool Equals(IDScanItem other)
        {
            return string.Equals(Type, other.Type) && string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}