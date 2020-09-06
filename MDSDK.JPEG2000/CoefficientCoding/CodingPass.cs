// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;

namespace MDSDK.JPEG2000.CoefficientCoding
{
    readonly struct CodingPass : IEquatable<CodingPass>
    {
        private int Index { get; }

        private CodingPass(int index)
        {
            Index = index;
        }

        public int BitPlane => (Index + 2) / 3;

        public CodingPassType CodingPassType => (CodingPassType)((Index + 2) % 3);

        public CodingPass PreviousCodingPass => new CodingPass(Index - 1);

        public static IEnumerable<CodingPass> GetCodingPasses(uint nCodingPasses)
        {
            for (var index = 0; index < nCodingPasses; index++)
            {
                yield return new CodingPass(index);
            }
        }

        public bool Equals(CodingPass other)
        {
            return Index == other.Index;
        }

        public override string ToString()
        {
            return $"CodingPass.{Index}.{CodingPassType}.{BitPlane}";
        }

        public static bool operator==(CodingPass a, CodingPass b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CodingPass a, CodingPass b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return (obj is CodingPass other) && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Index;
        }
    }
}
