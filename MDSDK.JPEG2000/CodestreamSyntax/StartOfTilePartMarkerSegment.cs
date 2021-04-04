// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    internal class StartOfTilePartMarkerSegment : IMarkerSegment
    {
        public ushort I_TileIndexNumber { get; private set; }

        public uint P_TilePartLength { get; private set; }

        public byte TP_TilePartInstance { get; private set; }

        public byte TN_NumberOfTileParts { get; private set; }

        public void ReadFrom(CodestreamReader reader)
        {
            var input = reader.Input;

            I_TileIndexNumber = input.Read<UInt16>();
            P_TilePartLength = input.Read<UInt32>();
            TP_TilePartInstance = input.ReadByte();
            TN_NumberOfTileParts = input.ReadByte();

            Debug.Assert(input.AtEnd);
        }
    }
}
