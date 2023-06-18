using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSDK.JPEG2000.Convert.JP2File
{
    interface IBox
    {
        public void ReadFrom(JP2FileReader reader);
    }
}
