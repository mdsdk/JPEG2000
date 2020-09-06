// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.WaveletTransformation
{
    abstract class InverseDiscreteWaveletTransformer
    {
        public bool PerformParallel1DSubbandReconstruction { get; set; } = true;

        public abstract WaveletTransform WaveletTransform { get; }

        public abstract void Perform2DSubbandReconstruction(Signal2D a, Signal2D aLL, Signal2D aHL, Signal2D aLH, Signal2D aHH);

        public abstract Signal2D CreateSignal2D(Range uRange, Range vRange);

        public static InverseDiscreteWaveletTransformer Create(WaveletTransform waveletTransform, int componentSamplePrecision)
        {
            ThrowIf(componentSamplePrecision > 16);

            if (waveletTransform == WaveletTransform.Reversible_5_3)
            {
                return new ReversibleInverseDiscreteWaveletTransformer();
            }
            else if (waveletTransform == WaveletTransform.Irreversible_9_7)
            {
                if (componentSamplePrecision <= 8)
                {
                    return new SinglePrecisionIrreversibleInverseDiscreteWaveletTransformer();
                }
                else
                {
                    return new DoublePrecisionIrreversibleInverseDiscreteWaveletTransformer();
                }
            }
            else
            {
                throw NotSupported(waveletTransform);
            }
        }
    }

    abstract class InverseDiscreteWaveletTransformer<T> : InverseDiscreteWaveletTransformer
    {
        public override void Perform2DSubbandReconstruction(Signal2D a, Signal2D aLL, Signal2D aHL, Signal2D aLH, Signal2D aHH)
        {
            Perform2DSubbandReconstruction((Signal2D<T>)a, (Signal2D<T>)aLL, (Signal2D<T>)aHL, (Signal2D<T>)aLH, (Signal2D<T>)aHH);
        }

        public override Signal2D CreateSignal2D(Range uRange, Range vRange)
        {
            return new Signal2D<T>(uRange, vRange);
        }

        public void Perform2DSubbandReconstruction(Signal2D<T> a, Signal2D<T> aLL, Signal2D<T> aHL, Signal2D<T> aLH, Signal2D<T> aHH)
        {
            Perform2DInterleave(a, aLL, aHL, aLH, aHH);

            if (PerformParallel1DSubbandReconstruction)
            {
                PerformParallelHorizontalSubbandReconstruction(a);
                PerformParallelVerticalSubbandReconstruction(a);
            }
            else
            {
                PerformHorizontalSubbandReconstruction(a);
                PerformVerticalSubbandReconstruction(a);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Perform2DInterleave(Signal2D<T> a, Signal2D<T> b, SubbandType subbandType)
        {
            static Range GetHalfRange(Range range, SubbandType.Filter filter)
            {
                return filter switch
                {
                    SubbandType.Filter.LowPass => Range.FromBounds(CeilDiv(range.Start, 2), CeilDiv(range.End, 2)),
                    SubbandType.Filter.HighPass => Range.FromBounds(FloorDiv(range.Start, 2), FloorDiv(range.End, 2)),
                    _ => throw NotSupported(filter),
                };
            }

            static int GetOffset(SubbandType.Filter filter)
            {
                return filter switch
                {
                    SubbandType.Filter.LowPass => 0,
                    SubbandType.Filter.HighPass => 1,
                    _ => throw NotSupported(filter),
                };
            }

            var halfURange = GetHalfRange(a.URange, subbandType.HorizontalFilter);
            var halfVRange = GetHalfRange(a.VRange, subbandType.VerticalFilter);

            var uOffset = GetOffset(subbandType.HorizontalFilter);
            var vOffset = GetOffset(subbandType.VerticalFilter);

            halfURange.GetBounds(out int hu0, out int hu1);
            halfVRange.GetBounds(out int hv0, out int hv1);

            for (var hv = hv0; hv < hv1; hv++)
            {
                for (var hu = hu0; hu < hu1; hu++)
                {
                    a[2 * hu + uOffset, 2 * hv + vOffset] = b[hu, hv];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Perform2DInterleave(Signal2D<T> a, Signal2D<T> aLL, Signal2D<T> aHL, Signal2D<T> aLH, Signal2D<T> aHH)
        {
            Perform2DInterleave(a, aLL, SubbandType.LL);
            Perform2DInterleave(a, aLH, SubbandType.LH);
            Perform2DInterleave(a, aHL, SubbandType.HL);
            Perform2DInterleave(a, aHH, SubbandType.HH);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PerformHorizontalSubbandReconstruction(Signal2D<T> a)
        {
            var extendedRange = Get1DExtendedRange(a.URange);

            var yExt = new Signal1D<T>(extendedRange);
            var xExt = new Signal1D<T>(extendedRange);

            a.VRange.GetBounds(out int v0, out int v1);

            for (var v = v0; v < v1; v++)
            {
                a.GetRow(v, yExt);
                Perform1DSubbandReconstruction(a.URange, yExt, xExt);
                a.SetRow(v, xExt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PerformVerticalSubbandReconstruction(Signal2D<T> a)
        {
            var extendedRange = Get1DExtendedRange(a.VRange);

            var yExt = new Signal1D<T>(extendedRange);
            var xExt = new Signal1D<T>(extendedRange);

            a.URange.GetBounds(out int u0, out int u1);

            for (var u = u0; u < u1; u++)
            {
                a.GetColumn(u, yExt);
                Perform1DSubbandReconstruction(a.VRange, yExt, xExt);
                a.SetColumn(u, xExt);
            }
        }

        struct ThreadLocal1DSubbandReconstructionState
        {
            public Signal1D<T> YExt { get; }

            public Signal1D<T> XExt { get; }

            public ThreadLocal1DSubbandReconstructionState(Range extendedRange)
            {
                YExt = new Signal1D<T>(extendedRange);
                XExt = new Signal1D<T>(extendedRange);
            }
        }

        private void PerformParallelHorizontalSubbandReconstruction(Signal2D<T> a)
        {
            var extendedRange = Get1DExtendedRange(a.URange);

            a.VRange.GetBounds(out int v0, out int v1);

            Parallel.For(v0, v1, () => new ThreadLocal1DSubbandReconstructionState(extendedRange), (v, pls, state) =>
            {
                a.GetRow(v, state.YExt);
                Perform1DSubbandReconstruction(a.URange, state.YExt, state.XExt);
                a.SetRow(v, state.XExt);
                return state;
            }, state => { });
        }

        private void PerformParallelVerticalSubbandReconstruction(Signal2D<T> a)
        {
            var extendedRange = Get1DExtendedRange(a.VRange);

            a.URange.GetBounds(out int u0, out int u1);

            Parallel.For(u0, u1, () => new ThreadLocal1DSubbandReconstructionState(extendedRange), (u, pls, state) =>
            {
                a.GetColumn(u, state.YExt);
                Perform1DSubbandReconstruction(a.VRange, state.YExt, state.XExt);
                a.SetColumn(u, state.XExt);
                return state;
            }, state => { });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Range Get1DExtendedRange(Range range)
        {
            range.GetBounds(out int i0, out int i1);

            int iLeft;
            int iRight;

            switch (WaveletTransform)
            {
                case WaveletTransform.Reversible_5_3:
                    iLeft = IsEven(i0) ? 1 : 2;
                    iRight = IsOdd(i1) ? 1 : 2;
                    break;
                case WaveletTransform.Irreversible_9_7:
                    iLeft = IsEven(i0) ? 3 : 4;
                    iRight = IsOdd(i1) ? 3 : 4;
                    break;
                default:
                    throw NotSupported(WaveletTransform);
            }

            return Range.FromBounds(i0 - iLeft, i1 + iRight);
        }

        protected abstract T DivideBy2(T value);

        protected abstract void Apply1DFilter(Range range, Signal1D<T> yExt, Signal1D<T> xExt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Perform1DSubbandReconstruction(Range range, Signal1D<T> yExt, Signal1D<T> xExt)
        {
            range.GetBounds(out int i0, out int i1);
            if (i0 == i1 - 1)
            {
                if (IsEven(i0))
                {
                    xExt[i0] = yExt[i0];
                }
                else
                {
                    xExt[i0] = DivideBy2(yExt[i0]);
                }
            }
            else
            {
                Signal1D<T>.ExtendPeriodicSymmetrically(range, yExt);
                Apply1DFilter(range, yExt, xExt);
            }
        }
    }

    sealed class ReversibleInverseDiscreteWaveletTransformer : InverseDiscreteWaveletTransformer<int>
    {
        public sealed override WaveletTransform WaveletTransform => WaveletTransform.Reversible_5_3;

        protected sealed override int DivideBy2(int value) => value / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void Apply1DFilter(Range range, Signal1D<int> yExt, Signal1D<int> xExt)
        {
            range.GetBounds(out int i0, out int i1);

            var n0 = FloorDiv(i0, 2);
            var n1 = FloorDiv(i1, 2);

            // Equation F.5

            for (var n = n0; n < n1 + 1; n++)
            {
                xExt[2 * n] = yExt[2 * n] - FloorDiv(yExt[2 * n - 1] + yExt[2 * n + 1] + 2, 4);
            }

            // Equation F.6

            for (var n = n0; n < n1; n++)
            {
                xExt[2 * n + 1] = yExt[2 * n + 1] + FloorDiv(xExt[2 * n] + xExt[2 * n + 2], 2);
            }
        }
    }

    abstract class IrreversibleInverseDiscreteWaveletTransformer<T> : InverseDiscreteWaveletTransformer<T>
    {
        public sealed override WaveletTransform WaveletTransform => WaveletTransform.Irreversible_9_7;
    }

    sealed class SinglePrecisionIrreversibleInverseDiscreteWaveletTransformer : IrreversibleInverseDiscreteWaveletTransformer<float>
    {
        protected sealed override float DivideBy2(float value) => value / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void Apply1DFilter(Range range, Signal1D<float> yExt, Signal1D<float> xExt)
        {
            range.GetBounds(out int i0, out int i1);

            // Parameters defined in Table F.4

            const float alpha = (float)-1.586134342059924;
            const float beta = (float)-0.052980118572961;
            const float gamma = (float)0.882911075530934;
            const float delta = (float)0.443506852043971;
            const float kappa = (float)1.230174104914001;

            var n0 = FloorDiv(i0, 2);
            var n1 = FloorDiv(i1, 2);

            // Equation F.7 - Step 1

            for (var n = n0 - 1; n < n1 + 2; n++)
            {
                xExt[2 * n] = kappa * yExt[2 * n];
            }

            // Equation F.7 - Step 2

            for (var n = n0 - 2; n < n1 + 2; n++)
            {
                xExt[2 * n + 1] = (1 / kappa) * yExt[2 * n + 1];
            }

            // Equation F.7 - Step 3

            for (var n = n0 - 1; n < n1 + 2; n++)
            {
                xExt[2 * n] = xExt[2 * n] - delta * (xExt[2 * n - 1] + xExt[2 * n + 1]);
            }

            // Equation F.7 - Step 4

            for (var n = n0 - 1; n < n1 + 1; n++)
            {
                xExt[2 * n + 1] = xExt[2 * n + 1] - gamma * (xExt[2 * n] + xExt[2 * n + 2]);
            }

            // Equation F.7 - Step 5

            for (var n = n0; n < n1 + 1; n++)
            {
                xExt[2 * n] = xExt[2 * n] - beta * (xExt[2 * n - 1] + xExt[2 * n + 1]);
            }

            // Equation F.7 - Step 6

            for (var n = n0; n < n1; n++)
            {
                xExt[2 * n + 1] = xExt[2 * n + 1] - alpha * (xExt[2 * n] + xExt[2 * n + 2]);
            }
        }
    }

    sealed class DoublePrecisionIrreversibleInverseDiscreteWaveletTransformer : IrreversibleInverseDiscreteWaveletTransformer<double>
    {
        protected sealed override double DivideBy2(double value) => value / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void Apply1DFilter(Range range, Signal1D<double> yExt, Signal1D<double> xExt)
        {
            range.GetBounds(out int i0, out int i1);

            // Parameters defined in Table F.4

            const double alpha = -1.586134342059924;
            const double beta = -0.052980118572961;
            const double gamma = 0.882911075530934;
            const double delta = 0.443506852043971;
            const double kappa = 1.230174104914001;

            var n0 = FloorDiv(i0, 2);
            var n1 = FloorDiv(i1, 2);

            // Equation F.7 - Step 1

            for (var n = n0 - 1; n < n1 + 2; n++)
            {
                xExt[2 * n] = kappa * yExt[2 * n];
            }

            // Equation F.7 - Step 2

            for (var n = n0 - 2; n < n1 + 2; n++)
            {
                xExt[2 * n + 1] = (1 / kappa) * yExt[2 * n + 1];
            }

            // Equation F.7 - Step 3

            for (var n = n0 - 1; n < n1 + 2; n++)
            {
                xExt[2 * n] = xExt[2 * n] - delta * (xExt[2 * n - 1] + xExt[2 * n + 1]);
            }

            // Equation F.7 - Step 4

            for (var n = n0 - 1; n < n1 + 1; n++)
            {
                xExt[2 * n + 1] = xExt[2 * n + 1] - gamma * (xExt[2 * n] + xExt[2 * n + 2]);
            }

            // Equation F.7 - Step 5

            for (var n = n0; n < n1 + 1; n++)
            {
                xExt[2 * n] = xExt[2 * n] - beta * (xExt[2 * n - 1] + xExt[2 * n + 1]);
            }

            // Equation F.7 - Step 6

            for (var n = n0; n < n1; n++)
            {
                xExt[2 * n + 1] = xExt[2 * n + 1] - alpha * (xExt[2 * n] + xExt[2 * n + 2]);
            }
        }
    }
}
