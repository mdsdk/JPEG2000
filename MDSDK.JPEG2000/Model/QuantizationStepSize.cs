// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Model
{
    class QuantizationStepSize
    {
        public int Exponent { get; }
        
        public int Mantissa { get; }

        public QuantizationStepSize(ushort us)
        {
            Exponent = us >> 11;
            Mantissa = us & 0x07FF;
        }

        public override string ToString()
        {
            return $"{GetType().Name}(Exponent={Exponent},Mantissa={Mantissa})";
        }
    }
}
