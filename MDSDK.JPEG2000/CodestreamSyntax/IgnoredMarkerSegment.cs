// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class IgnoredMarkerSegment : IMarkerSegment
    {
        public Marker Marker { get; }

        public IgnoredMarkerSegment(Marker marker)
        {
            Marker = marker;
        }

        public void ReadFrom(CodestreamReader input)
        {
            input.DataReader.Input.SkipRemainingBytes();
        }
    }
}
