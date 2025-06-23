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
using System.Drawing;
using System.Windows.Media;
using MeterTacker.WaterClassifySummary;
using MeterTacker.ConsumptionDisaggregation;

namespace MeterTacker.CheckoutData
{
    public partial class GetData : Window
    {
        private static readonly string connectionString = "Server=saya-live.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=SAYA;User Id=teamqa;Password=MLIS@3120;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0;";
        public GetData()
        {
            InitializeComponent();
        }
        public bool hasUnsavedChanges { get; set; } = false;
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved updates. If you continue, all updated data will be lost.\n\nDo you want to proceed?",
                    "Unsaved Changes Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
                hasUnsavedChanges = false;
            }
            allData.Clear();
            LoadData();
        }
        private void RefreshPage_Click(object sender, RoutedEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved updates. If you continue, all updated data will be lost.\n\nDo you want to proceed?",
                    "Unsaved Changes Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
                hasUnsavedChanges = false;
            }
            allData.Clear();
            LoadData();
        }
        public List<DataRow> allData = new List<DataRow>();
        public List<DataRow> originalData = new List<DataRow>();
        private async void LoadData()
        {
            busyIndicator.IsBusy = true;
            try
            {
                if (cmbTableName.SelectedIndex <= 0)
                {
                    MessageBox.Show("Please select a table before loading data.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (!dpStartDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select a Start Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!dpEndDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select an End Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
                {
                    MessageBox.Show("Start Date must be before End Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DateTime startDate = dpStartDate.SelectedDate.Value;
                DateTime endDate = dpEndDate.SelectedDate.Value;
                string selectedTable = cmbTableName.SelectedValue as string;
                if(selectedTable == null)
                {
                    MessageBox.Show("Please select a valid table.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (startDate > endDate)
                {
                    MessageBox.Show("Start Date must be before End Date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (selectedTable == "get_water_status_filtered" || selectedTable == "water_meter_flow_report_latest")
                {
                    TimeSpan range = endDate - startDate;
                    if (range.TotalDays > 30)
                    {
                        MessageBox.Show("Date range cannot exceed 30 days.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                var selectedOption = cmbTableName.SelectedItem as TableOption;
                if (selectedOption == null || string.IsNullOrWhiteSpace(selectedOption.ActualName))
                {
                    MessageBox.Show("Please select a valid table.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string selectedFunction = selectedOption.ActualName;
                string query = $"SELECT * FROM {selectedFunction}(@meterNumber, @gateway, @startDate, @endDate)";
                var meterNumber = txtMeterNumber.Text.Trim();
                var gateway = txtGateway.Text.Trim();
                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@meterNumber", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = meterNumber },
                    new NpgsqlParameter("@gateway", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = gateway },
                    new NpgsqlParameter("@startDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = startDate },
                    new NpgsqlParameter("@endDate", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value = endDate }
                };
                allData.Clear();
                originalData.Clear();
                await Task.Run(() =>
                {
                    using (DataTable dt = SqlHelper.ExecuteDataTable(connectionString, CommandType.Text, query, parameters))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var copiedRow = row.Table.NewRow();
                            copiedRow.ItemArray = (object[])row.ItemArray.Clone();

                            allData.Add(row);
                            originalData.Add(copiedRow);
                        }

                    }
                });
                WaterFlowGrid.ItemsSource = allData.Count > 0 ? allData.CopyToDataTable().DefaultView : null;
                bool hasData = allData.Count > 0;
                Update_data.IsEnabled = hasData;
                SavDB.IsEnabled = hasData;
                RefreshPage.IsEnabled = hasData;
                WaterClassifier.IsEnabled = hasData;
                if (hasData)
                {
                    SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
                    SolidColorBrush greenbrush = new SolidColorBrush(Colors.Green);
                    SolidColorBrush iconbrush = new SolidColorBrush(Colors.White);
                    Update_data.Background = orangebrush;
                    var updateStackPanel = Update_data.Content as StackPanel;
                    if (updateStackPanel != null)
                    {
                        foreach (var child in updateStackPanel.Children)
                        {
                            if (child is TextBlock tb)
                                tb.Foreground = iconbrush;
                            if (child is FontAwesome.WPF.ImageAwesome icon)
                                icon.Foreground = iconbrush;
                        }
                    }
                    SavDB.Background = greenbrush;
                    var saveStackPanel = SavDB.Content as StackPanel;
                    if (saveStackPanel != null)
                    {
                        foreach (var child in saveStackPanel.Children)
                        {
                            if (child is TextBlock tb)
                                tb.Foreground = iconbrush;
                            if (child is FontAwesome.WPF.ImageAwesome icon)
                                icon.Foreground = iconbrush;
                        }
                    }
                    RefreshPage.Background = greenbrush;
                    var RefreshStackPanel = RefreshPage.Content as StackPanel;
                    if (RefreshStackPanel != null)
                    {
                        foreach (var child in RefreshStackPanel.Children)
                        {
                            if (child is TextBlock tb)
                                tb.Foreground = iconbrush;
                            if (child is FontAwesome.WPF.ImageAwesome icon)
                                icon.Foreground = iconbrush;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
        private void cmbTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || cmbTableName.SelectedIndex <= 0) return;
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
            txtMeterNumber.Text = "";
            txtGateway.Text = "";
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            allData.Clear();
            WaterFlowGrid.ItemsSource = null;
            Update_data.IsEnabled = false;
            SavDB.IsEnabled = false;
            RefreshPage.IsEnabled = false;
            SolidColorBrush graybrush = new SolidColorBrush(Colors.Gray);
            SolidColorBrush grayTextBrush = new SolidColorBrush(Colors.Gray);
            void SetButtonToDisabledState(Button btn)
            {
                btn.Background = graybrush;
                var panel = btn.Content as StackPanel;
                if (panel != null)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is TextBlock tb)
                            tb.Foreground = grayTextBrush;
                        if (child is FontAwesome.WPF.ImageAwesome icon)
                            icon.Foreground = grayTextBrush;
                    }
                }
            }
            SetButtonToDisabledState(Update_data);
            SetButtonToDisabledState(SavDB);
            SetButtonToDisabledState(RefreshPage);
        }
        private void Update_data_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRow = WaterFlowGrid.SelectedItem as DataRowView;
            if (dataRow == null)
            {
                MessageBox.Show("Please select a row to update.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbTableName.SelectedItem == null)
            {
                MessageBox.Show("Please select a table before updating.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedOption = cmbTableName.SelectedItem as TableOption;
            if (selectedOption == null)
            {
                MessageBox.Show("Please select a valid table.");
                return;
            }
            string selectedTable = selectedOption.ActualName;
            string meterCol;
            if (selectedTable == "daily_meter_vise_cons_raw")
                meterCol = "MeterNumber";
            else
                meterCol = "meterNumber";
            string gatewayCol;
            if (selectedTable == "daily_meter_vise_cons_raw")
                gatewayCol = "GatewayMac";
            else
                gatewayCol = "gw";
            string parameterNumber = dataRow.Row.Table.Columns.Contains(meterCol) ? dataRow.Row[meterCol].ToString() : string.Empty;
            string gatewayNumber = dataRow.Row.Table.Columns.Contains(gatewayCol) ? dataRow.Row[gatewayCol].ToString() : string.Empty;
            DateTime? FilterDate = dpStartDate.SelectedDate;
            this.Hide();
            UpdateData updateData = new UpdateData(parameterNumber, gatewayNumber, FilterDate, selectedTable);
            updateData.ShowDialog();
            this.Show();
        }
        private void SavDB_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SaveDB dB = new SaveDB();
            dB.ShowDialog();
            this.Show();
        }
        private void WaterClassifier_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            WaterClassify waterClassify = new WaterClassify();
            waterClassify.ShowDialog();
            this.Show();
        }
        private void Consumption_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Consumption consumption = new Consumption();
            consumption.ShowDialog();
            this.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbTableName.ItemsSource = new List<TableOption>
            {
                new TableOption { DisplayName = "-- Select Table --", ActualName = "" },
                new TableOption { DisplayName = "WaterMeterStatusReportLatest", ActualName = "get_water_status_filtered" },
                new TableOption { DisplayName = "DailyMeterViseCons", ActualName = "daily_meter_vise_cons_raw" },
                new TableOption { DisplayName = "WaterMeterFlowReportLatest", ActualName = "water_meter_flow_report_latest" },
            };
            cmbTableName.SelectedIndex = 0;
        }
    }
}