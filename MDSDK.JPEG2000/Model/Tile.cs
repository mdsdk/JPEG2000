// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;

namespace MDSDK.JPEG2000.Model
{
    class Tile
    {
        public Image Image { get; }

        public int TileIndex { get; }

        public int HorizontalIndex { get; }

        public int VerticalIndex { get; }

        public Rectangle BoundsInReferenceGrid_tx { get; }

        public TileComponent[] TileComponents { get; }

        public Tile(Image image, int tileIndex)
        {
            Image = image;

            TileIndex = tileIndex;
            
            HorizontalIndex = TileIndex % Image.NTilesY;
            
            VerticalIndex= TileIndex / Image.NTilesY;

            BoundsInReferenceGrid_tx = image.GetTileBoundsInReferenceGrid_tx(HorizontalIndex, VerticalIndex);

            TileComponents = new TileComponent[Image.Components.Length];

            for (var iComponent = 0; iComponent < image.Components.Length; iComponent++)
            {
                TileComponents[iComponent] = new TileComponent(this, image.Components[iComponent]);
            }
        }
    }
}
