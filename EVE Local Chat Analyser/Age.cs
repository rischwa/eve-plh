using System;

namespace EveLocalChatAnalyser
{
    public class Age
    {
        private readonly int _months;

        private readonly int _years;

        public Age(DateTime dateOfBirth)
        {
            var now = DateTime.UtcNow;
            if (dateOfBirth > now)
            {
                throw new ArgumentException("Value lies in the future, this is not allowed", "dateOfBirth");
            }

            var yearDifference = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month)
            {
                --yearDifference;
            }
            _years = yearDifference;

            var monthDifference = now.Month - dateOfBirth.Month;
            if (monthDifference < 0)
            {
                monthDifference = 12 + monthDifference;
            }

            _months = monthDifference;
        }

        public int Months
        {
            get { return _months; }
        }

        public int Years
        {
            get { return _years; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Age) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_months*397) ^ _years;
            }
        }

        public override string ToString()
        {
            return _years + "y " + _months + "m";
        }

        public static bool operator ==(Age left, Age right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Age left, Age right)
        {
            return !Equals(left, right);
        }

        protected bool Equals(Age other)
        {
            return _months == other._months && _years == other._years;
        }
    }
}