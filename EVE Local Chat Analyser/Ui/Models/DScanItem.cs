using System;

namespace EveLocalChatAnalyser.Ui.Models
{
    
    public interface IDScanItem : IEquatable<IDScanItem>
    {
        Distance Distance { get; }
        string Type { get; }
        string Name { get; }

        string ToDScanString();
    }

    

    public class DScanItem : IDScanItem
    {
        public DScanItem(){}
        public bool Equals(IDScanItem other)
        {
            return string.Equals(Type, other.Type) && string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public DScanItem(string dscanLine)
        {
            if (String.IsNullOrWhiteSpace(dscanLine))
            {
                throw new ArgumentException();
            }
            var cells = dscanLine.Split('\t');
            if (cells.Length != 4)
            {
                throw new ArgumentException();
            }

            //in the german client everything ends with *
            Name = cells[1];
            Type = cells[2].EndsWith("*") ? cells[2].Substring(0, cells[2].Length - 1) : cells[2];
            Distance = new Distance(cells[3]);
        }

        public Distance Distance { get;  set; }

        public string Type { get;  set; }

        public string Name { get;  set; }

        public string ToDScanString()
        {
            return string.Format("{0}\t{1}\t{2}", Name, Type, Distance);
        }

        public override string ToString()
        {
            return Name + " (" + Type + ")";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DScanItem) obj);
        }
    }
}