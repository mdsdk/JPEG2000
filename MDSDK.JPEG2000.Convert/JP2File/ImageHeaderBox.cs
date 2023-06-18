// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class ImageHeaderBox : IBox
    {
        public uint Height { get; private set; }

        public uint Width { get; private set; }

        public ushort NumberOfComponents { get; private set; }

        public byte BitsPerComponent { get; private set; }
        
        public byte CompressionType { get; private set; }

        public byte ColorspaceUnknown { get; private set; }

        public byte IntellectualProperty { get; private set; }

        public void ReadFrom(JP2FileReader reader)
        {
            var input = reader.DataReader;

            Height = input.Read<UInt32>();
            Width = input.Read<UInt32>();
            NumberOfComponents = input.Read<ushort>();
            BitsPerComponent = input.ReadByte();
            CompressionType = input.ReadByte();
            ColorspaceUnknown = input.ReadByte();
            IntellectualProperty = input.ReadByte();

            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
