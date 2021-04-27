// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000.Utils;
using System;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class CodingStyleComponentMarkerSegment : CodingStyleMarkerSegment, IComponentMarkerSegment
    {
        public int C_ComponentIndex { get; private set; }

        private void Read_C_ComponentIndex(BinaryDataReader dataReader, int nComponents)
        {
            if (nComponents > 256)
            {
                C_ComponentIndex = dataReader.Read<UInt16>();
            }
            else
            {
                C_ComponentIndex = dataReader.ReadByte();
            }
        }

        public override void ReadFrom(CodestreamReader input)
        {
            var dataReader = input.DataReader;

            Read_C_ComponentIndex(dataReader, input.Image.Header.SIZ.C_NumberOfImageComponents);
            Read_S_CodingStyle(dataReader);
            Read_SP_Parameters(dataReader);

            Debug.Assert(dataReader.Input.AtEnd);
        }
    }
}
