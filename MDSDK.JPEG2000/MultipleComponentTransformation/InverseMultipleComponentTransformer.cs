// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System.Threading.Tasks;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.MultipleComponentTransformation
{
    abstract class InverseMultipleComponentTransformer
    {
        public abstract RGB<Signal2D> Transform(YCbCr<Signal2D> yCbCr);

        public static InverseMultipleComponentTransformer Create(WaveletTransform waveletTransformUsed, ArithmeticType arithmeticType)
        {
            if (waveletTransformUsed == WaveletTransform.Reversible_5_3)
            {
                return arithmeticType switch
                {
                    ArithmeticType.Int32 => new ReversibleInverseMultipleComponentTransformer(),
                    _ => throw NotSupported(arithmeticType)
                };
            }
            else if (waveletTransformUsed == WaveletTransform.Irreversible_9_7)
            {
                return arithmeticType switch
                {
                    ArithmeticType.Single => new SinglePrecisionIrreversibleInverseMultipleComponentTransformer(),
                    ArithmeticType.Double => new DoublePrecisionIrreversibleInverseMultipleComponentTransformer(),
                    _ => throw NotSupported(arithmeticType)
                };
            }
            else
            {
                throw NotSupported(waveletTransformUsed);
            }
        }
    }

    abstract class InverseMultipleComponentTransformer<T> : InverseMultipleComponentTransformer
    {
        public override RGB<Signal2D> Transform(YCbCr<Signal2D> yCbCr)
        {
            var uRange = yCbCr.Y.URange;
            var vRange = yCbCr.Y.VRange;

            uRange.GetBounds(out int u0, out int u1);
            vRange.GetBounds(out int v0, out int v1);

            var y = (Signal2D<T>)yCbCr.Y;
            var cb = (Signal2D<T>)yCbCr.Cb;
            var cr = (Signal2D<T>)yCbCr.Cr;

            var r = new Signal2D<T>(uRange, vRange);
            var g = new Signal2D<T>(uRange, vRange);
            var b = new Signal2D<T>(uRange, vRange);

            Parallel.For(v0, v1, v =>
            {
                for (var u = u0; u < u1; u++)
                {
                    var c = Transform(y[u, v], cb[u, v], cr[u, v]);
                    r[u, v] = c.R;
                    g[u, v] = c.G;
                    b[u, v] = c.B;
                }
            });

            return new RGB<Signal2D>(r, g, b);
        }

        protected abstract RGB<T> Transform(T y, T cb, T cr);
    }

    sealed class ReversibleInverseMultipleComponentTransformer : InverseMultipleComponentTransformer<int>
    {
        protected sealed override RGB<int> Transform(int y, int cb, int cr)
        {
            var rgb = new RGB<int>();
            rgb.G = y - FloorDiv(cr + cb, 4);
            rgb.R = cr + rgb.G;
            rgb.B = cb + rgb.G;
            return rgb;
        }
    }

    sealed class SinglePrecisionIrreversibleInverseMultipleComponentTransformer : InverseMultipleComponentTransformer<float>
    {
        protected sealed override RGB<float> Transform(float y, float cb, float cr)
        {
            var rgb = new RGB<float>();
            rgb.R = y + 1.402f * cr;
            rgb.G = y - 0.34413f * cb - 0.71414f * cr;
            rgb.B = y + 1.772f * cb;
            return rgb;
        }
    }

    sealed class DoublePrecisionIrreversibleInverseMultipleComponentTransformer : InverseMultipleComponentTransformer<double>
    {
        protected sealed override RGB<double> Transform(double y, double cb, double cr)
        {
            var rgb = new RGB<double>();
            rgb.R = y + 1.402 * cr;
            rgb.G = y - 0.34413 * cb - 0.71414 * cr;
            rgb.B = y + 1.772 * cb;
            return rgb;
        }
    }
}
