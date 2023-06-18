// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    static class BoxFactory
    {
        public static readonly Dictionary<BoxType, Type> BoxTypeClasses = new Dictionary<BoxType, Type>
        {
            { BoxType.Signature, typeof(SignatureBox) },
            { BoxType.FileType, typeof(FileTypeBox) },
            { BoxType.JP2Header , typeof(JP2HeaderBox) },
            { BoxType.ImageHeader , typeof(ImageHeaderBox) },
            { BoxType.BitsPerComponent , typeof(BitsPerComponentBox) },
            { BoxType.ColorSpecification , typeof(ColourSpecificationBox) },
            { BoxType.Palette , typeof(PaletteBox) },
            { BoxType.ComponentMapping, typeof(ComponentMappingBox) },
            { BoxType.ChannelDefinition, typeof(ChannelDefinitionBox) },
            { BoxType.Resolution, typeof(ResolutionBox) },
            { BoxType.CaptureResolution, typeof(CaptureResolutionBox) },
            { BoxType.DefaultDisplayResolution, typeof(DefaultDisplayResolutionBox) },
            { BoxType.ContiguousCodestream, typeof(ContiguousCodestreamBox) },
            { BoxType.IntellectualProperty, typeof(IntellectualPropertyBox) },
            { BoxType.XML, typeof(XMLBox) },
            { BoxType.UUID, typeof(UUIDBox) },
            { BoxType.UUIDInfo, typeof(UUIDInfoBox) },
            { BoxType.UUIDList, typeof(UUIDListBox) },
            { BoxType.URL, typeof(URLBox) },
        };

        public static IBox CreateBox(BoxType boxType)
        {
            ThrowIf(!BoxTypeClasses.TryGetValue(boxType, out Type boxTypeClass));
            return (IBox)Activator.CreateInstance(boxTypeClass);
        }

        public static BoxType GetBoxType<TBox>() where TBox : IBox
        {
            foreach (var boxTypeClass in BoxTypeClasses)
            {
                if (boxTypeClass.Value == typeof(TBox))
                {
                    return boxTypeClass.Key;
                }
            }
            throw NotSupported(typeof(TBox));
        }
    }
}
