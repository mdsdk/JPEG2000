// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using System;
using System.Runtime.InteropServices;

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

        public int SumAbsH => Math.Abs(H_Left) + Math.Abs(H_Right);

        public int SumAbsV => Math.Abs(V_Above) + Math.Abs(V_Below);

        public int SumAbsD => Math.Abs(D_Left_Above) + Math.Abs(D_Right_Above) + Math.Abs(D_Left_Below) + Math.Abs(D_Right_Below);

        public int GetSignificanceCodingContextLabel(SubbandType subbandType)
        {
            if (subbandType.HorizontalFilter == SubbandType.Filter.LowPass)
            {
                return Get_LL_LH_HL_SignificanceCodingContextLabel(SumAbsH, SumAbsV, SumAbsD);
            }
            else if (subbandType.VerticalFilter == SubbandType.Filter.LowPass)
            {
                return Get_LL_LH_HL_SignificanceCodingContextLabel(SumAbsV, SumAbsH, SumAbsD);
            }
            else
            {
                return Get_HH_SignificanceCodingContextLabel(SumAbsH + SumAbsV, SumAbsD);
            }
        }

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

        public int GetRefinementCodingContextLabel()
        {
            return IsZero ? 14 : 15;
        }
    }
}