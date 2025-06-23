using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ArkaPRP83.Core.Helper;
using Npgsql;
using MeterTacker.Update;
using System.Threading.Tasks;
using MeterTacker.Savedata;
using MeterTacker.GetDataWUpdate;
using MeterTacker.Helper;

namespace MeterTacker.CheckoutData
{
    public partial class GetData : Window
    {
        private static readonly string connectionString = "Server=saya-live.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=SAYA;User Id=teamqa;Password=MLIS@3120;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0;";

        public bool hasUnsavedChanges { get; set; } = false;
        public GetData()
        {
            InitializeComponent();
        }

        private async void Search_click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedOption = cmbTableName.SelectedItem as TableOption;
                if (selectedOption == null || string.IsNullOrWhiteSpace(selectedOption.ActualName))
                {
                    MessageBox.Show("Please select a valid table.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtMeterNumber.Text))
                {
                    MessageBox.Show("Please enter a valid Meter Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtGateway.Text))
                {
                    MessageBox.Show("Please enter a valid Gateway Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select both Start and End Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime startDate = dpStartDate.SelectedDate.Value;
                DateTime endDate = dpEndDate.SelectedDate.Value;

                if (startDate > endDate)
                {
                    MessageBox.Show("Start Date must be before End Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedFunctionName = selectedOption.ActualName;
                CommonList.selectedTableName = selectedFunctionName;

                if ((selectedFunctionName == "get_water_status_filtered" || selectedFunctionName == "water_meter_flow_report_latest") &&
                    (endDate - startDate).TotalDays > 30)
                {
                    MessageBox.Show("Date range cannot exceed 30 days.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string meterNumber = txtMeterNumber.Text.Trim();
                string gateway = txtGateway.Text.Trim();

                string query = $"SELECT * FROM {selectedFunctionName}(@meterNumber, @gateway, @startDate, @endDate)";
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@meterNumber", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = meterNumber },
                    new NpgsqlParameter("@gateway", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = gateway },
                    new NpgsqlParameter("@startDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = startDate },
                    new NpgsqlParameter("@endDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = endDate }
                };

                CommonList.allData.Clear();
                CommonList.originalData.Clear();
                busyIndicator.IsBusy = true;
                await Task.Run(() =>
                {
                    using (DataTable dt = SqlHelper.ExecuteDataTable(connectionString, CommandType.Text, query, parameters))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var copiedRow = dt.NewRow();
                            copiedRow.ItemArray = (object[])row.ItemArray.Clone();

                            CommonList.allData.Add(row);
                            CommonList.originalData.Add(copiedRow);
                        }
                    }
                });
                busyIndicator.IsBusy = false;

                CommonList.oldMeterNumber = meterNumber;
                CommonList.oldGateway = gateway;
                CommonList.startDate = startDate;
                CommonList.endDate = endDate; 
                CommonList.selectedTableName = selectedFunctionName;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || cmbTableName.SelectedIndex <= 0)
                return;

            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved updates. If you continue, all updated data will be lost.\n\nDo you want to proceed?",
                    "Unsaved Changes Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    cmbTableName.SelectionChanged -= cmbTableName_SelectionChanged;
                    cmbTableName.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                    cmbTableName.SelectionChanged += cmbTableName_SelectionChanged;
                    return;
                }

                hasUnsavedChanges = false;
            }
            var selected = cmbTableName.SelectedItem as TableOption;
            if (selected != null)
            {
                CommonList.selectedTableName = selected.ActualName;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbTableName.ItemsSource = CommonList.TableOptions;
            cmbTableName.SelectedIndex = 0;
        }
    }
}
