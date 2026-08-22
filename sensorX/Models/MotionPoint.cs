using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class MotionPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public MotionPoint? Previous { get; set; }
    }
}
