// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Reflection;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    readonly struct BoxType : IEquatable<BoxType>
    {
        private readonly uint _value;

        public BoxType(uint value)
        {
            _value = value;
        }

        public bool Equals(BoxType other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return (obj is BoxType boxType) && Equals(boxType);
        }

        public static bool operator ==(BoxType a, BoxType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BoxType a, BoxType b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return (int)_value;
        }

        public override string ToString()
        {
            foreach (var field in typeof(BoxType).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var BoxType = (BoxType)field.GetValue(null);
                if (Equals(BoxType))
                {
                    return field.Name;
                }
            }
            return _value.ToString("X");
        }
        
        public static readonly BoxType Signature = new BoxType(0x6A502020);
        public static readonly BoxType FileType = new BoxType(0x66747970);
        public static readonly BoxType JP2Header = new BoxType(0x6A703268);
        public static readonly BoxType ImageHeader = new BoxType(0x69686472);
        public static readonly BoxType BitsPerComponent = new BoxType(0x62706363);
        public static readonly BoxType ColorSpecification = new BoxType(0x636F6C72);
        public static readonly BoxType Palette = new BoxType(0x70636C72);
        public static readonly BoxType ComponentMapping = new BoxType(0x636D6170);
        public static readonly BoxType ChannelDefinition = new BoxType(0x63646566);
        public static readonly BoxType Resolution = new BoxType(0x72657320);
        public static readonly BoxType CaptureResolution = new BoxType(0x72657363);
        public static readonly BoxType DefaultDisplayResolution = new BoxType(0x72657364);
        public static readonly BoxType ContiguousCodestream = new BoxType(0x6A703263);
        public static readonly BoxType IntellectualProperty = new BoxType(0x6A703269);
        public static readonly BoxType XML = new BoxType(0x786D6C20);
        public static readonly BoxType UUID = new BoxType(0x75756964);
        public static readonly BoxType UUIDInfo = new BoxType(0x75696E66);
        public static readonly BoxType UUIDList = new BoxType(0x75637374);
        public static readonly BoxType URL = new BoxType(0x75726C20);
    }
}
