// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class BitsPerComponentBox : IBox
    {
        public byte BitsPerComponent { get; private set; }
        
        public void ReadFrom(JP2FileReader reader)
        {
            var input = reader.DataReader;
            BitsPerComponent = input.ReadByte();
            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
