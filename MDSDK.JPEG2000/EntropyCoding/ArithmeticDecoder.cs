// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Runtime.CompilerServices;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.EntropyCoding
{
    class ArithmeticDecoder
    {
        private static QeTableRow[] QeTable { get; } = QeTableRow.GetQeTableRows();

        private byte[] Bytes { get; }

        private int BP { get; set; }

        private byte B => Bytes[BP];

        private byte B1 => Bytes[BP + 1];

        private uint CT { get; set; }

        private uint C { get; set; }

        private ushort A { get; set; }

        public ArithmeticDecoder(byte[] bytes, int bpst)
        {
            Bytes = bytes;

            BP = bpst;
            C = (uint)(B << 16);

            ByteIn();

            C <<= 7;
            CT -= 7;
            A = 0x8000;
        }

        private void ByteIn()
        {
            if (B != 0xFF)
            {
                BP++;
                C = (uint)(C + (B << 8));
                CT = 8;
            }
            else if (B1 > 0x8F)
            {
                HandleMarkerCode(B1);
                C += 0xFF00;
                CT = 8;
            }
            else
            {
                BP++;
                C = (uint)(C + (B << 9));
                CT = 7;
            }
        }

        private static void HandleMarkerCode(byte markerCodeSuffix)
        {
            if (markerCodeSuffix != 0xFF)
            {
                throw NotSupported(markerCodeSuffix);
            }
        }

        private ushort Chigh
        {
            get => (ushort)(C >> 16);
            set => C = (uint)(value << 16) | (C & 0x0000FFFFU);
        }

        [MethodImpl(HotspotMethodImplOptions)]
        public int DecodeNextBit(Context ctx)
        {
            var qeTableRow_CTX_I = QeTable[ctx.I];

            var Qe_CTX_I = qeTableRow_CTX_I.QeValue;

            A -= Qe_CTX_I;

            int d;

            if (Chigh >= Qe_CTX_I)
            {
                Chigh -= Qe_CTX_I;
                if ((A & 0x8000) == 0)
                {
                    MPSExchange(qeTableRow_CTX_I, ctx, out d);
                    Renormalize();
                }
                else
                {
                    d = ctx.MPS;
                }
            }
            else
            {
                LPSExchange(qeTableRow_CTX_I, ctx, out d);
                Renormalize();
            }

            return d;
        }

        private static void Exchange(bool flip, QeTableRow qeTableRow_CX_I, Context cx, out int d)
        {
            if (!flip)
            {
                d = cx.MPS;
                cx.I = qeTableRow_CX_I.NextMPSIndex;
            }
            else
            {
                d = (cx.MPS == 0) ? 1 : 0;
                if (qeTableRow_CX_I.Switch)
                {
                    cx.MPS = d;
                }
                cx.I = qeTableRow_CX_I.NextLPSIndex;
            }
        }

        private void MPSExchange(QeTableRow qeTableRow_CTX_I, Context cx, out int d)
        {
            var Qe_CX_I = qeTableRow_CTX_I.QeValue;
            Exchange(A < Qe_CX_I, qeTableRow_CTX_I, cx, out d);
        }

        private void LPSExchange(QeTableRow qeTableRow_CTX_I, Context cx, out int d)
        {
            var Qe_CX_I = qeTableRow_CTX_I.QeValue;
            Exchange(A >= Qe_CX_I, qeTableRow_CTX_I, cx, out d);
            A = Qe_CX_I;
        }

        private void Renormalize()
        {
            do
            {
                if (CT == 0)
                {
                    ByteIn();
                }
                A <<= 1;
                C <<= 1;
                CT--;
            }
            while ((A & 0x8000) == 0);
        }

        public int BytesLeft => Bytes.Length - BP;
    }
}
