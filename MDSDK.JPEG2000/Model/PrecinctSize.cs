// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Model
{
    readonly struct PrecinctSize
    {
        public int PPx_WidthExponent { get; }

        public int PPy_HeightExponent { get; }

        public PrecinctSize(int ppx, int ppy)
        {
            PPx_WidthExponent = ppx;
            PPy_HeightExponent = ppy;
        }
    }
}
