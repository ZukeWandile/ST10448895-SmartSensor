using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class MotionBatch
    {
        public float[][] RawPoints { get; set; } // 2D array to hold raw motion data points

        public MotionBatch(float[][] rawPoints)
        {
            RawPoints = rawPoints;// Initialize the RawPoints property with the provided 2D array
        }
    }
}
