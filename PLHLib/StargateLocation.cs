using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace PLHLib
{
    public static class Vector3Extensions
    {
        public static double Distance(this Vector3 one, Vector3 two)
        {
            var first = one.X - two.X;
            var second = one.Y - two.Y;
            var third = one.Z - two.Z;
            return Math.Sqrt(first * first + second * second + third * third);
        }
    }

    public static class StargateLocationExtensions
    {
        public static bool HasIntersection(this IEnumerable<StargateLocation> location1, IEnumerable<StargateLocation> location2)
        {
            return
                location1.Any(
                              x =>
                              location2.Any(
                                            y =>
                                            (x.SolarSystemID1 == y.SolarSystemID1 && x.SolarSystemID2 == y.SolarSystemID2)
                                            || (x.SolarSystemID1 == y.SolarSystemID2 && x.SolarSystemID2 == y.SolarSystemID1)));
        }
    }

    public sealed class Vector3
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public Vector3 ()
        {
        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    public sealed class StargateLocation
    {
        private bool Equals(StargateLocation other)
        {
            return SolarSystemID1 == other.SolarSystemID1 && SolarSystemID2 == other.SolarSystemID2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SolarSystemID1 * 397) ^ SolarSystemID2;
            }
        }

        public const double _150_KM = 150000;

        public int SolarSystemID1 { get; set; }

        public int SolarSystemID2 { get; set; }

        public Vector3 Position { get; set; }

        public bool IsInRange(Vector3 pos)
        {
            return Position.Distance(pos) < _150_KM;
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
            return obj is StargateLocation && Equals((StargateLocation) obj);
        }
    }
}