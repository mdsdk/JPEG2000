// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Collections.Generic;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class FunctionalMarkerSegmentSet
    {
        public CodingStyleDefaultMarkerSegment COD { get; protected set; }

        public CodingStyleMarkerSegment[] COC { get; private set; }

        public QuantizationDefaultMarkerSegment QCD { get; protected set; }

        public QuantizationMarkerSegment[] QCC { get; private set; }

        protected void Init(int nComponents, IEnumerable<IMarkerSegment> markerSegments)
        {
            COC = new CodingStyleMarkerSegment[nComponents];

            QCC = new QuantizationMarkerSegment[nComponents];

            foreach (var markerSegment in markerSegments)
            {
                if (markerSegment is CodingStyleDefaultMarkerSegment cod)
                {
                    COD = cod;
                }
                else if (markerSegment is CodingStyleComponentMarkerSegment coc)
                {
                    COC[coc.C_ComponentIndex] = coc;
                }
                else if (markerSegment is QuantizationDefaultMarkerSegment qcd)
                {
                    QCD = qcd;
                }
                else if (markerSegment is QuantizationComponentMarkerSegment qcc)
                {
                    QCC[qcc.C_ComponentIndex] = qcc;
                }
            }
        }
    }
}
