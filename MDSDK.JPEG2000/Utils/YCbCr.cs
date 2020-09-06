// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Utils
{
    struct YCbCr<T>
    {
        public T Y;
        public T Cb;
        public T Cr;

        public YCbCr(T y, T cb, T cr)
        {
            Y = y;
            Cb = cb;
            Cr = cr;
        }

        public bool Is<TOther>(out YCbCr<TOther> other) where TOther : T
        {
            if ((Y is TOther y) && (Cb is TOther cb) && (Cr is TOther cr))
            {
                other = new YCbCr<TOther>(y, cb, cr);
                return true;
            }
            else
            {
                other = default;
                return false;
            }
        }
    }
}
