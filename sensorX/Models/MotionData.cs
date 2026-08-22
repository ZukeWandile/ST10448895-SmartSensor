using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class MotionData
    {
        public bool PersonDetected { get; set; }

        public double Confidence { get; set; }

        public double XPosition { get; set; }

        public double YPosition { get; set; }

        public double MovementSpeed { get; set; }
    }
}
