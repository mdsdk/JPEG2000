// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using System.Text;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class FileTypeBox : IBox
    {
        public string Brand { get; private set; }

        public uint MinorVersion { get; private set; }

        public string[] CompatibilityList { get; private set; }

        public void ReadFrom(JP2FileReader reader)
        {
            Brand = Encoding.ASCII.GetString(reader.RawInput.ReadBytes(4));

            MinorVersion = reader.DataReader.Read<UInt32>();

            ThrowIf(reader.RawInput.BytesRemaining % 4 != 0);

            var compatibilityListLength = reader.RawInput.BytesRemaining / 4;

            CompatibilityList = new string[compatibilityListLength];
            for (var i = 0; i < compatibilityListLength; i++)
            {
                CompatibilityList[i] = Encoding.ASCII.GetString(reader.RawInput.ReadBytes(4));
            }

            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
