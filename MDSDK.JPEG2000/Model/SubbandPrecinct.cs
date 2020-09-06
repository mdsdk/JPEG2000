// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Quantization;
using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Model
{
    class SubbandPrecinct
    {
        public ResolutionLevel ResolutionLevel { get; }

        public Subband Subband { get; }

        public Precinct Precinct { get; }

        public Rectangle SubbandTileComponentSampleBounds_tb { get; }

        public int NCodeBlocksX { get; }

        public int NCodeBlocksY { get; }

        public TagTree CodeBlockInclusionTagTree { get; }

        public TagTree ZeroBitPlaneTagTree { get; }

        public CodeBlock[] CodeBlocks { get; }

        public Signal2D TransformCoefficientValues { get; }

        public SubbandPrecinct(ResolutionLevel resolutionLevel, Subband subband, Precinct precinct)
        {
            ResolutionLevel = resolutionLevel;
            Subband = subband;
            Precinct = precinct;

            SubbandTileComponentSampleBounds_tb = ResolutionLevel.TilePartComponent.TileComponent.GetSubbandTileComponentSampleBounds_tb(subband);

            NCodeBlocksX = CeilDiv(SubbandTileComponentSampleBounds_tb.Width, ResolutionLevel.CodeBlockWidth);
            NCodeBlocksY = CeilDiv(SubbandTileComponentSampleBounds_tb.Height, ResolutionLevel.CodeBlockHeight);

            CodeBlockInclusionTagTree = new TagTree((uint)NCodeBlocksX, (uint)NCodeBlocksY);
            ZeroBitPlaneTagTree = new TagTree((uint)NCodeBlocksX, (uint)NCodeBlocksY);

            CodeBlocks = new CodeBlock[NCodeBlocksY * NCodeBlocksX];

            var uRange = SubbandTileComponentSampleBounds_tb.XRange;
            var vRange = SubbandTileComponentSampleBounds_tb.YRange;

            TransformCoefficientValues = ResolutionLevel.TilePartComponent.ArithmeticType switch
            {
                ArithmeticType.Int32 => new Signal2D<int>(uRange, vRange),
                ArithmeticType.Single => new Signal2D<float>(uRange, vRange),
                ArithmeticType.Double => new Signal2D<double>(uRange, vRange),
                _ => throw NotSupported(ResolutionLevel.TilePartComponent.ArithmeticType)
            };
        }

        public CodeBlock AddCodeBlock(int i, int j)
        {
            var x0 = SubbandTileComponentSampleBounds_tb.X0 + i * ResolutionLevel.CodeBlockWidth;
            var y0 = SubbandTileComponentSampleBounds_tb.Y0 + j * ResolutionLevel.CodeBlockHeight;
            var x1 = Math.Min(SubbandTileComponentSampleBounds_tb.X1, x0 + ResolutionLevel.CodeBlockWidth);
            var y1 = Math.Min(SubbandTileComponentSampleBounds_tb.Y1, y0 + ResolutionLevel.CodeBlockHeight);
            var bounds = new Rectangle(x0, y0, x1, y1);

            var codeBlock = new CodeBlock(this, bounds);
            CodeBlocks[i + NCodeBlocksX * j] = codeBlock;
            return codeBlock;
        }

        public Signal2D GetReconstructedTransformCoefficientValues()
        {
            foreach (var codeBlock in CodeBlocks)
            {
                if (codeBlock != null)
                {
                    codeBlock.DecodeTask.Wait();
                }
            }

            if (ResolutionLevel.TilePartComponent.GetDequantisizer(out Dequantisizer dequantisizer))
            {
                dequantisizer.Dequantisize(TransformCoefficientValues, ResolutionLevel.TilePartComponent, Subband);
            }

            return TransformCoefficientValues;
        }
    }
}
