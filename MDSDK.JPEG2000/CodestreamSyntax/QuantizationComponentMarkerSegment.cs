// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class QuantizationComponentMarkerSegment : QuantizationMarkerSegment, IComponentMarkerSegment
    {
        public int C_ComponentIndex { get; private set; }

        private void Read_C_ComponentIndex(ByteReader input, int nComponents)
        {
            if (nComponents > 256)
            {
                C_ComponentIndex = BigEndian.ReadUInt16(input);
            }
            else
            {
                C_ComponentIndex = input.ReadByte();
            }
        }

        public override void ReadFrom(CodestreamReader reader)
        {
            Read_C_ComponentIndex(reader.Input, reader.Image.Components.Length);
            Read_S_QuantizationStyle(reader.Input);
            Read_SP_Parameters(reader.Input);
        }
    }
}
