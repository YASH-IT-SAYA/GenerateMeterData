﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MeterTacker.CheckoutData;
using Npgsql;

namespace MeterTacker.Savedata
{
    public partial class SaveDB : Window
    {

        private static readonly string developmentEnvironment = "Server=saya-dev2.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=sayadev;User Id=SayaDev;Password=duca$$0234;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0";
        private static readonly string testingEnvironment = "Server=saya-dev2.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=sayatesting;User Id=SayaDev;Password=duca$$0234;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0";

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
            busyIndicator.IsBusy = true;

            try
            {
                var mainWindow = Application.Current.Windows.OfType<GetData>().FirstOrDefault();
                if (mainWindow == null)
                {
                    MessageBox.Show("Main window not found.");
                    return;
                }

                if (mainWindow.cmbTableName.SelectedItem is TableOption selectedOption && !string.IsNullOrWhiteSpace(selectedOption.ActualName))
                {
                    string sourceTableName = selectedOption.ActualName;

                    if (!tableNameMapping.ContainsKey(sourceTableName))
                    {
                        MessageBox.Show("Selected table does not have a mapped target table.");
                        return;
                    }

                    string targetTableName = tableNameMapping[sourceTableName];
                    Dictionary<string, string> columnMap = columnMappings[sourceTableName];

                    DataTable dataTable = mainWindow.allData.CopyToDataTable();

                    string selectedDb = ((ComboBoxItem)cmbTableName.SelectedItem)?.Content?.ToString();
                    string connStr = selectedDb == "Development Environment"
                        ? developmentEnvironment
                        : selectedDb == "Testing Environment"
                            ? testingEnvironment
                            : null;

                    if (string.IsNullOrEmpty(connStr))
                    {
                        MessageBox.Show("Please select a valid environment.");
                        return;
                    }

                    await Task.Run(() =>
                    {
                        SaveToDatabase(dataTable, targetTableName, columnMap, connStr);
                    });

                    MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please select a table in the main window.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                busyIndicator.IsBusy = false;
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
                MessageBox.Show($"An error occurred while saving data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
