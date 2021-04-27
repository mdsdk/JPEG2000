// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.Runtime.CompilerServices;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Utils
{
    class BitReader
    {
        private readonly BufferedStreamReader _input;

        private uint _bits;

        private int _bitCount;

        internal BitReader(BufferedStreamReader input)
        {
            _input = input;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        internal uint ReadBit()
        {
            if (_bitCount == 0)
            {
                var skipNextStuffBit = _bits == 0xFF;
                _bits = _input.ReadByte();
                _bitCount = skipNextStuffBit ? 7 : 8;
            }

            var bit = (_bits >> --_bitCount) & 1;

            return bit;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        internal uint ReadBits(int n)
        {
            if (n > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "must be <= 32");
            }

            uint bits = 0;
            
            while (n-- > 0)
            {
                bits |= ReadBit() << n;
            }

            return bits;
        }
    }
}
