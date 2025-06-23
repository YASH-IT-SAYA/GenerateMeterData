using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MeterTacker.GetDataWUpdate;
using MeterTacker.Helper;

namespace MeterTacker.Update
{
    public partial class UpdateData : Window
    {
        public bool IsDataUpdated { get; private set; } = false;

        public UpdateData()
        {
            InitializeComponent();
            dpStartDate.DisplayDateEnd = DateTime.Now;
            UpdateBlackoutDates();
        }
        private void UpdateBlackoutDates()
        {
            if (CommonList.startDate != null && CommonList.endDate != null)
            {
                int dayDifference = (CommonList.endDate - CommonList.startDate).Days;
                DateTime blackoutStart = DateTime.Today.AddDays(-dayDifference);
                dpStartDate.BlackoutDates.Clear(); 
                dpStartDate.BlackoutDates.Add(new CalendarDateRange(blackoutStart, DateTime.MaxValue));
            }
        }
        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!dpStartDate.SelectedDate.HasValue)
                return;
            DateTime newDate = dpStartDate.SelectedDate.Value;
            DateTime baseDate;
            if (CommonList.startDate != null)
            {
                baseDate = CommonList.startDate;
            }
            else
            {
                txtBaseDateSummary.Text = "Base Date: (Not Selected)";
                txtNewDateSummary.Text = "New Date: " + newDate.ToShortDateString();
                txtDayDiffSummary.Text = "";
                return;
            }
            int days = (newDate - baseDate).Days;
            txtBaseDateSummary.Text = "Base Date: " + baseDate.ToShortDateString();
            txtNewDateSummary.Text = "New Date: " + newDate.ToShortDateString();
            txtDayDiffSummary.Text = $"Shift Days: {days} days";
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;
            try
            {
                if (string.IsNullOrWhiteSpace(txtParameterNumber.Text))
                {
                    MessageBox.Show("Please enter a valid Meter Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtGatewayNumber.Text))
                {
                    MessageBox.Show("Please enter a valid Gateway Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!dpStartDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select the new date to shift to.");
                    return;
                }
                DateTime selectedDate = dpStartDate.SelectedDate.Value;
                if (CommonList.startDate == null)
                {
                    if (!dpBaseDate.SelectedDate.HasValue)
                    {
                        MessageBox.Show("Please select a base date to compare from...");
                        return;
                    }
                    CommonList.startDate = dpBaseDate.SelectedDate.Value;
                }
                int daydiff = (selectedDate - CommonList.startDate).Days;
                string newMeterNumber = txtParameterNumber.Text;
                string newGatewayNumber = txtGatewayNumber.Text;
                var mainWindow = Application.Current.Windows.OfType<MeterTacker.GetDataWUpdate.GetDataWithUpdate>().FirstOrDefault();
                if (mainWindow == null)
                {
                    MessageBox.Show("Main window not found.");
                    return;
                }
                var currentData = CommonList.originalData;
                CommonList.allData.Clear();

                string meterCol = CommonList.selectedTableName == "daily_meter_vise_cons_raw" ? "MeterNumber" : "meterNumber";
                string gatewayCol = CommonList.selectedTableName == "daily_meter_vise_cons_raw" ? "GatewayMac" : "gw";

                string oldMeterNumber = currentData.FirstOrDefault()?[$"{meterCol}"]?.ToString() ?? CommonList.oldMeterNumber;
                string oldGatewayNumber = currentData.FirstOrDefault()?[$"{gatewayCol}"]?.ToString() ?? CommonList.oldGateway;

                foreach (DataRow originalRow in currentData)
                {
                    bool match = true;

                    if (!string.IsNullOrWhiteSpace(oldMeterNumber) &&
                        (!originalRow.Table.Columns.Contains(meterCol) || originalRow[meterCol].ToString() != oldMeterNumber))
                        match = false;

                    if (!string.IsNullOrWhiteSpace(oldGatewayNumber) &&
                        (!originalRow.Table.Columns.Contains(gatewayCol) || originalRow[gatewayCol].ToString() != oldGatewayNumber))
                        match = false;

                    var row = originalRow.Table.NewRow();
                    row.ItemArray = (object[])originalRow.ItemArray.Clone();
                    if (match)
                    {
                        switch (CommonList.selectedTableName)
                        {
                            case "get_water_status_filtered":
                                if (row.Table.Columns.Contains("meterLocalTime") && row["meterLocalTime"] != DBNull.Value)
                                    row["meterLocalTime"] = ((DateTime)row["meterLocalTime"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("createdDate") && row["createdDate"] != DBNull.Value)
                                    row["createdDate"] = ((DateTime)row["createdDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("meterLocalDate") && row["meterLocalDate"] != DBNull.Value)
                                    row["meterLocalDate"] = ((DateTime)row["meterLocalDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("meterNumber"))
                                    row["meterNumber"] = newMeterNumber;
                                if (row.Table.Columns.Contains("gw"))
                                    row["gw"] = newGatewayNumber;
                                break;

                            case "daily_meter_vise_cons_raw":
                                if (row.Table.Columns.Contains("TodayDate") && row["TodayDate"] != DBNull.Value)
                                    row["TodayDate"] = ((DateTime)row["TodayDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value)
                                    row["CreatedDate"] = ((DateTime)row["CreatedDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("MeterNumber"))
                                    row["MeterNumber"] = newMeterNumber;
                                if (row.Table.Columns.Contains("GatewayMac"))
                                    row["GatewayMac"] = newGatewayNumber;
                                break;

                            case "water_meter_flow_report_latest":
                                if (row.Table.Columns.Contains("meterLocalTime") && row["meterLocalTime"] != DBNull.Value)
                                    row["meterLocalTime"] = ((DateTime)row["meterLocalTime"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("createdDate") && row["createdDate"] != DBNull.Value)
                                    row["createdDate"] = ((DateTime)row["createdDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("meterLocalDate") && row["meterLocalDate"] != DBNull.Value)
                                    row["meterLocalDate"] = ((DateTime)row["meterLocalDate"]).AddDays(daydiff);
                                if (row.Table.Columns.Contains("meterNumber"))
                                    row["meterNumber"] = newMeterNumber;
                                if (row.Table.Columns.Contains("gw"))
                                    row["gw"] = newGatewayNumber;
                                break;

                            default:
                                MessageBox.Show("Update logic not defined for the selected table.");
                                break;
                        }
                    }
                    CommonList.allData.Add(row);
                }
                mainWindow.WaterFlowGrid.ItemsSource = null;
                mainWindow.WaterFlowGrid.ItemsSource = CommonList.allData.CopyToDataTable().DefaultView;
                mainWindow.hasUnsavedChanges = true;
                MessageBox.Show("All matching rows updated");
                IsDataUpdated = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"error {ex}");
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
    }
}
