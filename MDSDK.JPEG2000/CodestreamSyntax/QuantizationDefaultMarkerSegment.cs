// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class QuantizationDefaultMarkerSegment : QuantizationMarkerSegment
    {
        public override void ReadFrom(CodestreamReader reader)
        {
            Read_S_QuantizationStyle(reader.Input);
            Read_SP_Parameters(reader.Input);
        }
    }
}
