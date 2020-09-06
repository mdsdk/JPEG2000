// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Diagnostics;

namespace MDSDK.JPEG2000.Model
{
    class ReversibleStepSize
    {
        public int Exponent { get; }

        public ReversibleStepSize(byte b)
        {
            Debug.Assert((b & 0x07) == 0);
            Exponent = b >> 3;
        }

        public override string ToString()
        {
            return $"{GetType().Name}(Exponent={Exponent})";
        }
    }
}
