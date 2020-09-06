// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Utils;
using static System.Math;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Model
{
    class Image
    {
        public ImageHeader Header { get; }

        public Component[] Components { get; }

        public int NTilesX { get; }

        public int NTilesY { get; }

        public Tile[] Tiles { get; }

        public Image(ImageHeader header)
        {
            Header = header;

            Components = new Component[Header.SIZ.C_NumberOfImageComponents];

            for (var iComponent = 0; iComponent < Components.Length; iComponent++)
            {
                Components[iComponent] = new Component(this, iComponent);
            }

            var siz = Header.SIZ;

            NTilesX = CeilDiv(siz.X_ReferenceGridWidth - siz.XT_HorizontalOffsetOfFirstTileInReferenceGrid, siz.XT_TileWidth);
            NTilesY = CeilDiv(siz.Y_ReferenceGridHeight - siz.YTO_VerticalOffsetOfFirstTileInReferenceGrid, siz.YT_TileHeight);

            Tiles = new Tile[NTilesX * NTilesY];

            for (var iTile = 0; iTile < Tiles.Length; iTile++)
            {
                Tiles[iTile] = new Tile(this, iTile);
            }
        }

        public Rectangle GetTileBoundsInReferenceGrid_tx(int p, int q)
        {
            var siz = Header.SIZ;

            var tx0 = Max(siz.XT_HorizontalOffsetOfFirstTileInReferenceGrid + p * siz.XT_TileWidth,
                siz.XO_HorizontalOffsetOfImageAreaInReferenceGrid);

            var ty0 = Max(siz.YTO_VerticalOffsetOfFirstTileInReferenceGrid + q * siz.YT_TileHeight,
                siz.YO_VerticalOffsetOfImageAreaInReferenceGrid);

            var tx1 = Min(siz.XT_HorizontalOffsetOfFirstTileInReferenceGrid + (p + 1) * siz.XT_TileWidth,
                siz.X_ReferenceGridWidth);

            var ty1 = Min(siz.YTO_VerticalOffsetOfFirstTileInReferenceGrid + (q + 1) * siz.YT_TileHeight,
                siz.Y_ReferenceGridHeight);

            return new Rectangle(tx0, ty0, tx1, ty1);
        }
    }
}

