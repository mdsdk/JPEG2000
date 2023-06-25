// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.MultipleComponentTransformation;
using MDSDK.JPEG2000.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000
{
    public static class JPEG2000Decoder
    {
        private static T GetCommonProperty<T>(TilePartComponent[] tilePartComponents, Func<TilePartComponent, T> getProperty)
        {
            var property = getProperty(tilePartComponents[0]);
            ThrowIf(tilePartComponents.Skip(1).Any(tpc => !EqualityComparer<T>.Default.Equals(getProperty(tpc), property)));
            return property;
        }

        private static RGB<Signal2D> ReconstructImageUsingMultiComponentTransform(TilePartComponent[] tilePartComponents)
        {
            ThrowIf(tilePartComponents.Length != 3);

            var yCbCr = new YCbCr<Signal2D>
            {
                Y = tilePartComponents[0].ReconstructImageSampleValues(),
                Cb = tilePartComponents[1].ReconstructImageSampleValues(),
                Cr = tilePartComponents[2].ReconstructImageSampleValues()
            };

            var waveletTransformUsed = GetCommonProperty(tilePartComponents, tpc => tpc.TilePart.Header.COD.SP_WaveletTransformUsed);
            var arithmeticType = GetCommonProperty(tilePartComponents, tpc => tpc.ArithmeticType);

            var imct = InverseMultipleComponentTransformer.Create(waveletTransformUsed, arithmeticType);
            return imct.Transform(yCbCr);
        }

        static void DecodeImage(Image image, TilePartComponent[] tilePartComponents, PixelBuffer pixelBuffer)
        {
            var cod = image.Header.COD;
            if (cod.SG_MultipleComponentTransform == MultipleComponentTransform.Components_0_1_2)
            {
                var rgb = ReconstructImageUsingMultiComponentTransform(tilePartComponents);
                CopyToPixelDataBuffer(tilePartComponents[0], rgb.R, pixelBuffer);
                CopyToPixelDataBuffer(tilePartComponents[1], rgb.G, pixelBuffer);
                CopyToPixelDataBuffer(tilePartComponents[2], rgb.B, pixelBuffer);
            }
            else if (cod.SG_MultipleComponentTransform == MultipleComponentTransform.None)
            {
                foreach (var tilePartComponent in tilePartComponents)
                {
                    var a = tilePartComponent.ReconstructImageSampleValues();
                    CopyToPixelDataBuffer(tilePartComponent, a, pixelBuffer);
                }
            }
            else
            {
                throw NotSupported(cod.SG_MultipleComponentTransform);
            }
        }

        private static void CopyToPixelDataBuffer(TilePartComponent tilePartComponent, Signal2D a, PixelBuffer pixelBuffer)
        {
            var inverseDCLevelShifter = InverseDCLevelShifter.Create(tilePartComponent.ArithmeticType);
            inverseDCLevelShifter.CopyToPixelBuffer(tilePartComponent.TileComponent.Component, a, pixelBuffer);
        }

        public static int[] DecodeImage(BufferedStreamReader input)
        {
            var codestreamReader = new CodestreamReader(input);
            var tilePartComponents = codestreamReader.DecodeCodeStream();
            var image = codestreamReader.Image;
            var siz = image.Header.SIZ;
            var pixelBuffer = new PixelBuffer(siz.XT_TileWidth, siz.YT_TileHeight, siz.C_NumberOfImageComponents);
            DecodeImage(image, tilePartComponents, pixelBuffer);
            return pixelBuffer.Data;
        }
    }
}
