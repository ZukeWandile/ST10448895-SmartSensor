using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sensorX.Models
{
    public static class SensorStore
    {// This static class holds a collection of Sensor objects that can be accessed throughout the application.
        public static ObservableCollection<Sensor> Sensors = new ObservableCollection<Sensor>();
    }
}