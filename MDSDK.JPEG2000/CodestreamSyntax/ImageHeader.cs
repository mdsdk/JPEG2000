// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Collections.Generic;
using System.Linq;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class ImageHeader : FunctionalMarkerSegmentSet
    {
        public SizeMarkerSegment SIZ { get; }

        public ImageHeader(IEnumerable<IMarkerSegment> markerSegments)
        {
            SIZ = markerSegments.OfType<SizeMarkerSegment>().Single();

            Init(SIZ.C_NumberOfImageComponents, markerSegments);
        }
    }
}
