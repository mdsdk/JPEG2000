// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class ChannelDefinitionBox : IBox
    {
        public ushort NumberOfChannelDescriptions{ get; private set; }
        
        public ChannelDescription[] ChannelDescriptions{ get; private set; }
            
        public void ReadFrom(JP2FileReader reader)
        {
            var input = reader.DataReader;

            NumberOfChannelDescriptions = input.Read<UInt16>();

            ChannelDescriptions = new ChannelDescription[NumberOfChannelDescriptions];
            for (int i = 0; i < ChannelDescriptions.Length; i++)
            {
                var channelDescription = new ChannelDescription();
                channelDescription.ReadFrom(input);
                ChannelDescriptions[i] = channelDescription;
            }

            ThrowIf(!reader.RawInput.AtEnd);
        }
    }
}
