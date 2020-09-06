// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    static class MarkerSegmentFactory
    {
        public static IMarkerSegment CreateMarkerSegment(Marker marker)
        {
            if (marker == Marker.SIZ)
            {
                return new SizeMarkerSegment();
            }
            else if (marker == Marker.COD)
            {
                return new CodingStyleDefaultMarkerSegment();
            }
            else if (marker == Marker.COC)
            {
                return new CodingStyleComponentMarkerSegment();
            }
            else if (marker == Marker.QCD)
            {
                return new QuantizationDefaultMarkerSegment();
            }
            else if (marker == Marker.QCC)
            {
                return new QuantizationComponentMarkerSegment();
            }
            else if (marker == Marker.CME)
            {
                return new CommentAndExtensionMarkerSegment();
            }
            else if (marker == Marker.SOT)
            {
                return new StartOfTilePartMarkerSegment();
            }
            else
            {          
                return new IgnoredMarkerSegment(marker);
            }
        }
    }
}
