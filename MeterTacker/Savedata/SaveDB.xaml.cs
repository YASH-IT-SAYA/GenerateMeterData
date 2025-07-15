using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MeterTacker.CheckoutData;
using MeterTacker.Helper;
using Npgsql;
using System.Configuration;
using log4net;

namespace MeterTacker.Savedata
{
    public partial class SaveDB : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string developmentEnvironment = ConfigurationManager.ConnectionStrings["developmentEnvironment"].ConnectionString;
        private string testingEnvironment = ConfigurationManager.ConnectionStrings["testingEnvironment"].ConnectionString;

        private readonly Dictionary<string, string> tableNameMapping = new Dictionary<string, string>()
        {
            { "get_water_status_filtered", "\"WaterMeterStatusReportLatest\"" },
            { "daily_meter_vise_cons_raw", "\"DailyMeterViseCons\"" },
            { "water_meter_flow_report_latest", "\"WaterMeterFlowReportLatest\"" }
        };
        private readonly Dictionary<string, Dictionary<string, string>> columnMappings = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "get_water_status_filtered", new Dictionary<string, string>()
                {
                    { "id", "id" },
                    { "meterLocalTime", "meterLocalTime" },
                    {"temperature","temperature" },
                    {"pressure","pressure" },
                    {"burstDetected","burstDetected" },
                    {"leakDetected","leakDetected" },
                    {"overFlowDetected","overFlowDetected" },
                    {"valveOpen","valveOpen" },
                    {"flowSensorFunctional","flowSensorFunctional" },
                    {"temperatureSensorFunctional","temperatureSensorFunctional" },
                    {"batteryStatus","batteryStatus" },
                    {"valveMalFunction","valveMalFunction" },
                    {"closeValveError","closeValveError" },
                    {"openValveError","openValveError" },
                    { "createdDate", "createdDate" },
                    { "valvepartialopen", "valvepartialopen" },
                    { "ep", "ep" },
                    { "vop", "vop" },
                    { "meterLocalDate", "meterLocalDate" },
                    { "meterNumber", "meterNumber" },
                    { "gw", "gw" }
                }
            },
            {
                "daily_meter_vise_cons_raw", new Dictionary<string, string>()
                {
                    { "TodayDate", "TodayDate" },
                    { "CreatedDate", "CreatedDate" },
                    { "Consumption", "Consumption" },
                    { "Temperature", "Temperature" },
                    { "Pressure", "Pressure" },
                    { "MeterNumber", "MeterNumber" },
                    { "GatewayMac", "GatewayMac" },
                    { "RowCount", "RowCount" },
                    { "TempRowCount", "TempRowCount" },
                    { "PressRowCount", "PressRowCount" },
                    { "LastMeterReading", "LastMeterReading" },
                }
            },
            {
                "water_meter_flow_report_latest", new Dictionary<string, string>()
                {
                    {"id","id" },
                    {"flow","flow" },
                    {"tf","tf" },
                    {"fr","fr" },
                    {"fim","fim" },
                    {"u","u" },
                    {"totalflowinft3","totalflowinft3" },
                    {"flowinft3","flowinft3" },
                    {"flowrateinft3","flowrateinft3" },
                    {"isDisplay","isDisplay" },
                    { "meterLocalTime", "meterLocalTime" },
                    { "createdDate", "createdDate" },
                    { "meterLocalDate", "meterLocalDate" },
                    { "meterNumber", "meterNumber" },
                    { "gw", "gw" },
                    { "previousflow", "previousflow" },
                    { "anomaly", "anomaly" },
                }
            }
        };

        public SaveDB()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Savebutton clicked");
            busyIndicator.IsBusy = true;
            try
            {

                if (!string.IsNullOrWhiteSpace(CommonList.selectedTableName))
                {


                    if (!tableNameMapping.ContainsKey(CommonList.selectedTableName))
                    {
                        log.Info("selected table does not have a mapped target table.");
                        MessageBox.Show("Selected table does not have a mapped target table.");
                        return;
                    }

                    string targetTableName = tableNameMapping[CommonList.selectedTableName];
                    Dictionary<string, string> columnMap = columnMappings[CommonList.selectedTableName];

                    DataTable dataTable = CommonList.allData.CopyToDataTable();

                    string selectedDb = ((ComboBoxItem)cmbTableName.SelectedItem)?.Content?.ToString();
                    string connStr = selectedDb == "Development Environment"
                        ? developmentEnvironment
                        : selectedDb == "Testing Environment"
                            ? testingEnvironment
                            : null;

                    if (string.IsNullOrEmpty(connStr))
                    {
                        log.Info("Select the valid environment");
                        MessageBox.Show("Please select a valid environment.");
                        return;
                    }

                    await Task.Run(() =>
                    {
                        SaveToDatabase(dataTable, targetTableName, columnMap, connStr);
                    });
                    busyIndicator.IsBusy = false;
                    log.Info("Data Successfully Saved");
                    MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    log.Info("Please select a table in the main window.");
                    MessageBox.Show("Please select a table in the main window.");
                }

            }
            catch (Exception ex)
            {
                log.Error($"Error while saving data: {ex.Message}");
                MessageBox.Show($"Error while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveToDatabase(DataTable data, string targetTable, Dictionary<string, string> columnMap, string connStr)
        {
            try
            {
                string targetColumns = string.Join(", ", columnMap.Values.Select(c => $"\"{c}\""));
                string copyCommand = $"COPY {targetTable} ({targetColumns}) FROM STDIN (FORMAT CSV)";
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    using (var writer = conn.BeginTextImport(copyCommand))
                    {
                        foreach (DataRow row in data.Rows)
                        {
                            var values = columnMap.Keys.Select(srcCol =>
                            {
                                if (!row.Table.Columns.Contains(srcCol)) return "";

                                object val = row[srcCol];
                                if (val == null || val == DBNull.Value) return "";

                                if (val is DateTime dt)
                                    return dt.ToString("yyyy-MM-dd HH:mm:ss");
                                return val.ToString().Replace(",", " ");
                            });
                            writer.WriteLine(string.Join(",", values));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An error occurred while saving data:\n{ex.Message}");
                MessageBox.Show($"An error occurred while saving data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
