// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.CoefficientCoding
{
    [StructLayout(LayoutKind.Explicit)]
    struct ContextVector
    {
        [FieldOffset(0)] public sbyte H_Left;
        [FieldOffset(1)] public sbyte H_Right;
        [FieldOffset(2)] public sbyte V_Above;
        [FieldOffset(3)] public sbyte V_Below;
        [FieldOffset(4)] public sbyte D_Left_Above;
        [FieldOffset(5)] public sbyte D_Right_Above;
        [FieldOffset(6)] public sbyte D_Left_Below;
        [FieldOffset(7)] public sbyte D_Right_Below;

        [FieldOffset(0)] public long IsZeroChecker;

        [MethodImpl(HotspotMethodImplOptions)]
        private static int Get_LL_LH_HL_SignificanceCodingContextLabel(int sumAbsHVa, int sumAbsHVb, int sumAbsD)
        {
            return sumAbsHVa switch
            {
                0 => sumAbsHVb switch
                {
                    0 => sumAbsD switch
                    {
                        0 => 0,
                        1 => 1,
                        _ => 2
                    },
                    1 => 3,
                    _ => 4
                },
                1 => sumAbsHVb switch
                {
                    0 => sumAbsD switch
                    {
                        0 => 5,
                        _ => 6
                    },
                    _ => 7
                },
                _ => 8
            };
        }

        private static int[,,] Make_LL_LH_HL_SignificanceCodingContextLabelLookupTable()
        {
            var lookupTable = new int[3, 3, 5];
            for (var sumAbsHVa = 0; sumAbsHVa <= 2; sumAbsHVa++)
            {
                for (var sumAbsHVb = 0; sumAbsHVb <= 2; sumAbsHVb++)
                {
                    for (var sumAbsD = 0; sumAbsD <= 4; sumAbsD++)
                    {
                        lookupTable[sumAbsHVa, sumAbsHVb, sumAbsD] = Get_LL_LH_HL_SignificanceCodingContextLabel(sumAbsHVa, sumAbsHVb, sumAbsD);
                    }
                }
            }
            return lookupTable;
        }

        private static int[,,] LL_LH_HL_SignificanceCodingContextLabelLookupTable = Make_LL_LH_HL_SignificanceCodingContextLabelLookupTable();

        [MethodImpl(HotspotMethodImplOptions)]
        public static int Get_HH_SignificanceCodingContextLabel(int sumAbsHV, int sumAbsD)
        {
            return sumAbsD switch
            {
                0 => sumAbsHV switch
                {
                    0 => 0,
                    1 => 1,
                    _ => 2
                },
                1 => sumAbsHV switch
                {
                    0 => 3,
                    1 => 4,
                    _ => 5,
                },
                2 => sumAbsHV switch
                {
                    0 => 6,
                    _ => 7,
                },
                _ => 8
            };
        }

        private static int[,] Make_HH_SignificanceCodingContextLabelLookupTable()
        {
            var lookupTable = new int[5, 5];
            for (var sumAbsHV = 0; sumAbsHV <= 4; sumAbsHV++)
            {
                for (var sumAbsD = 0; sumAbsD <= 4; sumAbsD++)
                {
                    lookupTable[sumAbsHV, sumAbsD] = Get_HH_SignificanceCodingContextLabel(sumAbsHV, sumAbsD);
                }
            }
            return lookupTable;
        }

        private static int[,] HH_SignificanceCodingContextLabelLookupTable = Make_HH_SignificanceCodingContextLabelLookupTable();

        private static int AbsSum(sbyte a, sbyte b) => (a & 1) + (b & 1);

        private static int AbsSum(sbyte a, sbyte b, sbyte c, sbyte d) => (a & 1) + (b & 1) + (c & 1) + (d & 1);

        private int SumAbsH => AbsSum(H_Left, H_Right);

        private int SumAbsV => AbsSum(V_Above, V_Below);

        private int SumAbsHV => AbsSum(H_Left, H_Right, V_Above, V_Below);

        private int SumAbsD => AbsSum(D_Left_Above, D_Right_Above, D_Left_Below, D_Right_Below);

        [MethodImpl(HotspotMethodImplOptions)]
        public int GetSignificanceCodingContextLabel(SubbandType subbandType)
        {
            if (subbandType.HorizontalFilter == SubbandType.Filter.LowPass)
            {
                return LL_LH_HL_SignificanceCodingContextLabelLookupTable[SumAbsH, SumAbsV, SumAbsD];
            }
            else if (subbandType.VerticalFilter == SubbandType.Filter.LowPass)
            {
                return LL_LH_HL_SignificanceCodingContextLabelLookupTable[SumAbsV, SumAbsH, SumAbsD];
            }
            else
            {
                return HH_SignificanceCodingContextLabelLookupTable[SumAbsHV, SumAbsD];
            }
        }

        [MethodImpl(HotspotMethodImplOptions)]
        public void GetSignCodingContextLabelAndXorBit(out int contextLabel, out int xorBit)
        {
            var hContribution = (H_Left == H_Right) ? H_Left : H_Left + H_Right;
            var vContribution = (V_Above == V_Below) ? V_Above : V_Above + V_Below;

            var offset = 3 * hContribution + vContribution;
            if (offset < 0)
            {
                contextLabel = 9 - offset;
                xorBit = 1;
            }
            else
            {
                contextLabel = 9 + offset;
                xorBit = 0;
            }
        }

        public bool IsZero => IsZeroChecker == 0;

        [MethodImpl(HotspotMethodImplOptions)]
        public int GetRefinementCodingContextLabel()
        {
            return IsZero ? 14 : 15;
        }
    }
}