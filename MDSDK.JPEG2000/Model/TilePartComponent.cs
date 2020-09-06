// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Quantization;
using MDSDK.JPEG2000.WaveletTransformation;
using static MDSDK.JPEG2000.Utils.StaticInclude;
using System.Diagnostics;
using MDSDK.JPEG2000.Utils;

namespace MDSDK.JPEG2000.Model
{
    class TilePartComponent
    {
        public TilePart TilePart { get; }

        public TileComponent TileComponent { get; }

        public CodingStyleMarkerSegment COC { get; }

        public QuantizationMarkerSegment QCC { get; }

        public ResolutionLevel[] ResolutionLevels { get; }

        public ArithmeticType ArithmeticType { get; }

        public TilePartComponent(TilePart tilePart, TileComponent tileComponent)
        {
            TilePart = tilePart;

            TileComponent = tileComponent;

            COC = tilePart.Header.COC[TileComponent.Component.ComponentIndex];
            QCC = tilePart.Header.QCC[TileComponent.Component.ComponentIndex];

            var nResolutions = COC.SP_NumberOfDecompositionLevels + 1;

            ResolutionLevels = new ResolutionLevel[nResolutions];

            for (var iResolution = 0; iResolution < nResolutions; iResolution++)
            {
                ResolutionLevels[iResolution] = new ResolutionLevel(this, iResolution);
            }

            if (COC.SP_WaveletTransformUsed == WaveletTransform.Reversible_5_3)
            {
                ArithmeticType = ArithmeticType.Int32;
            }
            else if (COC.SP_WaveletTransformUsed == WaveletTransform.Irreversible_9_7)
            {
                ArithmeticType = ArithmeticType.Double;
            }
            else
            {
                throw NotSupported(COC.SP_WaveletTransformUsed);
            }
        }

        public bool GetDequantisizer(out Dequantisizer dequantisizer)
        {
            if (QCC.QuantizationStyle == QuantizationStyle.NoQuantization)
            {
                dequantisizer = null;
                return false;
            }
            else if (QCC.QuantizationStyle == QuantizationStyle.ScalarExpounded)
            {
                if (COC.SP_WaveletTransformUsed == WaveletTransform.Irreversible_9_7)
                {
                    dequantisizer = ArithmeticType switch
                    {
                        ArithmeticType.Single => new SinglePrecisionIrreversibleDequantisizer(),
                        ArithmeticType.Double => new DoublePrecisionIrreversibleDequantisizer(),
                        _ => throw NotSupported(ArithmeticType)
                    };
                    return true;
                }
                else
                {
                    throw NotSupported(COC.SP_WaveletTransformUsed);
                }
            }
            else
            {
                throw NotSupported(QCC.QuantizationStyle);
            }
        }

        public InverseDiscreteWaveletTransformer GetInverseDiscreteWaveletTransformer()
        {
            if (COC.SP_WaveletTransformUsed == WaveletTransform.Reversible_5_3)
            {
                return ArithmeticType switch
                {
                    ArithmeticType.Int32 => new ReversibleInverseDiscreteWaveletTransformer(),
                    _ => throw NotSupported(ArithmeticType)
                };
            }
            else if (COC.SP_WaveletTransformUsed == WaveletTransform.Irreversible_9_7)
            {
                return ArithmeticType switch
                {
                    ArithmeticType.Single => new SinglePrecisionIrreversibleInverseDiscreteWaveletTransformer(),
                    ArithmeticType.Double => new DoublePrecisionIrreversibleInverseDiscreteWaveletTransformer(),
                    _ => throw NotSupported(ArithmeticType)
                };
            }
            else
            {
                throw NotSupported(COC.SP_WaveletTransformUsed);
            }
        }

        public Signal2D ReconstructImageSampleValues()
        {
            var idwt = GetInverseDiscreteWaveletTransformer();

            var subbandPrecincts = ResolutionLevels[0].SubbandPrecincts;

            Debug.Assert(subbandPrecincts.Count == 1);

            var ll = subbandPrecincts[0];
            var llSubband = ll.Subband;

            Debug.Assert(llSubband.SubbandType == SubbandType.LL);

            var llSignal = ll.GetReconstructedTransformCoefficientValues();

            for (var i = 1; i < ResolutionLevels.Length; i++)
            {
                subbandPrecincts = ResolutionLevels[i].SubbandPrecincts;

                Debug.Assert(subbandPrecincts.Count == 3);

                var hl = subbandPrecincts[0];
                var lh = subbandPrecincts[1];
                var hh = subbandPrecincts[2];

                Debug.Assert(hl.Subband.SubbandType == SubbandType.HL);
                Debug.Assert(lh.Subband.SubbandType == SubbandType.LH);
                Debug.Assert(hh.Subband.SubbandType == SubbandType.HH);

                llSubband = new Subband(-1, llSubband.DecompositionLevel - 1, SubbandType.LL);

                var tb = TileComponent.GetSubbandTileComponentSampleBounds_tb(llSubband);
                var a = llSignal.Create(tb.XRange, tb.YRange);

                idwt.Perform2DSubbandReconstruction(a, llSignal,
                    hl.GetReconstructedTransformCoefficientValues(),
                    lh.GetReconstructedTransformCoefficientValues(),
                    hh.GetReconstructedTransformCoefficientValues());

                llSignal = a;
            }

            return llSignal;
        }
    }
}
