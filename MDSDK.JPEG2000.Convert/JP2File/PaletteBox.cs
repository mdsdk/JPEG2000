// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class PaletteBox : IBox
    {
        public void ReadFrom(JP2FileReader reader)
        {
            throw NotSupported(BoxType.Palette);
        }
    }
}
