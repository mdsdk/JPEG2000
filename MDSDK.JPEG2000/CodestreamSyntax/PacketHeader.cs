// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;
using MDSDK.JPEG2000.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MDSDK.JPEG2000.Model;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class PacketHeader
    {
        public int Layer { get; }
        
        public ResolutionLevel ResolutionLevel { get; }

        public PacketHeader(int layer, ResolutionLevel resolutionLevel)
        {
            Layer = layer;
            ResolutionLevel = resolutionLevel;
        }

        public List<CodeBlock> IncludedCodeBlocks { get; } = new List<CodeBlock>();

        public void ReadFrom(BitReader bitReader)
        {
            var tilePartComponent = ResolutionLevel.TilePartComponent;

            var iComponent = tilePartComponent.TileComponent.Component.ComponentIndex;
            var iResolution = ResolutionLevel.ResolutionIndex;

            var subbands = tilePartComponent.TilePart.Header.Subbands[iComponent][iResolution];
            foreach (var subband in subbands)
            { 
                ReadSubBand(bitReader, subband);
            }
        }

        private void ReadSubBand(BitReader bitReader, Subband subband)
        {
            var subbandPrecinct = ResolutionLevel.GetSubbandPrecinct(subband);

            var nCodeBlocksX = subbandPrecinct.NCodeBlocksX;
            var nCodeBlocksY = subbandPrecinct.NCodeBlocksY;

            var codeBlockInclusionTagTree = subbandPrecinct.CodeBlockInclusionTagTree;

            for (var y = 0; y < nCodeBlocksY; y++)
            {
                for (var x = 0; x < nCodeBlocksX; x++)
                {
                    var codeBlockFirstIncludedInLayerIsValid = codeBlockInclusionTagTree.TryGetValue((uint)x, (uint)y, 
                        (uint)Layer, bitReader, out uint codeBlockFirstIncludedInLayer);

                    var codeBlockIsIncludedInPacket = false;
                    
                    if (codeBlockFirstIncludedInLayerIsValid)
                    {
                        if (codeBlockFirstIncludedInLayer < Layer)
                        {
                            codeBlockIsIncludedInPacket = (bitReader.ReadBit() == 1);
                        }
                        else
                        {
                            codeBlockIsIncludedInPacket = codeBlockFirstIncludedInLayer == Layer;
                        }
                    }

                    if (codeBlockIsIncludedInPacket)
                    {
                        var codeBlock = subbandPrecinct.AddCodeBlock(x, y);

                        if (codeBlockFirstIncludedInLayer == Layer)
                        {
                            var zeroBitPlaneTagTree = subbandPrecinct.ZeroBitPlaneTagTree;
                            var nAllZeroBitPlanesIsValid = zeroBitPlaneTagTree.TryGetValue((uint)x, (uint)y, uint.MaxValue, 
                                bitReader, out uint nAllZeroBitPlanes);
                            Debug.Assert(nAllZeroBitPlanesIsValid); // must be true since limit == uint.MaxValue
                            codeBlock.NAllZeroBitPlanes_P = (int)nAllZeroBitPlanes;
                        }

                        codeBlock.NCodingPasses = (ushort)ReadNumberOfCodingPasses(bitReader);

                        ThrowIf(ResolutionLevel.TilePartComponent.COC.SP_CodeBlockCodingPassStyle != 0);

                        var codeBlockLengthIndicator = 3; // TODO: Get from and update in layer for this code block
                        while (bitReader.ReadBit() != 0)
                        {
                            codeBlockLengthIndicator++;
                        }

                        var nBitsUsedToStoreCodeBlockLength = codeBlockLengthIndicator + StaticInclude.FloorLog2(codeBlock.NCodingPasses);

                        codeBlock.LengthInBytes = bitReader.ReadBits(nBitsUsedToStoreCodeBlockLength);

                        IncludedCodeBlocks.Add(codeBlock);
                    }
                }
            }
        }

        private static uint ReadNumberOfCodingPasses(BitReader input)
        {
            if (input.ReadBit() == 0)
            {
                return 1;
            }
            else if (input.ReadBit() == 0)
            {
                return 2;
            }
            else
            {
                var next2Bits = input.ReadBits(2);
                if (next2Bits != 0b11)
                {
                    return 3 + next2Bits;
                }
                else
                {
                    var next5Bits = input.ReadBits(5);
                    if (next5Bits != 0b11111)
                    {
                        return 6 + next5Bits;
                    }
                    else
                    {
                        var next7Bits = input.ReadBits(7);
                        return 37 + next7Bits;
                    }
                }
            }
        }
    }
}
