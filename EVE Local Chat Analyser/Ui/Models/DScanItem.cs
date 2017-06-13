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
            if (cells.Length != 3)
            {
                throw new ArgumentException();
            }

            //in the german client everything ends with *
            Name = cells[0];
            Type = cells[1].EndsWith("*") ? cells[1].Substring(0, cells[1].Length - 1) : cells[1];
            Distance = new Distance(cells[2]);
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