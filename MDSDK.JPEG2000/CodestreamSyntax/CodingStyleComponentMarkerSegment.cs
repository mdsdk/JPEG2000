// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class CodingStyleComponentMarkerSegment : CodingStyleMarkerSegment, IComponentMarkerSegment
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
            var input = reader.Input;

            Read_C_ComponentIndex(input, reader.Image.Header.SIZ.C_NumberOfImageComponents);
            Read_S_CodingStyle(input);
            Read_SP_Parameters(input);

            Debug.Assert(input.AtEnd);
        }
    }
}
