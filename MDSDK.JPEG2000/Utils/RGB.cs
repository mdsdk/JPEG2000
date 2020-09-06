// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Utils
{
    struct RGB<T>
    {
        public T R;
        public T G;
        public T B;

        public RGB(T r, T g, T b)
        {
            R = r;
            G = g;
            B = b;
        }

        public bool Is<TOther>(out RGB<TOther> other) where TOther : T
        {
            if ((R is TOther r) && (G is TOther g) && (B is TOther b))
            {
                other = new RGB<TOther>(r, g, b);
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
