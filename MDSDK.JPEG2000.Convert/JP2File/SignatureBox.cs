// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class SignatureBox : IBox
    {
        public uint Signature { get; private set; }
        
        public void ReadFrom(JP2FileReader reader)
        {
            Signature = reader.DataReader.Read<UInt32>();
            
            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
