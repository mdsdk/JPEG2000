using MDSDK.BinaryIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class ChannelDescription
    {
        public ushort ChannelIndex { get; private set; }

        public ushort ChannelType { get; private set; }

        public ushort ChannelAssociation { get; private set; }

        public void ReadFrom(BinaryDataReader input)
        {
            ChannelIndex = input.Read<UInt16>();
            ChannelType = input.Read<UInt16>();
            ChannelAssociation = input.Read<UInt16>();
        }

    }
}
