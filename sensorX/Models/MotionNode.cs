using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class MotionNode//creating a simple hierarchy object
    {
        public string Name { get; set; }

        public MotionNode? Parent { get; set; }

        public MotionNode(string name)
        {
            Name = name;
        }
    }
}
