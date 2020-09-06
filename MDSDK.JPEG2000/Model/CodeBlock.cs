// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System.Threading.Tasks;

namespace MDSDK.JPEG2000.Model
{
    class CodeBlock
    {
        public Subband Subband { get; }

        public Rectangle Bounds { get; }

        public SubbandPrecinct SubbandPrecinct { get; }

        public CodeBlock(Subband subband, Rectangle bounds)
        {
            Subband = subband;
            Bounds = bounds;
        }

        public CodeBlock(SubbandPrecinct subbandPrecinct, Rectangle bounds)
            : this(subbandPrecinct.Subband, bounds)
        {
            SubbandPrecinct = subbandPrecinct;
        }

        public int? NAllZeroBitPlanes_P { get; set; }

        public ushort NCodingPasses { get; set; }

        public uint LengthInBytes { get; set; }

        public override string ToString()
        {
            return $"CodeBlock(sb={Subband} bounds={Bounds} nCPs={NCodingPasses} n0BPs={NAllZeroBitPlanes_P}, l={LengthInBytes})";
        }

        public Task DecodeTask { get; set; }
    }
}
