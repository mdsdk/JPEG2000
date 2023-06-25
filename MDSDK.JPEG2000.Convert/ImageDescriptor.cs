// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.JPEG2000.Utils.StaticInclude;
using System.Diagnostics;
using System.Drawing;
using System;

namespace MDSDK.JPEG2000.Convert
{
    class ImageDescriptor
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public Colourspace Colourspace { get; set; }

        public ImageComponentDescriptor[] Components { get; set; }

        private int GetColorComponentIndex(int channelAssociation, int defaultColorComponentIndex)
        {
            for (var i = 0; i < Components.Length; i++)
            {
                var cd = Components[i].ChannelDescription;
                if ((cd != null) && (cd.ChannelType == 0) && (cd.ChannelAssociation == channelAssociation))
                {
                    return i;
                }
            }
            return defaultColorComponentIndex;
        }

        public delegate Color BitmapPixelCreator(int[] imageData, ref int i);

        public BitmapPixelCreator GetBitmapPixelCreator()
        {
            return Colourspace switch
            {
                Colourspace.sRGB => GetSRGBPixelCreator(),
                Colourspace.sYCC => GetSYCCPixelCreator(),
                Colourspace.Greyscale => GetGreyscalePixelCreator(),
                _ => throw NotSupported(Colourspace)
            };
        }

        private BitmapPixelCreator GetSRGBPixelCreator()
        {
            var rComponentIndex = GetColorComponentIndex(1, 0);
            var gComponentIndex = GetColorComponentIndex(2, 1);
            var bComponentIndex = GetColorComponentIndex(3, 2);

            Color GetPixelColor(int[] imageData, ref int i)
            {
                var r = imageData[i + rComponentIndex];
                var g = imageData[i + gComponentIndex];
                var b = imageData[i + bComponentIndex];

                i += 3;
                
                return Color.FromArgb(255, r, g, b);
            }

            return GetPixelColor;
        }

        private static int ClampTo8Bit(double x) => (int)Math.Max(0, Math.Min(x, 255));

        private BitmapPixelCreator GetSYCCPixelCreator()
        {
            var yComponentIndex = GetColorComponentIndex(1, 0);
            var cbComponentIndex = GetColorComponentIndex(2, 1);
            var crComponentIndex = GetColorComponentIndex(3, 2);

            Color GetPixelColor(int[] imageData, ref int i)
            {
                var y = imageData[i + yComponentIndex];
                var cb = imageData[i + cbComponentIndex];
                var cr = imageData[i + crComponentIndex];

                i += 3;

                var r = y + 1.402 * (cr - 128);
                var g = y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128);
                var b = y + 1.772 * (cb - 128);

                return Color.FromArgb(255, ClampTo8Bit(r), ClampTo8Bit(g), ClampTo8Bit(b));
            }

            return GetPixelColor;
        }

        private BitmapPixelCreator GetGreyscalePixelCreator()
        {
            ThrowIf(Components.Length != 1);
            
            var component = Components[0];

            ThrowIf(component.IsSigned);

            var maxValue = (1 << component.BitDepth) - 1;

            Color GetPixelColor(int[] imageData, ref int i)
            {
                var pixelValue = ClampTo8Bit(255.0 * imageData[i++] / maxValue);
                return Color.FromArgb(255, pixelValue, pixelValue, pixelValue);
            }

            return GetPixelColor;
        }
    }
}
