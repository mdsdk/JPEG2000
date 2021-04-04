// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System;
using System.Text;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class CommentAndExtensionMarkerSegment : IMarkerSegment
    {
        public ushort R_RegistrationValue { get; private set; }

        public byte[] Data { get; private set; }

        public string DataAsString { get; private set; }

        public void ReadFrom(CodestreamReader reader)
        {
            var input = reader.Input;

            R_RegistrationValue = input.Read<UInt16>();

            Data = input.ReadRemainingBytes();

            if (R_RegistrationValue == 1)
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                DataAsString = encoding.GetString(Data);
            }
        }
    }
}
