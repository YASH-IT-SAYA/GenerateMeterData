﻿using System;
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

namespace MeterTacker.GetDataWUpdate
{
    public partial class GetDataWithUpdate : Window
    {
        private string selectedFunction;
        private string meterNumber;
        private string gateway;
        private DateTime startDate;

        //public List<DataRow> allData = new List<DataRow>();
        //public List<DataRow> originalData = new List<DataRow>();
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
                    MessageBox.Show($"no data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                UpdateButtonsState(CommonList.allData.Count > 0);
            }
            catch (Exception ex)
            {
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
            GetData getData = new GetData();
            bool? result = getData.ShowDialog();


            if (result == true) 
            {
                LoadData(); 
            }
        }

        private bool ConfirmUnsavedChanges()
        {
            var result = MessageBox.Show(
                "You have unsaved updates. If you continue, all updated data will be lost.\n\nDo you want to proceed?",
                "Unsaved Changes Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void Update_data_Click(object sender, RoutedEventArgs e)
        {
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
            this.Hide();
            var saveWindow = new SaveDB();
            saveWindow.ShowDialog();
            this.Show();
        }

       
    }
}
