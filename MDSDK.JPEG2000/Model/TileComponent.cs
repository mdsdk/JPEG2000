// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Model
{
    class TileComponent
    {
        public Tile Tile { get; }

        public Component Component { get; }

        public Rectangle TileComponentSampleBounds_tc { get; }

        public TileComponent(Tile tile, Component component)
        {
            Tile = tile;

            Component = component;

            TileComponentSampleBounds_tc = Component.ToComponentSampleSpace(tile.BoundsInReferenceGrid_tx);
        }

        public Rectangle GetSubbandTileComponentSampleBounds_tb(Subband subBand)
        {
            var tc = TileComponentSampleBounds_tc;

            var xo = (subBand.SubbandType.HorizontalFilter == SubbandType.Filter.HighPass) ? 1 : 0;
            var yo = (subBand.SubbandType.VerticalFilter == SubbandType.Filter.HighPass) ? 1 : 0;

            var nb = subBand.DecompositionLevel;

            var m = 1 << (nb - 1);
            var d = 1 << nb;

            var dtcx = m * xo;
            var dtcy = m * yo;

            var tbx0 = CeilDiv(tc.X0 - dtcx, d);
            var tby0 = CeilDiv(tc.Y0 - dtcy, d);
            var tbx1 = CeilDiv(tc.X1 - dtcx, d);
            var tby1 = CeilDiv(tc.Y1 - dtcy, d);

            return new Rectangle(tbx0, tby0, tbx1, tby1);
        }
    }
}
