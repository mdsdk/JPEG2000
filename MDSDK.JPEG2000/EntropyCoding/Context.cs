// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.EntropyCoding
{
    class Context
    {
        public const int Zero = 0;
        public const int RunLength = 17;
        public const int Uniform = 18;
        public const int Count = 19;

        public int Label { get; }

        public int I { get; set; }

        public Context(int label)
        {
            Label = label;
            
            I = label switch
            {
                Zero => 4,
                RunLength => 3,
                Uniform => 46,
                _ => 0,
            };
        }

        public int MPS { get; set; }
    }
}
