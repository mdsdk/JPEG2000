using MDSDK.BinaryIO;
using MDSDK.JPEG2000.CodestreamSyntax;
using MDSDK.JPEG2000.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    public class JP2FileReader
    {
        internal BufferedStreamReader RawInput { get; }

        internal BinaryDataReader DataReader { get; }

        internal JP2HeaderBox Header { get; }

        internal int CodestreamLength { get; }

        public JP2FileReader(Stream stream)
        {
            RawInput = new BufferedStreamReader(stream);

            DataReader = new BinaryDataReader(RawInput, ByteOrder.BigEndian);

            var boxHeader = BoxHeader.ReadFrom(DataReader);
            var signatureBox = ReadBox<SignatureBox>(boxHeader);
            ThrowIf(signatureBox.Signature != 0x0D0A870A);

            boxHeader = BoxHeader.ReadFrom(DataReader);
            var fileTypeBox = ReadBox<FileTypeBox>(boxHeader);
            ThrowIf((fileTypeBox.Brand != "jp2 ") && (fileTypeBox.Brand != "jpx "));

            boxHeader = BoxHeader.ReadFrom(DataReader);
            while (boxHeader.BoxType != BoxType.ContiguousCodestream)
            {
                if (boxHeader.BoxType == BoxType.JP2Header)
                {
                    ThrowIf(Header != null);
                    Header = ReadBox<JP2HeaderBox>(boxHeader);
                }
                else
                {
                    ThrowIf(boxHeader.BoxDataLength < 0);
                    RawInput.SkipBytes(boxHeader.BoxDataLength);
                }
                boxHeader = BoxHeader.ReadFrom(DataReader);
            }

            CodestreamLength = boxHeader.BoxDataLength;
        }

        internal ImageDescriptor GetImageDescriptor()
        {
            return Header.CreateImageDescriptor();
        }

        internal IBox ReadBox(BoxHeader boxHeader)
        {
            var box = BoxFactory.CreateBox(boxHeader.BoxType);
            if (boxHeader.BoxDataLength < 0)
            {
                box.ReadFrom(this);
            }
            else
            {
                RawInput.Read(boxHeader.BoxDataLength, () =>
                {
                    box.ReadFrom(this);
                });
            }
            return box;
        }

        internal TBox ReadBox<TBox>(BoxHeader boxHeader) where TBox : IBox
        {
            var boxType = BoxFactory.GetBoxType<TBox>();
            ThrowIf(boxType != boxHeader.BoxType);
            return (TBox)ReadBox(boxHeader);
        }

        public int[] DecodeImage()
        {
            if (CodestreamLength < 0)
            {
                return JPEG2000Decoder.DecodeImage(RawInput);
            }
            else
            {
                int[] image = null;
                RawInput.Read(CodestreamLength, () =>
                {
                    image = JPEG2000Decoder.DecodeImage(RawInput);
                });
                return image;
            }
        }
    }
}
