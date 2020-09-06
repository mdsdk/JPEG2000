// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using System.Collections.Generic;
using System.Linq;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class TilePartHeader : FunctionalMarkerSegmentSet
    {
        public StartOfTilePartMarkerSegment SOT { get; }

        public Subband[][][] Subbands { get; }

        public TilePartHeader(Image image, IEnumerable<IMarkerSegment> markerSegments)
        {
            SOT = markerSegments.OfType<StartOfTilePartMarkerSegment>().Single();

            var nComponents = image.Header.SIZ.C_NumberOfImageComponents;

            Init(nComponents, markerSegments);

            COD ??= image.Header.COD;

            QCD ??= image.Header.QCD;

            for (var i = 0; i < nComponents; i++)
            {
                COC[i] ??= COD ?? image.Header.COC[i] ?? image.Header.COD;
                QCC[i] ??= QCD ?? image.Header.QCC[i] ?? image.Header.QCD;
            }

            Subbands = CreateSubbands(nComponents);
        }

        private Subband[][][] CreateSubbands(int nComponents)
        {
            var subbands = new Subband[nComponents][][];
            for (var component = 0; component < nComponents; component++)
            {
                var nDecompositionLevels = COC[component].SP_NumberOfDecompositionLevels;

                var subbandIndex = 0;

                Subband NewSubband(int decompositionLevel, SubbandType subbandType)
                {
                    return new Subband(subbandIndex++, decompositionLevel, subbandType);
                }

                subbands[component] = new Subband[nDecompositionLevels + 1][];
                for (var resolution = 0; resolution <= nDecompositionLevels; resolution++)
                {
                    if (resolution == 0)
                    {
                        var decompositionLevel = nDecompositionLevels - resolution;
                        subbands[component][resolution] = new[]
                        {
                            NewSubband(decompositionLevel, SubbandType.LL)
                        };
                    }
                    else
                    {
                        var decompositionLevel = nDecompositionLevels - (resolution - 1);
                        subbands[component][resolution] = new[]
                        {
                            NewSubband(decompositionLevel, SubbandType.HL),
                            NewSubband(decompositionLevel, SubbandType.LH),
                            NewSubband(decompositionLevel, SubbandType.HH)
                        };
                    }
                }
            }
            return subbands;
        }
    }
}

