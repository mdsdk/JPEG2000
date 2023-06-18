// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class CaptureResolutionBox : IBox
    {
        public ushort VerticalCaptureGridResolutionNumerator { get; private set; }
        
        public ushort VerticalCaptureGridResolutionDenominator { get; private set; }
        
        public ushort HorizontalCaptureGridResolutionNumerator { get; private set; }

        public ushort HorizontalCaptureGridResolutionDenominator { get; private set; }

        public sbyte VerticalCaptureGridResolutionExponent { get; private set; }

        public sbyte HorizontalCaptureGridResolutionExponent { get; private set; }

        public void ReadFrom(JP2FileReader reader)
        {
            var input = reader.DataReader;
            VerticalCaptureGridResolutionNumerator = input.Read<UInt16>();
            VerticalCaptureGridResolutionDenominator = input.Read<UInt16>();
            HorizontalCaptureGridResolutionNumerator = input.Read<UInt16>();
            HorizontalCaptureGridResolutionDenominator = input.Read<UInt16>();
            VerticalCaptureGridResolutionExponent = input.ReadSByte();
            HorizontalCaptureGridResolutionExponent = input.ReadSByte();
            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
