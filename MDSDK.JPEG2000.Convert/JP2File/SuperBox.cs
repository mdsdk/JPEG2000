// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Collections.Generic;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    abstract class SuperBox : IBox
    {
        public List<IBox> NestedBoxes { get; } = new List<IBox>();

        protected abstract bool IsValidNestedBoxType(BoxType boxType);
        
        public void ReadFrom(JP2FileReader reader)
        {
            while (!reader.RawInput.AtEnd)
            {
                var boxHeader = BoxHeader.ReadFrom(reader.DataReader);
                ThrowIf(!IsValidNestedBoxType(boxHeader.BoxType));
                var box = reader.ReadBox(boxHeader);
                NestedBoxes.Add(box);
            }
        }
    }
}
