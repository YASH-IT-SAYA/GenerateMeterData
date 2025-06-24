using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using MeterTacker.Savedata;
using MeterTacker.Update;
using MeterTacker.CheckoutData;
using MeterTacker.Helper;
using log4net;

namespace MeterTacker.GetDataWUpdate
{
    public partial class GetDataWithUpdate : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool hasUnsavedChanges { get; set; } = false;
        public GetDataWithUpdate()
        {
            InitializeComponent();
            CommonList.Reset();
            if (CommonList.allData.Count > 0)
            {
                LoadData();
            }
        }
        private void LoadData()
        {
            try
            {
                if (CommonList.allData.Count > 0)
                {
                    DataTable dt = CommonList.allData.CopyToDataTable();
                    WaterFlowGrid.ItemsSource = dt.DefaultView;
                }
                else
                {
                    log.Info("No Data Found");
                    MessageBox.Show($"no data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                UpdateButtonsState(CommonList.allData.Count > 0);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to Data Found {ex.Message}");
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButtonsState(bool hasData)
        {
            SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush orangeBrush = new SolidColorBrush(Colors.Orange);
            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
            Update_data.IsEnabled = hasData;
            SavDB.IsEnabled = false;
            if (hasData)
            {
                StyleButton(Update_data, orangeBrush, whiteBrush);
                StyleButton(SavDB, Brushes.Gray, Brushes.LightGray);
                StyleButton(RefreshPage, greenBrush, whiteBrush);
            }
        }

        private void StyleButton(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            if (button.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock tb)
                        tb.Foreground = foreground;
                    if (child is ImageAwesome icon)
                        icon.Foreground = foreground;
                }
            }
        }

        private void RefreshPage_Click(object sender, RoutedEventArgs e)
        {
            log.Info("RefreshPage Button clicked");
            GetData getData = new GetData();
            bool? result = getData.ShowDialog();
            if (result == true)
            {
                LoadData();
            }
        }

        private void Update_data_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Update button is clicked");
            var updateWindow = new UpdateData();
            updateWindow.ShowDialog();

            if (updateWindow.IsDataUpdated)
            {
                Update_data.IsEnabled = false;
                StyleButton(Update_data, Brushes.Gray, Brushes.LightGray);

                SavDB.IsEnabled = true;
                StyleButton(SavDB, new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.White));

                hasUnsavedChanges = true;
            }
            else
            {
                SavDB.IsEnabled = false;
                StyleButton(SavDB, Brushes.Gray, Brushes.LightGray);
            }

            this.Show();
        }
        private void SavDB_Click(object sender, RoutedEventArgs e)
        {
            log.Info("SaveDB button is clicked");
            this.Hide();
            var saveWindow = new SaveDB();
            saveWindow.ShowDialog();
            this.Show();
        }
    }
}
