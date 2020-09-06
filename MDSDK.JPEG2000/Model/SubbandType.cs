// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.JPEG2000.Model
{
    readonly struct SubbandType : IEquatable<SubbandType>
    {
        public enum Filter : byte
        {
            LowPass,
            HighPass
        }

        private static string ToString(Filter filter)
        {
            return filter switch
            {
                Filter.LowPass => "L",
                Filter.HighPass => "H",
                _ => throw new NotSupportedException()
            };
        }

        public Filter HorizontalFilter { get; }

        public Filter VerticalFilter { get; }

        private SubbandType(Filter horizontalFilter, Filter verticalFilter)
        {
            HorizontalFilter = horizontalFilter;
            VerticalFilter = verticalFilter;
        }

        public override string ToString() => ToString(HorizontalFilter) + ToString(VerticalFilter);

        public bool Equals(SubbandType other) => (HorizontalFilter == other.HorizontalFilter) && (VerticalFilter == other.VerticalFilter);

        public override bool Equals(object obj) => (obj is SubbandType other) && Equals(other);

        public override int GetHashCode() => throw new NotSupportedException();

        public static bool operator==(SubbandType a, SubbandType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SubbandType a, SubbandType b)
        {
            return !a.Equals(b);
        }

        public static readonly SubbandType LL = new SubbandType(Filter.LowPass, Filter.LowPass);
        public static readonly SubbandType HL = new SubbandType(Filter.HighPass, Filter.LowPass);
        public static readonly SubbandType LH = new SubbandType(Filter.LowPass, Filter.HighPass);
        public static readonly SubbandType HH = new SubbandType(Filter.HighPass, Filter.HighPass);
    }
}
