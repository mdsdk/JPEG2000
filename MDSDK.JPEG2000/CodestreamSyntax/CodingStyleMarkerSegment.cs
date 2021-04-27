// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000.Model;
using MDSDK.JPEG2000.Utils;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    abstract class CodingStyleMarkerSegment : IMarkerSegment
    {
        public CodingStyle S_CodingStyle { get; protected set; }

        public byte SP_NumberOfDecompositionLevels { get; protected set; }

        public byte SP_CodeBlockWidthExponentOffsetValue { get; protected set; }

        public byte SP_CodeBlockHeightExponentOffsetValue { get; protected set; }

        public CodeBlockCodingPassStyle SP_CodeBlockCodingPassStyle { get; protected set; }

        public WaveletTransform SP_WaveletTransformUsed { get; protected set; }

        public PrecinctSize[] SP_ResolutionPrecinctSizes { get; private set; }

        public abstract void ReadFrom(CodestreamReader input);

        protected void Read_S_CodingStyle(BinaryDataReader dataReader)
        {
            S_CodingStyle = (CodingStyle)dataReader.ReadByte();
        }

        protected void Read_SP_Parameters(BinaryDataReader dataReader)
        {
            SP_NumberOfDecompositionLevels = dataReader.ReadByte();
            SP_CodeBlockWidthExponentOffsetValue = dataReader.ReadByte();
            SP_CodeBlockHeightExponentOffsetValue = dataReader.ReadByte();
            SP_CodeBlockCodingPassStyle = (CodeBlockCodingPassStyle)dataReader.ReadByte();
            SP_WaveletTransformUsed = (WaveletTransform)dataReader.ReadByte();

            SP_ResolutionPrecinctSizes = new PrecinctSize[SP_NumberOfDecompositionLevels + 1];

            for (var i = 0; i < SP_ResolutionPrecinctSizes.Length; i++)
            {
                if ((S_CodingStyle & CodingStyle.CustomPrecinctSizesUsed) != 0)
                {
                    var pps = dataReader.ReadByte();
                    SP_ResolutionPrecinctSizes[i] = new PrecinctSize(pps & 0x0F, pps >> 4);
                }
                else
                {
                    SP_ResolutionPrecinctSizes[i] = new PrecinctSize(15, 15);
                }
            }
        }
    }
}
