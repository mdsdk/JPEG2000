// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    interface IComponentMarkerSegment : IMarkerSegment
    {
        int C_ComponentIndex { get; }
    }
}
