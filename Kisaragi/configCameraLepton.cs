using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace configCamera
{
    class cameraLepton
    {
        static public string convertTemp(ushort temp100K)
        {
            double tempC = (double)temp100K / 100.0 - 273.15;
            return string.Format("{0:0.00} ºC", tempC);
        }
    }
}
