// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    interface IMarkerSegment
    {
        void ReadFrom(CodestreamReader reader);
    }
}
