using MeterTacker.CheckoutData;
using System;
using System.Collections.Generic;
using System.Data;

namespace MeterTacker.Helper
{
    public class CommonList
    {
        public static List<DataRow> allData = new List<DataRow>();
        public static List<DataRow> originalData = new List<DataRow>();
        public static string oldMeterNumber { get; set; } = "";
        public static string newMeterNumber { get; set; } = "";
        public static string oldGateway { get; set; } = "";
        public static string newGateway { get; set; } = "";
        public static string selectedTableName { get; set; } = "";
        public static DateTime startDate { get; set; }
        public static DateTime endDate { get; set; }

        public static List<TableOption> TableOptions { get; } = new List<TableOption>
        {
            new TableOption { DisplayName = "-- Select Table --", ActualName = "" },
            new TableOption { DisplayName = "WaterMeterStatusReportLatest(Tempreature,Pressure)", ActualName = "get_water_status_filtered" },
            new TableOption { DisplayName = "DailyMeterViseCons(Week,Month,Year)", ActualName = "daily_meter_vise_cons_raw" },
            new TableOption { DisplayName = "WaterMeterFlowReportLatest(WaterConsumptionDayChart)", ActualName = "water_meter_flow_report_latest" }
        };
        public static void Reset()
        {
            allData.Clear();
            originalData.Clear();
            oldMeterNumber = "";
            newMeterNumber = "";
            oldGateway = "";
            newGateway = "";
            selectedTableName = "";
            startDate = default;
            endDate = default;
        }
    }

    public class TableOption
    {
        public string DisplayName { get; set; }
        public string ActualName { get; set; }
    }
}
