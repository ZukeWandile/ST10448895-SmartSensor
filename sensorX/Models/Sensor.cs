using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class Sensor
    {
        public string MacAddress { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string NodeId { get; set; } = string.Empty;

        public string Status { get; set; } = "Online";

        public string DateRegistered { get; set; } = string.Empty;
    }
}
