// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.JPEG2000.Model
{
    class Subband
    {
        public int SubbandIndex { get; }

        public int DecompositionLevel { get; }

        public SubbandType SubbandType { get; }

        public Subband(int subbandIndex, int decompositionLevel, SubbandType subbandType)
        {
            SubbandIndex = subbandIndex;
            DecompositionLevel = decompositionLevel;
            SubbandType = subbandType;
        }

        public override string ToString()
        {
            return $"{DecompositionLevel}{SubbandType}";
        }
    }
}
