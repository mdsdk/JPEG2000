// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.Utils
{
    internal static class BigEndian
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(ByteReader input)
        {
            return (short)((input.ReadByte() << 8) | (input.ReadByte() << 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16(ByteReader input)
        {
            return (ushort)(input.ReadByte() << 8 | (input.ReadByte() << 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(ByteReader input)
        {
            return 0
                | (input.ReadByte() << 3 * 8)
                | (input.ReadByte() << 2 * 8)
                | (input.ReadByte() << 1 * 8)
                | (input.ReadByte() << 0 * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32(byte b0, byte b1, byte b2, byte b3)
        {
            return 0
                | ((uint)b0 << 3 * 8)
                | ((uint)b1 << 2 * 8)
                | ((uint)b2 << 1 * 8)
                | ((uint)b3 << 0 * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32(Span<byte> b)
        {
            return ToUInt32(b[0], b[1], b[2], b[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte R(ByteReader i) => i.ReadByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(ByteReader i)
        {
            return ToUInt32(R(i), R(i), R(i), R(i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(ByteReader input)
        {
            return 0
                | ((long)input.ReadByte() << 7 * 8)
                | ((long)input.ReadByte() << 6 * 8)
                | ((long)input.ReadByte() << 5 * 8)
                | ((long)input.ReadByte() << 4 * 8)
                | ((long)input.ReadByte() << 3 * 8)
                | ((long)input.ReadByte() << 2 * 8)
                | ((long)input.ReadByte() << 1 * 8)
                | ((long)input.ReadByte() << 0 * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64(ByteReader input)
        {
            return 0
                | ((ulong)input.ReadByte() << 7 * 8)
                | ((ulong)input.ReadByte() << 6 * 8) 
                | ((ulong)input.ReadByte() << 5 * 8) 
                | ((ulong)input.ReadByte() << 4 * 8)
                | ((ulong)input.ReadByte() << 3 * 8)
                | ((ulong)input.ReadByte() << 2 * 8)
                | ((ulong)input.ReadByte() << 1 * 8)
                | ((ulong)input.ReadByte() << 0 * 8);
        }
    }
}
