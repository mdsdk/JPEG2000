// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.CoefficientCoding
{
    class Coefficient
    {
        public sbyte Sign;

        public int MagnitudeBits;

        public int LastScannedInBitPlane;

        public ContextVector ContextVector;

        [MethodImpl(CoefficientDecoder.HotspotMethodImplOptions)]
        public Coefficient()
        {
            LastScannedInBitPlane = -1;
        }

        [MethodImpl(CoefficientDecoder.HotspotMethodImplOptions)]
        public void ApplySignificance(sbyte sign, int bitPlane)
        {
            Sign = sign;
            if (sign != 0)
            {
                Debug.Assert(MagnitudeBits == 0);
                MagnitudeBits = 1;
            }
            LastScannedInBitPlane = bitPlane;
        }

        [MethodImpl(CoefficientDecoder.HotspotMethodImplOptions)]
        public void ApplyRefinement(int magnitudeBit, int bitPlane)
        {
            MagnitudeBits = (MagnitudeBits << 1) | magnitudeBit;
            LastScannedInBitPlane = bitPlane;
        }

        [MethodImpl(CoefficientDecoder.HotspotMethodImplOptions)]
        public int GetValue()
        {
            return Sign * MagnitudeBits;
        }
    }
}
