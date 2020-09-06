// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.JPEG2000.Model
{
    [Flags]
    enum CodeBlockCodingPassStyle
    {
        SelectiveArithmeticCodingBypass = 0x01,
        ResetContextProbabilitiesOnCodingPassBoundaries = 0x02,
        TerminationOnEachCodingPass = 0x04,
        VerticallyStripeCausalContext = 0x08,
        PredictableTermination = 0x10,
        SegmentationSymbolsAreUsed = 0x20
    }
}
