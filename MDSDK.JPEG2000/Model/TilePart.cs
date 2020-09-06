// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.CodestreamSyntax;

namespace MDSDK.JPEG2000.Model
{
    class TilePart
    {
        public Tile Tile { get; }

        public TilePartHeader Header { get; }

        public TilePart(Tile tile, TilePartHeader header)
        {
            Tile = tile;

            Header = header;
        }
    }
}
