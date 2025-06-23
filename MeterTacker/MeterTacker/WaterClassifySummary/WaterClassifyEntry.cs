using System;

namespace MeterTacker.WaterClassifySummary
{
    public class WaterClassifyModel
    {
        public float HighFlowRateConsumption { get; set; }
        public float LowFlowRateConsumption { get; set; }
        public float MediumFlowRateConsumption { get; set; }
        public float Outliers { get; set; }
        public int MeterNumber { get; set; }            
        public string GatewayNumber { get; set; }        
        public float Consumption { get; set; }           
        public string Date { get; set; }                 
        public string Environment { get; set; }          
    }
}
