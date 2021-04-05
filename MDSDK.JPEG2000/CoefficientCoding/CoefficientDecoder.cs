// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.EntropyCoding;
using MDSDK.JPEG2000.Utils;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.CoefficientCoding
{
    class CoefficientDecoder
    {
        private CodeBlock CodeBlock { get; }

        private int Width { get; }

        private int Height { get; }

        private ArithmeticDecoder EntropyCoder { get; }

        private Coefficient[] CoefficientArray { get; }

        private Context[] ContextArray { get; }

        public CoefficientDecoder(CodeBlock codeBlock, byte[] bytes)
        {
            CodeBlock = codeBlock;

            Width = codeBlock.Bounds.Width;
            Height = codeBlock.Bounds.Height;

            CoefficientArray = new Coefficient[(Width + 2) * (Height + 2)];

            for (int y = -1; y < Height + 1; y++)
            {
                for (int x = -1; x < Width + 1; x++)
                {
                    CoefficientArray[GetCoefficientIndex(x, y)] = new Coefficient();
                }
            }

            EntropyCoder = new ArithmeticDecoder(bytes, 0);

            ContextArray = new Context[Context.Count];
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private int GetCoefficientIndex(int x, int y)
        {
            return (y + 1) * (Width + 2) + (x + 1);
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private ContextVector GetContextVector(int x, int y)
        {
            return CoefficientArray[GetCoefficientIndex(x, y)].ContextVector;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private Context GetContext(int contextLabel)
        {
            var cx = ContextArray[contextLabel];
            if (cx == null)
            {
                cx = new Context(contextLabel);
                ContextArray[contextLabel] = cx;
            }
            return cx;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private bool HasZeroContext(int x, int y)
        {
            var contextVector = GetContextVector(x, y);
            return contextVector.IsZero;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private sbyte DecodeSign(ContextVector contextVector, int x, int y)
        {
            contextVector.GetSignCodingContextLabelAndXorBit(out int contextLabel, out int xorBit);
            var cx = GetContext(contextLabel);
            var d = EntropyCoder.DecodeNextBit(cx);
            var signBit = d ^ xorBit;
            sbyte sign = (signBit == 0) ? 1 : -1;
            UpdateNeighbourContextVectors(x, y, sign);
            return sign;
        }

        private void UpdateNeighbourContextVectors(int x, int y, sbyte sign)
        {
            var left = x - 1;
            var right = x + 1;
            var above = y - 1;
            var below = y + 1;

            CoefficientArray[GetCoefficientIndex(left, y)].ContextVector.H_Right = sign;
            CoefficientArray[GetCoefficientIndex(right, y)].ContextVector.H_Left = sign;
            CoefficientArray[GetCoefficientIndex(x, above)].ContextVector.V_Below = sign;
            CoefficientArray[GetCoefficientIndex(x, below)].ContextVector.V_Above = sign;
            CoefficientArray[GetCoefficientIndex(left, above)].ContextVector.D_Right_Below = sign;
            CoefficientArray[GetCoefficientIndex(right, above)].ContextVector.D_Left_Below = sign;
            CoefficientArray[GetCoefficientIndex(left, below)].ContextVector.D_Right_Above = sign;
            CoefficientArray[GetCoefficientIndex(right, below)].ContextVector.D_Left_Above = sign;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private void RunSignificancePassOnCoefficient(int x, int y, bool includeZeroContext)
        {
            var coefficient = CoefficientArray[GetCoefficientIndex(x, y)];
            if (coefficient.Sign == 0)
            {
                var contextLabel = coefficient.ContextVector.GetSignificanceCodingContextLabel(CodeBlock.Subband.SubbandType);
                if ((contextLabel != 0) || includeZeroContext)
                {
                    var cx = GetContext(contextLabel);
                    if (EntropyCoder.DecodeNextBit(cx) == 1)
                    {
                        var sign = DecodeSign(coefficient.ContextVector, x, y);
                        coefficient.ApplySignificance(sign, CurrentCodingPass.BitPlane);
                    }
                    else
                    {
                        coefficient.ApplySignificance(0, CurrentCodingPass.BitPlane);
                    }
                }
            }
        }

        internal int[,] GetDecodedCoefficientValues()
        {
            var m = new int[Width, Height];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var coefficient = CoefficientArray[GetCoefficientIndex(x, y)];
                    m[x, y] = coefficient.GetValue();
                }
            }
            return m;
        }

        private CodingPass CurrentCodingPass { get; set; }

        internal void RunCodingPasses()
        {
            foreach (var codingPass in CodingPass.GetCodingPasses(CodeBlock.NCodingPasses))
            {
                CurrentCodingPass = codingPass;
                RunCurrentCodingPass();
            }
        }

        private void RunCurrentCodingPass()
        {
            switch (CurrentCodingPass.CodingPassType)
            {
                case CodingPassType.Cleanup:
                    RunCleanupPass();
                    break;
                case CodingPassType.Significance:
                    RunSignificancePass();
                    break;
                case CodingPassType.Refinement:
                    RunRefinementPass();
                    break;
                default:
                    throw new Exception("Invalid coding pass " + CurrentCodingPass);
            }
        }

        private delegate void BitScanner(int x, int y);

        [MethodImpl(HotspotMethodImplOptions)]
        private void RunCodingPass(BitScanner singleBitScanner, BitScanner quadBitScanner = null)
        {
            [MethodImpl(HotspotMethodImplOptions)]
            void DefaultQuadScanner(int x, int y)
            {
                singleBitScanner.Invoke(x, y + 0);
                singleBitScanner.Invoke(x, y + 1);
                singleBitScanner.Invoke(x, y + 2);
                singleBitScanner.Invoke(x, y + 3);
            }

            if (quadBitScanner == null)
            {
                quadBitScanner = DefaultQuadScanner;
            }

            var y = 0;
            while (y <= Height - 4)
            {         
                for (var x = 0; x < Width; x++)
                {
                    quadBitScanner.Invoke(x, y);
                }
                y += 4;
            }

            var n = Height - y;
            if (n > 0)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var i = 0; i < n; i++)
                    {
                        singleBitScanner.Invoke(x, y + i);
                    }
                }
            }
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private bool IsIncludedInCurrentCleanupPass(int x, int y)
        {
            var coefficient = CoefficientArray[GetCoefficientIndex(x, y)];
            return coefficient.LastScannedInBitPlane < CurrentCodingPass.BitPlane;
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private void RunCleanupPass()
        {
            [MethodImpl(HotspotMethodImplOptions)]
            bool IsIncludedInCleanupPassAndHasZeroContext(int x, int y)
            {
                return IsIncludedInCurrentCleanupPass(x, y) && HasZeroContext(x, y);
            }

            [MethodImpl(HotspotMethodImplOptions)]
            bool NextFourCoefficientsAreIncludedInCleanupPassAndHaveZeroContext(int x, int y)
            {
                Debug.Assert(y % 4 == 0);

                return IsIncludedInCleanupPassAndHasZeroContext(x, y + 0)
                    && IsIncludedInCleanupPassAndHasZeroContext(x, y + 1)
                    && IsIncludedInCleanupPassAndHasZeroContext(x, y + 2)
                    && IsIncludedInCleanupPassAndHasZeroContext(x, y + 3);
            }

            [MethodImpl(HotspotMethodImplOptions)]
            void SingleBitScanner(int x, int y)
            {
                if (IsIncludedInCurrentCleanupPass(x, y))
                {
                    RunSignificancePassOnCoefficient(x, y, includeZeroContext: true);
                }
            }

            [MethodImpl(HotspotMethodImplOptions)]
            void QuadBitScanner(int x, int y)
            {
                if (NextFourCoefficientsAreIncludedInCleanupPassAndHaveZeroContext(x, y))
                {
                    var cx = GetContext(Context.RunLength);
                    var atLeastOneOfNext4CoefficientsIsSignificant = EntropyCoder.DecodeNextBit(cx) == 1;
                    if (atLeastOneOfNext4CoefficientsIsSignificant)
                    {
                        cx = GetContext(Context.Uniform);
                        var b0 = EntropyCoder.DecodeNextBit(cx);
                        var b1 = EntropyCoder.DecodeNextBit(cx);
                        var i = (b0 << 1) | b1;
                        var coefficient = CoefficientArray[GetCoefficientIndex(x, y + i)];
                        var sign = DecodeSign(coefficient.ContextVector, x, y + i);
                        coefficient.ApplySignificance(sign, CurrentCodingPass.BitPlane);
                        for (var j = i + 1; j < 4; j++)
                        {
                            SingleBitScanner(x, y + j);
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < 4; i++)
                    {
                        SingleBitScanner(x, y + i);
                    }
                }
            }

            RunCodingPass(SingleBitScanner, QuadBitScanner);
        }

        [MethodImpl(HotspotMethodImplOptions)]
        public void RunSignificancePass()
        {
            [MethodImpl(HotspotMethodImplOptions)]
            void SingleBitScanner(int x, int y)
            {
                RunSignificancePassOnCoefficient(x, y, includeZeroContext: false);
            }

            RunCodingPass(SingleBitScanner);
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private void RunRefinementPass()
        {
            [MethodImpl(HotspotMethodImplOptions)]
            void SingleBitScanner(int x, int y)
            {
                var coefficient = CoefficientArray[GetCoefficientIndex(x, y)];
                if ((coefficient.Sign != 0) && (coefficient.LastScannedInBitPlane != CurrentCodingPass.BitPlane))
                {
                    var isFirstRefinementForThisCoefficient = coefficient.MagnitudeBits == 1;
                    var contextLabel = GetRefinementContextLabel(isFirstRefinementForThisCoefficient, x, y);
                    var cx = GetContext(contextLabel);
                    var magnitudeBit = EntropyCoder.DecodeNextBit(cx);
                    coefficient.ApplyRefinement(magnitudeBit, CurrentCodingPass.BitPlane);
                }
            }

            RunCodingPass(SingleBitScanner);
        }

        private int GetRefinementContextLabel(bool isFirstRefinementForThisCoefficient, int x, int y)
        {
            if (isFirstRefinementForThisCoefficient)
            {
                var contextVector = GetContextVector(x, y);
                return contextVector.GetRefinementCodingContextLabel();
            }
            else
            {
                return 16;
            }
        }

        public void Run(TilePartComponent tilePartComponent)
        {
            RunCodingPasses();

            var transformCoefficientValues = CodeBlock.SubbandPrecinct.TransformCoefficientValues;

            if (tilePartComponent.ArithmeticType == ArithmeticType.Int32)
            {
                StoreTransformCoefficientValues(tilePartComponent, (Signal2D<int>)transformCoefficientValues, o => o);
            }
            else if (tilePartComponent.ArithmeticType == ArithmeticType.Single)
            {
                StoreTransformCoefficientValues(tilePartComponent, (Signal2D<float>)transformCoefficientValues, o => o);
            }
            else if (tilePartComponent.ArithmeticType == ArithmeticType.Double)
            {
                StoreTransformCoefficientValues(tilePartComponent, (Signal2D<double>)transformCoefficientValues, o => o);
            }
        }

        [MethodImpl(HotspotMethodImplOptions)]
        private void StoreTransformCoefficientValues<T>(TilePartComponent tilePartComponent, Signal2D<T> transformCoefficientValues,
            Func<int, T> convert)
        {
            var nAllZeroBitPlanes = CodeBlock.NAllZeroBitPlanes_P.Value;

            var Mb = tilePartComponent.QCC.GetMb(CodeBlock.Subband);

            var xRange = CodeBlock.Bounds.XRange;
            var yRange = CodeBlock.Bounds.YRange;

            xRange.GetBounds(out int x0, out int x1);
            yRange.GetBounds(out int y0, out int y1);

            for (var y = y0; y < y1; y++)
            {
                for (var x = x0; x < x1; x++)
                {
                    var coefficient = CoefficientArray[GetCoefficientIndex(x - x0, y - y0)];
                    var q = coefficient.GetValue();
                    if (q != 0)
                    {
                        var Nb = nAllZeroBitPlanes + coefficient.LastScannedInBitPlane + 1;
                        Debug.Assert(Mb >= Nb);
                        q <<= Mb - Nb;
                        transformCoefficientValues[x, y] = convert(q);
                    }
                }
            }
        }
    }
}