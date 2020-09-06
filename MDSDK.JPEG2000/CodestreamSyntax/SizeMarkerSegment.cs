// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    internal class SizeMarkerSegment : IMarkerSegment
    {
        public ushort R_CapabilitiesOfCodestream { get; private set; }

        public int X_ReferenceGridWidth { get; private set; }

        public int Y_ReferenceGridHeight { get; private set; }

        public int XO_HorizontalOffsetOfImageAreaInReferenceGrid { get; private set; }

        public int YO_VerticalOffsetOfImageAreaInReferenceGrid { get; private set; }

        public int XT_TileWidth { get; private set; }

        public int YT_TileHeight { get; private set; }

        public int XT_HorizontalOffsetOfFirstTileInReferenceGrid { get; private set; }

        public int YTO_VerticalOffsetOfFirstTileInReferenceGrid { get; private set; }

        public int C_NumberOfImageComponents { get; private set; }

        public class ComponentSpecification
        {
            public byte S_PrecisionAndSignOfComponentSamples { get; set; }

            public byte XR_HorizontalSubSamplingFactor { get; set; }

            public byte YR_VerticalSubSamplingFactor { get; set; }

            public static ComponentSpecification ReadFrom(ByteReader input)
            {
                return new ComponentSpecification
                {
                    S_PrecisionAndSignOfComponentSamples = input.ReadByte(),
                    XR_HorizontalSubSamplingFactor = input.ReadByte(),
                    YR_VerticalSubSamplingFactor = input.ReadByte()
                };
            }
        }

        public ComponentSpecification[] ComponentSpecifications { get; private set; }

        public void ReadFrom(CodestreamReader reader)
        {
            var input = reader.Input;

            R_CapabilitiesOfCodestream = BigEndian.ReadUInt16(input);
            X_ReferenceGridWidth = checked((int)BigEndian.ReadUInt32(input));
            Y_ReferenceGridHeight = checked((int)BigEndian.ReadUInt32(input));
            XO_HorizontalOffsetOfImageAreaInReferenceGrid = checked((int)BigEndian.ReadUInt32(input));
            YO_VerticalOffsetOfImageAreaInReferenceGrid = checked((int)BigEndian.ReadUInt32(input));
            XT_TileWidth = checked((int)BigEndian.ReadUInt32(input));
            YT_TileHeight = checked((int)BigEndian.ReadUInt32(input));
            XT_HorizontalOffsetOfFirstTileInReferenceGrid = checked((int)BigEndian.ReadUInt32(input));
            YTO_VerticalOffsetOfFirstTileInReferenceGrid = checked((int)BigEndian.ReadUInt32(input));
            C_NumberOfImageComponents = BigEndian.ReadUInt16(input);

            var componentSpecifications = new ComponentSpecification[C_NumberOfImageComponents];
            for (var i = 0; i < componentSpecifications.Length; i++)
            {
                componentSpecifications[i] = ComponentSpecification.ReadFrom(input);
            }
            ComponentSpecifications = componentSpecifications;

            Debug.Assert(input.AtEnd);
        }
    }
}
