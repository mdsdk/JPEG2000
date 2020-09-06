// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System.Collections.Generic;
using System.Linq;
using static MDSDK.JPEG2000.Utils.StaticInclude;
using static System.Math;

namespace MDSDK.JPEG2000.Model
{
    class ResolutionLevel
    {
        public TilePartComponent TilePartComponent { get; }

        public int ResolutionIndex { get; }

        public Rectangle TileComponentSampleBounds_tr { get; private set; }

        public int CodeBlockWidth { get; private set; }

        public int CodeBlockHeight { get; private set; }

        public List<SubbandPrecinct> SubbandPrecincts { get; } = new List<SubbandPrecinct>();

        public ResolutionLevel(TilePartComponent tilePartComponent, int resolutionIndex)
        {
            TilePartComponent = tilePartComponent;

            ResolutionIndex = resolutionIndex;

            InitTileComponentSampleBounds_tr();

            InitCodeBlockSize();
        }

        private void InitTileComponentSampleBounds_tr()
        {
            var tc = TilePartComponent.TileComponent.TileComponentSampleBounds_tc;

            var divisor = 1 << (TilePartComponent.COC.SP_NumberOfDecompositionLevels - ResolutionIndex);

            var trx0 = CeilDiv(tc.X0, divisor);
            var try0 = CeilDiv(tc.Y0, divisor);
            var trx1 = CeilDiv(tc.X1, divisor);
            var try1 = CeilDiv(tc.Y1, divisor);

            TileComponentSampleBounds_tr = new Rectangle(trx0, try0, trx1, try1);
        }

        private void InitCodeBlockSize()
        {
            var coc = TilePartComponent.COC;

            var xcb = coc.SP_CodeBlockWidthExponentOffsetValue + 2;
            var ycb = coc.SP_CodeBlockHeightExponentOffsetValue + 2;

            var rsp = coc.SP_ResolutionPrecinctSizes[ResolutionIndex];

            var ppx = rsp.PPx_WidthExponent;
            var ppy = rsp.PPy_HeightExponent;

            xcb = Min(xcb, (ResolutionIndex == 0) ? ppx : ppx - 1);
            ycb = Min(ycb, (ResolutionIndex == 0) ? ppy : ppy - 1);

            var siz = TilePartComponent.TilePart.Tile.Image.Header.SIZ;
            
            var divisor = 1 << (TilePartComponent.COC.SP_NumberOfDecompositionLevels - ResolutionIndex);

            var reducedResolutionTileWidth = CeilDiv(siz.XT_TileWidth, divisor);
            var reducedResolutionTileHeight = CeilDiv(siz.YT_TileHeight, divisor);

            CodeBlockWidth = Min(reducedResolutionTileWidth, 1 << xcb);
            CodeBlockHeight = Min(reducedResolutionTileHeight, 1 << ycb);
        }

        public void GetNPrecincts(out int nPrecinctsX, out int nPrecinctsY)
        {
            var tr = TileComponentSampleBounds_tr;

            var coc = TilePartComponent.COC;
            var rsp = coc.SP_ResolutionPrecinctSizes[ResolutionIndex];

            var ppx = rsp.PPx_WidthExponent;
            var ppy = rsp.PPy_HeightExponent;

            nPrecinctsX = CeilDiv(tr.X1, 1 << ppx) - FloorDiv(tr.X0, 1 << ppx);
            nPrecinctsY = CeilDiv(tr.Y1, 1 << ppy) - FloorDiv(tr.Y0, 1 << ppy);
        }

        public SubbandPrecinct GetSubbandPrecinct(Subband subband)
        {
            // TODO: Add support for precincts
            var subbandPrecinct = SubbandPrecincts.FirstOrDefault(o => o.Subband == subband);
            if (subbandPrecinct == null)
            {
                subbandPrecinct = new SubbandPrecinct(this, subband, new Precinct());
                SubbandPrecincts.Add(subbandPrecinct);
            }
            return subbandPrecinct;
        }
    }
}
