// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Utils
{
    internal readonly struct Rectangle
    {
        public int X0 { get; }
        
        public int Y0 { get; }
        
        public int X1 { get; }

        public int Y1 { get; }

        public Rectangle(int x0, int y0, int x1, int y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        public int Width => X1 - X0;

        public int Height => Y1 - Y0;

        public Range XRange => new Range(X0, Width);

        public Range YRange => new Range(Y0, Height);

        public override string ToString()
        {
            return $"{XRange}x{YRange}";
        }
    }
}
