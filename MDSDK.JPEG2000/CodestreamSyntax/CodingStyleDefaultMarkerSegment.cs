// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class CodingStyleDefaultMarkerSegment : CodingStyleMarkerSegment
    {
        public ProgressionOrder SG_ProgressionOrder { get; protected set; }

        public ushort SG_NumberOfLayers { get; protected set; }

        public MultipleComponentTransform SG_MultipleComponentTransform { get; protected set; }

        private void Read_SG_Parameters(BinaryDataReader input)
        {
            SG_ProgressionOrder = (ProgressionOrder)input.ReadByte();
            SG_NumberOfLayers = input.Read<UInt16>();
            SG_MultipleComponentTransform = (MultipleComponentTransform)input.ReadByte();
        }

        public override void ReadFrom(CodestreamReader input)
        {
            var dataReader = input.DataReader;

            Read_S_CodingStyle(dataReader);
            Read_SG_Parameters(dataReader);
            Read_SP_Parameters(dataReader);
            
            Debug.Assert(dataReader.Input.AtEnd);
        }
    }
}
