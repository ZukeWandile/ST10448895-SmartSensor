using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public class TelemetryHistoryItem//making the DataGrid easy to bind 
    {
        public string Time { get; set; } = string.Empty;

        public string Temperature { get; set; } = string.Empty;

        public string Power { get; set; } = string.Empty;

        public string Valve { get; set; } = string.Empty;
    }
}
