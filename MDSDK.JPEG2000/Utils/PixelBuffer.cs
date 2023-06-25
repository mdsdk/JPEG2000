using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Utils
{
    public class PixelBuffer
    {
        public int Width { get; }

        public int Height { get; }

        public int NComponents { get; }
        
        public int[] Data { get; }

        public PixelBuffer(int width, int height, int nComponents)
        {
            Width = width;
            Height = height;
            NComponents = nComponents;
            Data = new int[Width * Height * NComponents];
        }

        [MethodImpl(HotspotMethodImplOptions)]
        public void SetPixelValue(int horizontalSubSamplingFactor, int verticalSubSamplingFactor, 
            int x, int y, int component, int value)
        {
            var i0 = x * horizontalSubSamplingFactor;
            var j0 = y * verticalSubSamplingFactor;

            ThrowIf((i0 < 0) || (i0 >= Width));
            ThrowIf((j0 < 0) || (j0 >= Height));

            for (var j = j0; j < j0 + verticalSubSamplingFactor; j++)
            {
                for (var i = i0; i < i0 + horizontalSubSamplingFactor; i++)
                {
                    Data[j * Width * NComponents + i * NComponents + component] = value;
                }
            }
        }
    }
}
