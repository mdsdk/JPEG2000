using MDSDK.BinaryIO;
using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    readonly struct BoxHeader
    {
        public BoxType BoxType { get; }

        public int BoxDataLength { get; }

        private BoxHeader(BoxType boxType, int boxDataLength)
        {
            BoxType = boxType;
            BoxDataLength = boxDataLength;
        }

        public static BoxHeader ReadFrom(BinaryDataReader input)
        {
            var boxLength = input.Read<UInt32>();

            var boxType = new BoxType(input.Read<UInt32>());

            int boxDataLength;

            if (boxLength == 0)
            {
                boxDataLength = -1; // Unknown length
            }
            else if (boxLength == 1)
            {
                var longBoxLength = input.Read<UInt64>();
                ThrowIf(longBoxLength < 12);
                ThrowIf(longBoxLength > int.MaxValue);
                boxDataLength = (int)longBoxLength - 12;
            }
            else 
            {
                ThrowIf(boxLength < 8);
                ThrowIf(boxLength > int.MaxValue);
                boxDataLength = (int)boxLength - 8;
            }

            return new BoxHeader(boxType, boxDataLength);
        }
    }
}
