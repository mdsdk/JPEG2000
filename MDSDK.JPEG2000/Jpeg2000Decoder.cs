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

        static void DecodeImage(Image image, TilePartComponent[] tilePartComponents, int[] buffer, int offset, int count)
        {
            var cod = image.Header.COD;
            if (cod.SG_MultipleComponentTransform == MultipleComponentTransform.Components_0_1_2)
            {
                var rgb = ReconstructImageUsingMultiComponentTransform(tilePartComponents);
                CopyToPixelDataBuffer(tilePartComponents[0], rgb.R, buffer, offset, count);
                CopyToPixelDataBuffer(tilePartComponents[1], rgb.G, buffer, offset, count);
                CopyToPixelDataBuffer(tilePartComponents[2], rgb.B, buffer, offset, count);
            }
            else if (cod.SG_MultipleComponentTransform == MultipleComponentTransform.None)
            {
                foreach (var tilePartComponent in tilePartComponents)
                {
                    var a = tilePartComponent.ReconstructImageSampleValues();
                    CopyToPixelDataBuffer(tilePartComponent, a, buffer, offset, count);
                }
            }
            else
            {
                throw NotSupported(cod.SG_MultipleComponentTransform);
            }
        }

        private static void CopyToPixelDataBuffer(TilePartComponent tilePartComponent, Signal2D a, int[] buffer, int offset, int count)
        {
            var inverseDCLevelShifter = InverseDCLevelShifter.Create(tilePartComponent.ArithmeticType);
            inverseDCLevelShifter.CopyToPixelDataBuffer(tilePartComponent.TileComponent.Component, a, buffer, offset, count);
        }

        public static int[] DecodeImage(BinaryStreamReader input)
        {
            var originalByteOrder = input.ByteOrder;
            input.ByteOrder = ByteOrder.BigEndian;
            try
            {
                var codestreamReader = new CodestreamReader(input);
                var tilePartComponents = codestreamReader.DecodeCodeStream();
                var image = codestreamReader.Image;
                var siz = image.Header.SIZ;
                var buffer = new int[siz.XT_TileWidth * siz.YT_TileHeight * siz.C_NumberOfImageComponents];
                DecodeImage(image, tilePartComponents, buffer, 0, buffer.Length);
                return buffer;
            }
            finally
            {
                input.ByteOrder = originalByteOrder;
            }
        }
    }
}
