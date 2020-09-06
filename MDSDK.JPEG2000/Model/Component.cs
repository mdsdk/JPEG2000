// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Utils;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Model
{
    class Component
    {
        public Image Image { get; }

        public int ComponentIndex { get; }

        public SizeMarkerSegment.ComponentSpecification SIZ { get; }

        public bool ComponentSampleValuesAreSigned { get; } 

        public int ComponentSampleBitDepth { get; }

        public int DCShift { get; }

        public int MinSampleValue { get; }

        public int MaxSampleValue { get; }

        public Component(Image image, int componentIndex)
        {
            Image = image;

            ComponentIndex = componentIndex;

            SIZ = image.Header.SIZ.ComponentSpecifications[componentIndex];

            ComponentSampleValuesAreSigned = (SIZ.S_PrecisionAndSignOfComponentSamples & 0x80) != 0;
            
            ComponentSampleBitDepth = 1 + (SIZ.S_PrecisionAndSignOfComponentSamples & 0x7F);
            
            if (ComponentSampleValuesAreSigned)
            {
                DCShift = 0;
                MinSampleValue = -(1 << SIZ.S_PrecisionAndSignOfComponentSamples);
                MaxSampleValue = (1 << SIZ.S_PrecisionAndSignOfComponentSamples) - 1;
            }
            else
            {
                DCShift = (1 << SIZ.S_PrecisionAndSignOfComponentSamples);
                MinSampleValue = 0;
                MaxSampleValue = (1 << ComponentSampleBitDepth) - 1;
            }
        }

        public Rectangle ToComponentSampleSpace(Rectangle rect)
        {
            var x0 = CeilDiv(rect.X0, SIZ.XR_HorizontalSubSamplingFactor);
            var y0 = CeilDiv(rect.Y0, SIZ.YR_VerticalSubSamplingFactor);
            var x1 = CeilDiv(rect.X1, SIZ.XR_HorizontalSubSamplingFactor);
            var y1 = CeilDiv(rect.Y1, SIZ.YR_VerticalSubSamplingFactor);
            return new Rectangle(x0, y0, x1, y1);
        }
    }
}
