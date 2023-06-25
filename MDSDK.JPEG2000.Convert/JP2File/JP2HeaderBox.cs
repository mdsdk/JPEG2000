// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    class JP2HeaderBox : SuperBox
    {
        protected override bool IsValidNestedBoxType(BoxType boxType)
        {
            return (boxType == BoxType.ImageHeader)
                || (boxType == BoxType.BitsPerComponent)
                || (boxType == BoxType.ColorSpecification)
                || (boxType == BoxType.Palette)
                || (boxType == BoxType.ComponentMapping)
                || (boxType == BoxType.ChannelDefinition)
                || (boxType == BoxType.Resolution);
        }

        public ImageDescriptor CreateImageDescriptor()
        {
            var imageDescriptor = new ImageDescriptor();

            var imageHeader = NestedBoxes.OfType<ImageHeaderBox>().Single();

            ThrowIf(imageHeader.Width > int.MaxValue);
            ThrowIf(imageHeader.Height > int.MaxValue);
            ThrowIf(imageHeader.CompressionType != 7);

            imageDescriptor.Width = (int)imageHeader.Width;
            imageDescriptor.Height = (int)imageHeader.Height;

            var colourSpecification = NestedBoxes.OfType<ColourSpecificationBox>().First(
                o => o.EnumeratedColourspace != null);

            imageDescriptor.Colourspace = colourSpecification.EnumeratedColourspace.Value switch
            {
                16 => Colourspace.sRGB,
                17 => Colourspace.Greyscale,
                18 => Colourspace.sYCC,
                _ => imageHeader.NumberOfComponents switch
                {
                    1 => Colourspace.Greyscale,
                    3 => Colourspace.sRGB,
                    _=> throw NotSupported(colourSpecification.EnumeratedColourspace.Value)
                }
            };

            var bitsPerComponents = NestedBoxes.OfType<BitsPerComponentBox>().ToArray();

            imageDescriptor.Components = new ImageComponentDescriptor[imageHeader.NumberOfComponents];
            
            for (var i = 0; i < imageDescriptor.Components.Length; i++)
            {
                var bitsPerComponent = (imageHeader.BitsPerComponent == 255)
                    ? bitsPerComponents[i].BitsPerComponent
                    : imageHeader.BitsPerComponent;

                var component = new ImageComponentDescriptor
                {
                    BitDepth = (bitsPerComponent & 0x3F) + 1,
                    IsSigned = (bitsPerComponent & 0x80) != 0
                };

                imageDescriptor.Components[i] = component;
            }

            var channelDefinition = NestedBoxes.OfType<ChannelDefinitionBox>().SingleOrDefault();
            if (channelDefinition != null)
            {
                Debug.Assert(!NestedBoxes.OfType<ComponentMappingBox>().Any());
                foreach (var channelDescription in channelDefinition.ChannelDescriptions)
                {
                    imageDescriptor.Components[channelDescription.ChannelIndex].ChannelDescription = channelDescription;
                }
            }
            
            return imageDescriptor;
        }
    }
}
