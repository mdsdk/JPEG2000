// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.Utils
{
    internal static class StaticInclude
    {
        public const MethodImplOptions AggressiveMethodImplOptions = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

        public static void ThrowIf(bool condition, [CallerArgumentExpression("condition")] string callerArgumentExpression = null)
        {
            if (condition)
            {
                throw new Exception(callerArgumentExpression);
            }
        }

        public static Exception NotSupported(object obj)
        {
            return new NotSupportedException((obj == null) ? "<null>" : obj.ToString());
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CeilDiv(double dividend, double divisor)
        {
            return Math.Ceiling(dividend / divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilDiv(int dividend, int divisor)
        {
            return (int)CeilDiv(dividend, (double)divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FloorDiv(double dividend, double divisor)
        {
            return Math.Floor(dividend / divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloorDiv(float dividend, float divisor)
        {
            return (float)Math.Floor(dividend / divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorDiv(int dividend, int divisor)
        {
            return (int)FloorDiv(dividend, (double)divisor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorLog2(uint n)
        {
            var floorLog2 = 0;
            while (n >= 2)
            {
                floorLog2++;
                n >>= 1;
            }
            return floorLog2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(int i)
        {
            return (i & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(int i)
        {
            return (i & 1) != 0;
        }
    }
}
