// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class ResolutionBox : SuperBox
    {
        protected override bool IsValidNestedBoxType(BoxType boxType)
        {
            return (boxType == BoxType.CaptureResolution)
                || (boxType == BoxType.DefaultDisplayResolution);
        }
    }
}
