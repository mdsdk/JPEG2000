// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Threading.Tasks;
using MDSDK.JPEG2000.Utils;
using MDSDK.JPEG2000.Model;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Quantization
{
    abstract class Dequantisizer
    {
        public abstract void Dequantisize(Signal2D transformCoefficientValues, TilePartComponent tilePartComponent, Subband subband);
    }

    abstract class Dequantisizer<T> : Dequantisizer
    {
        public override void Dequantisize(Signal2D transformCoefficientValues, TilePartComponent tilePartComponent, Subband subband)
        {
            Dequantisize((Signal2D<T>)transformCoefficientValues, tilePartComponent, subband);
        }

        protected abstract void Dequantisize(Signal2D<T> transformCoefficientValues, TilePartComponent tilePartComponent, Subband subband);
    }

    sealed class ReversibleDequantisizer<T> : Dequantisizer<T>
    {
        protected sealed override void Dequantisize(Signal2D<T> transformCoefficientValues, TilePartComponent tilePartComponent, Subband subband)
        {
            // Nothing to do
        }
    }

    abstract class IrreversibleDequantisizer<T> : Dequantisizer<T>
    {
        private static double GetQuantizationStepSize(TilePartComponent tilePartComponent, Subband subband)
        {
            var qcc = tilePartComponent.QCC;

            ThrowIf(qcc.QuantizationStyle == QuantizationStyle.NoQuantization);

            // See E.1.1.1 "Determination of the quantization step size"

            var ri = tilePartComponent.TileComponent.Component.ComponentSampleBitDepth;

            var rb = ri + GetLog2SubbandGain(subband);

            if (qcc.QuantizationStyle == QuantizationStyle.ScalarExpounded)
            {
                var qss = qcc.QuantizationStepSizes[subband.SubbandIndex];
                return Math.Pow(2, rb - qss.Exponent) * (1 + qss.Mantissa / Math.Pow(2, 11));
            }
            else
            {
                throw NotSupported(qcc.QuantizationStyle);
            }
        }

        private static int GetLog2SubbandGain(Subband subband)
        {
            // See Table E.1 "Sub-band gains"

            if (subband.SubbandType == SubbandType.LL)
            {
                return 0;
            }
            else if (subband.SubbandType == SubbandType.LH)
            {
                return 1;
            }
            else if (subband.SubbandType == SubbandType.HL)
            {
                return 1;
            }
            else if (subband.SubbandType == SubbandType.HH)
            {
                return 2;
            }
            else
            {
                throw NotSupported(subband.SubbandType);
            }
        }

        protected abstract void ApplyQuantizationStepSize(Signal2D<T> transformCoefficientValues, int u, int v, double quantizationStepSize);

        protected sealed override void Dequantisize(Signal2D<T> transformCoefficientValues, TilePartComponent component, Subband subband)
        {
            var quantizationStepSize = GetQuantizationStepSize(component, subband);
            if (quantizationStepSize != 1)
            {
                transformCoefficientValues.URange.GetBounds(out int u0, out int u1);
                transformCoefficientValues.VRange.GetBounds(out int v0, out int v1);

                Parallel.For(v0, v1, v =>
                {
                    for (var u = u0; u < u1; u++)
                    {
                        ApplyQuantizationStepSize(transformCoefficientValues, u, v, quantizationStepSize);
                    }
                });
            }
        }
    }

    sealed class SinglePrecisionIrreversibleDequantisizer : IrreversibleDequantisizer<float>
    {
        protected sealed override void ApplyQuantizationStepSize(Signal2D<float> transformCoefficientValues, int u, int v,
            double quantizationStepSize)
        {
            transformCoefficientValues[u, v] *= (float)quantizationStepSize;
        }
    }

    sealed class DoublePrecisionIrreversibleDequantisizer : IrreversibleDequantisizer<double>
    {
        protected sealed override void ApplyQuantizationStepSize(Signal2D<double> transformCoefficientValues, int u, int v,
            double quantizationStepSize)
        {
            transformCoefficientValues[u, v] *= quantizationStepSize;
        }
    }
}
