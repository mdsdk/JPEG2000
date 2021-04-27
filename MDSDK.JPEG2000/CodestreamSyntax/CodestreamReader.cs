// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000.CoefficientCoding;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    internal class CodestreamReader
    {
        public BufferedStreamReader RawInput { get; }

        public BinaryDataReader DataReader { get; }

        public Image Image { get; }

        public CodestreamReader(BufferedStreamReader input)
        {
            RawInput = input;

            DataReader = new BinaryDataReader(input, ByteOrder.BigEndian);

            var imageHeader = ReadImageHeader();

            Image = new Image(imageHeader);
        }

        private void ReadMarkerSegment(ref Marker marker, List<IMarkerSegment> markerSegments)
        {
            var markerSegment = MarkerSegmentFactory.CreateMarkerSegment(marker);
            if (markerSegment is IgnoredMarkerSegment)
            {
                throw NotSupported(markerSegment);
            }
            ReadMarkerSegment(() => markerSegment.ReadFrom(this));
            markerSegments.Add(markerSegment);
            marker = ReadMarker();
        }

        private ImageHeader ReadImageHeader()
        {
            var marker = ReadMarker();
            if (marker != Marker.SOC)
            {
                throw new IOException($"Expected SOC but got {marker:X4}");
            }

            var markerSegments = new List<IMarkerSegment>();

            marker = ReadMarker();
            while ((marker != Marker.SOT) && (marker != Marker.EOC))
            {
                ReadMarkerSegment(ref marker, markerSegments);
            }

            if (marker != Marker.SOT)
            {
                throw new IOException($"Expected {Marker.SOT} but got {marker}");
            }

            return new ImageHeader(markerSegments);
        }

        private TilePartHeader ReadTilePartHeader()
        {
            var markerSegments = new List<IMarkerSegment>();

            var marker = Marker.SOT;
            do
            {
                ReadMarkerSegment(ref marker, markerSegments);
            }
            while (marker != Marker.SOD);

            return new TilePartHeader(Image, markerSegments);
        }

        public TilePartComponent[] DecodeCodeStream()
        {
            TilePartComponent[] tilePartComponents;

            Marker marker;
            do
            {
                var tilePartStartPosition = DataReader.Input.Position - 2;

                var tilePartHeader = ReadTilePartHeader();

                tilePartComponents = DecodePackets(tilePartHeader);

                var tilePartEndPosition = tilePartStartPosition + tilePartHeader.SOT.P_TilePartLength;
                DataReader.Input.SkipBytes(tilePartEndPosition - DataReader.Input.Position);

                marker = ReadMarker(); 

                ThrowIf(marker != Marker.EOC); // Only single tile part is supported
            }
            while (marker == Marker.SOT);

            if (marker != Marker.EOC)
            {
                throw new IOException($"Expected {Marker.EOC} but got {marker}");
            }

            return tilePartComponents;
        }

        private Marker ReadMarker()
        {
            var b = DataReader.ReadByte();
            if (b != 0xFF)
            {
                throw new IOException($"Expected 0xFF to start marker but got 0x{b:X2}");
            }
            b = DataReader.ReadByte();
            return new Marker((ushort)(0xFF00 | b));
        }

        private void ReadMarkerSegment(Action read)
        {
            var length = DataReader.Read<UInt16>();
            var input = DataReader.Input;
            input.Read(length - 2, () =>
            {
                read.Invoke();
                input.SkipRemainingBytes();
            });
        }

        private void DecodePacketsInLayerResolutionComponentPositionOrder(TilePartHeader tilePartHeader, TilePartComponent[] tilePartComponents)
        {
            var nLayers = tilePartHeader.COD.SG_NumberOfLayers;

            ThrowIf(nLayers != 1);

            var maxNDecompositionLevels = tilePartHeader.COC.Max(coc => coc.SP_NumberOfDecompositionLevels);

            for (var iLayer = 0; iLayer < nLayers; iLayer++)
            {
                for (var iResolution = 0; iResolution <= maxNDecompositionLevels; iResolution++)
                {
                    foreach (var tilePartComponent in tilePartComponents)
                    {
                        var resolutionLevel = tilePartComponent.ResolutionLevels[iResolution];
                        resolutionLevel.GetNPrecincts(out int nPrecinctsX, out int nPrecinctsY);
                        ThrowIf(nPrecinctsX != 1);
                        ThrowIf(nPrecinctsY != 1);
                        DecodePacket(iLayer, resolutionLevel);
                    }
                }
            }
        }

        private TilePartComponent[] DecodePackets(TilePartHeader tilePartHeader)
        {
            var tile = Image.Tiles[tilePartHeader.SOT.I_TileIndexNumber];

            var tilePart = new TilePart(tile, tilePartHeader);

            var tilePartComponents = new TilePartComponent[Image.Components.Length];

            for (var iComponent = 0; iComponent < Image.Components.Length; iComponent++)
            {
                var tileComponent = tile.TileComponents[iComponent];
                tilePartComponents[iComponent] = new TilePartComponent(tilePart, tileComponent);
            }

            var progressionOrder = tilePartHeader.COD.SG_ProgressionOrder;

            if (progressionOrder == ProgressionOrder.LayerResolutionComponentPosition)
            {
                DecodePacketsInLayerResolutionComponentPositionOrder(tilePartHeader, tilePartComponents);
            }
            else
            {
                throw NotSupported(progressionOrder);
            }

            return tilePartComponents;
        }

        private void DecodePacket(int layer, ResolutionLevel resolutionLevel)
        {
            var bitReader = new BitReader(DataReader.Input);

            var packetIsPresent = bitReader.ReadBit() != 0;
            if (packetIsPresent)
            {
                var packetHeader = new PacketHeader(layer, resolutionLevel);
                packetHeader.ReadFrom(bitReader);
                foreach (var codeBlock in packetHeader.IncludedCodeBlocks)
                {
                    StartDecodeCodeBlock(resolutionLevel.TilePartComponent, codeBlock);
                }
            }
        }

        private void StartDecodeCodeBlock(TilePartComponent tilePartComponent, CodeBlock codeBlock)
        {
            if (codeBlock.LengthInBytes > int.MaxValue - 2)
            {
                throw new NotSupportedException($"Code block too long ({codeBlock.LengthInBytes})");
            }

            var bytes = new byte[codeBlock.LengthInBytes + 2];
            DataReader.Read(bytes.AsSpan(0, (int)codeBlock.LengthInBytes));

            // Two 0xFF bytes must be added to flush the entropy coder (see D.4.1 "Expected codestream termination")

            bytes[codeBlock.LengthInBytes] = 0xFF;
            bytes[codeBlock.LengthInBytes + 1] = 0xFF;

            codeBlock.DecodeTask = Task.Run(() =>
            {
                var coefficientDecoder = new CoefficientDecoder(codeBlock, bytes);
                coefficientDecoder.Run(tilePartComponent);
            });
        }
    }
}
