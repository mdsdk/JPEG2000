// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Convert.JP2File;

namespace MDSDK.JPEG2000.Convert
{
    class ImageComponentDescriptor
    {
        public int BitDepth { get; set; }

        public bool IsSigned { get; set; }

        public ChannelDescription ChannelDescription { get; set; }
    }
}
