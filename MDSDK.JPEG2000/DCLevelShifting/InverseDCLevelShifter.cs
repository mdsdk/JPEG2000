// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.MultipleComponentTransformation
{
    abstract class InverseDCLevelShifter
    {
        public abstract void CopyToPixelDataBuffer(Component component, Signal2D a, int[] buffer, int offset, int count);

        public static InverseDCLevelShifter Create(ArithmeticType arithmeticType)
        {
            return arithmeticType switch
            {
                ArithmeticType.Int32 => new InverseInt32DCLevelShifter(),
                ArithmeticType.Single => new InverseSingleDCLevelShifter(),
                ArithmeticType.Double => new InverseDoubleDCLevelShifter(),
                _ => throw NotSupported(arithmeticType)
            };
        }
    }

    abstract class InverseDCLevelShifter<T> : InverseDCLevelShifter
    {
        public sealed override void CopyToPixelDataBuffer(Component component, Signal2D signal, int[] buffer, int offset, int count)
        {
            var a = (Signal2D<T>)signal;

            var siz = component.Image.Header.SIZ;

            ThrowIf(a.URange.Length != siz.XT_TileWidth);
            ThrowIf(a.VRange.Length != siz.YT_TileHeight);

            var nComponents = siz.C_NumberOfImageComponents;

            ThrowIf(count != nComponents * siz.XT_TileWidth * siz.YT_TileHeight);

            a.URange.GetBounds(out int u0, out int u1);
            a.VRange.GetBounds(out int v0, out int v1);

            var i = offset + component.ComponentIndex;
            for (var v = v0; v < v1; v++)
            {
                for (var u = u0; u < u1; u++)
                {
                    var sampleValue = PerformDCShift(a[u, v], component.DCShift);
                    if (sampleValue < component.MinSampleValue)
                    {
                        sampleValue = component.MinSampleValue;
                    }
                    else if (sampleValue > component.MaxSampleValue)
                    {
                        sampleValue = component.MaxSampleValue;
                    }
                    buffer[i] = sampleValue;
                    i += nComponents;
                }
            }
        }

        protected abstract int PerformDCShift(T value, int dcShift);
    }

    sealed class InverseInt32DCLevelShifter : InverseDCLevelShifter<int>
    {
        protected sealed override int PerformDCShift(int value, int dcShift) => value + dcShift;
    }

    sealed class InverseSingleDCLevelShifter : InverseDCLevelShifter<float>
    {
        protected sealed override int PerformDCShift(float value, int dcShift) => (int)Math.Round(value + dcShift);
    }

    sealed class InverseDoubleDCLevelShifter : InverseDCLevelShifter<double>
    {
        protected sealed override int PerformDCShift(double value, int dcShift) => (int)Math.Round(value + dcShift);
    }
}
