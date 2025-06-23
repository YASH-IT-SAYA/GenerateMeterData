using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterTacker.Models
{
    public class WaterMeterStatusModel
    {
        public long Id { get; set; }
        public string MeterNumber { get; set; }
        public string Gw { get; set; }
        public double? Temperature { get; set; }
        public double? Pressure { get; set; }
        public bool? BurstDetected { get; set; }
        public bool? LeakDetected { get; set; }
        public bool? OverFlowDetected { get; set; }
        public bool? ValveOpen { get; set; }
        public bool? FlowSensorFunctional { get; set; }
        public bool? TemperatureSensorFunctional { get; set; }
        public bool? BatteryStatus { get; set; }
        public bool? ValveMalFunction { get; set; }
        public bool? CloseValveError { get; set; }
        public bool? OpenValveError { get; set; }
        public DateTime MeterLocalTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? MeterLocalDate { get; set; }
        public bool? ValvePartialOpen { get; set; }
        public int? Vop { get; set; }
        public bool? Ep { get; set; }
    }

}
