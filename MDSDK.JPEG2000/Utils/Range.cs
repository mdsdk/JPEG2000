// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.Utils
{
    readonly struct Range : IEquatable<Range>
    {
        public int Start { get; }

        public int Length { get; }

        public Range(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int End => Start + Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBounds(out int start, out int end)
        {
            start = Start;
            end = End;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int i, out int offset)
        {
            offset = i - Start;
            return (offset >= 0) && (offset < Length);
        }

        public override string ToString()
        {
            return $"[{Start},{Start + Length})";
        }

        public static Range FromBounds(int start, int end)
        {
            return new Range(start, end - start);
        }

        public bool Equals(Range other)
        {
            return (Start == other.Start) && (Length == other.Length);
        }

        public override bool Equals(object obj)
        {
            return (obj is Range other) && Equals(other);
        }

        public override int GetHashCode()
        {
            return new Tuple<int, int>(Start, Length).GetHashCode();
        }

        public static bool operator ==(Range a, Range b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Range a, Range b)
        {
            return !a.Equals(b);
        }
    }
}
