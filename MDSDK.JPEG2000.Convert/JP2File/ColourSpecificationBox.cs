// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class ColourSpecificationBox : IBox
    {
        public byte SpecificationMethod { get; private set; }

        public sbyte Precedence { get; private set; }
        
        public byte ColorSpaceApproximation { get; private set; }

        public uint? EnumeratedColourspace { get; private set; }

        public byte[] ICCProfile { get; private set; }

        public void ReadFrom(JP2FileReader reader)
        {
            var input = reader.DataReader;

            SpecificationMethod = input.ReadByte();
            Precedence = (sbyte)input.ReadByte();
            ColorSpaceApproximation = input.ReadByte();

            if (SpecificationMethod == 1)
            {
                EnumeratedColourspace = input.Read<UInt32>();
            }
            else if (SpecificationMethod == 2)
            {
                ICCProfile = reader.RawInput.ReadRemainingBytes();
            }
            else
            {
                throw NotSupported(SpecificationMethod);
            }

            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
