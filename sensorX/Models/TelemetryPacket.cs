using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    // A generic struct that represents a telemetry packet containing a timestamp and a value of type T.
    public struct TelemetryPacket<T> where T : struct 
    {
        public DateTime Timestamp { get; }
        public T Value { get; }

        public TelemetryPacket(T value)
        {
            Timestamp = DateTime.Now;
            Value = value;
        }
    }
}
