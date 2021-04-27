// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;
using System;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using MDSDK.BinaryIO;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    abstract class QuantizationMarkerSegment : IMarkerSegment
    {
        public QuantizationStyle QuantizationStyle { get; private set; }

        public int NGuardBits_G { get; private set; }

        public ReversibleStepSize[] ReversibleStepSizes { get; private set; }

        public QuantizationStepSize[] QuantizationStepSizes { get; private set; }

        private void ReadReversibleStepSizes(BinaryDataReader dataReader)
        {
            ReversibleStepSizes = new ReversibleStepSize[dataReader.BytesRemaining];

            for (var i = 0; i < ReversibleStepSizes.Length; i++)
            {
                var b = dataReader.ReadByte();
                ReversibleStepSizes[i] = new ReversibleStepSize(b);
            }
        }

        private void ReadQuantizationStepSizes(BinaryDataReader input)
        {
            QuantizationStepSizes = new QuantizationStepSize[input.GetRemainingDataCount<UInt16>()];

            for (var i = 0; i < QuantizationStepSizes.Length; i++)
            {
                var us = input.Read<UInt16>();
                QuantizationStepSizes[i] = new QuantizationStepSize(us);
            }
        }

        protected void Read_S_QuantizationStyle(BinaryDataReader input)
        {
            var b = input.ReadByte();
            QuantizationStyle = (QuantizationStyle)(b & 0x1F);
            NGuardBits_G = b >> 5;
        }

        protected void Read_SP_Parameters(BinaryDataReader dataReader)
        {
            if (QuantizationStyle == QuantizationStyle.NoQuantization)
            {
                ReadReversibleStepSizes(dataReader);
            }
            else if (QuantizationStyle == QuantizationStyle.ScalarDerived)
            {
                ReadQuantizationStepSizes(dataReader);
            }
            else if (QuantizationStyle == QuantizationStyle.ScalarExpounded)
            {
                ReadQuantizationStepSizes(dataReader);
            }
            else
            {
                throw NotSupported(QuantizationStyle);
            }
        }

        public abstract void ReadFrom(CodestreamReader reader);

        public int GetEb(Subband subband)
        {
            if (ReversibleStepSizes != null)
            {
                return ReversibleStepSizes[subband.SubbandIndex].Exponent;
            }
            else if (QuantizationStepSizes != null)
            {
                return QuantizationStepSizes[subband.SubbandIndex].Exponent;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public int GetMb(Subband subband)
        {
            return NGuardBits_G + GetEb(subband) - 1;
        }
    }
}
