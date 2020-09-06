// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.JPEG2000.Model
{
    [Flags]
    enum CodingStyle
    {
        CustomPrecinctSizesUsed = 0x01,
        StartOfPacketMarkerSegmentsUsed = 0x02,
        EndOfPacketHeaderMarkerSegmentsUsed = 0x04,
    }
}
