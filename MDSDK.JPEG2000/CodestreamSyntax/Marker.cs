// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Reflection;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    // From ITU-T Rec. T.800 (2000 FCDV1.0), Table A-2 — List of marker segments

    readonly struct Marker : IEquatable<Marker>
    {
        private readonly ushort _value;

        public Marker(ushort value)
        {
            _value = value;
        }

        public bool Equals(Marker other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Marker marker) && Equals(marker);
        }

        public static bool operator ==(Marker a, Marker b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Marker a, Marker b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            foreach (var field in typeof(Marker).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var marker = (Marker)field.GetValue(null);
                if (Equals(marker))
                {
                    return field.Name;
                }
            }
            return _value.ToString("X");
        }

        // Delimiting marker segments

        public static readonly Marker SOC = new Marker(0xFF4F); // Start of codestream
        public static readonly Marker SOT = new Marker(0xFF90); // Start of tile-part
        public static readonly Marker SOD = new Marker(0xFF93); // Start of data
        public static readonly Marker EOC = new Marker(0xFFD9); // End of codestream

        // Fixed information marker segments

        public static readonly Marker SIZ = new Marker(0xFF51); // Image and tile size

        // Functional marker segments

        public static readonly Marker COD = new Marker(0xFF52); // Coding style default
        public static readonly Marker COC = new Marker(0xFF53); // Coding style component
        public static readonly Marker QCD = new Marker(0xFF5C); // Quantization default
        public static readonly Marker QCC = new Marker(0xFF5D); // Quantization componenent
        public static readonly Marker RGN = new Marker(0xFF5E); // Region-of-interest
        public static readonly Marker POD = new Marker(0xFF5F); // Progression order default

        // Pointer marker segments

        public static readonly Marker TLM = new Marker(0xFF55); // Tile-part lengths, main header
        public static readonly Marker PLM = new Marker(0xFF57); // Packet length, main header
        public static readonly Marker PLT = new Marker(0xFF58); // Packet length, tile-part header
        public static readonly Marker PPM = new Marker(0xFF60); // Packed packet headers, main header
        public static readonly Marker PPT = new Marker(0xFF61); // Packed packet headers, tile-part header

        // In bit stream marker segments

        public static readonly Marker SOP = new Marker(0xFF91); // Start of packet
        public static readonly Marker EPH = new Marker(0xFF92); // End of packet header

        // Informational marker segment 

        public static readonly Marker CME = new Marker(0xFF64); //  Comment and extension
    }
}
