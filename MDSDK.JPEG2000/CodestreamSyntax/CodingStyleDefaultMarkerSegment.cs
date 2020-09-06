// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class CodingStyleDefaultMarkerSegment : CodingStyleMarkerSegment
    {
        public ProgressionOrder SG_ProgressionOrder { get; protected set; }

        public ushort SG_NumberOfLayers { get; protected set; }

        public MultipleComponentTransform SG_MultipleComponentTransform { get; protected set; }

        private void Read_SG_Parameters(ByteReader input)
        {
            SG_ProgressionOrder = (ProgressionOrder)input.ReadByte();
            SG_NumberOfLayers = BigEndian.ReadUInt16(input);
            SG_MultipleComponentTransform = (MultipleComponentTransform)input.ReadByte();
        }

        public override void ReadFrom(CodestreamReader reader)
        {
            var input = reader.Input;

            Read_S_CodingStyle(input);
            Read_SG_Parameters(input);
            Read_SP_Parameters(input);
            
            Debug.Assert(input.AtEnd);
        }
    }
}
