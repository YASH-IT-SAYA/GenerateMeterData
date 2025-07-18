﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Npgsql;

namespace MeterTacker.WaterClassifySummary
{
    public partial class WaterClassify : Window
    {
        private static readonly string developmentEnvironment = "Server=saya-dev2.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=sayadev;User Id=SayaDev;Password=duca$$0234;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0";
        private static readonly string testingEnvironment = "Server=saya-dev2.cq6nozddb1mr.us-west-2.rds.amazonaws.com;Port=5432;Database=sayatesting;User Id=SayaDev;Password=duca$$0234;Timeout=1024;Pooling=true;MaxPoolSize=50;CommandTimeout=0";

        public WaterClassify()
        {
            InitializeComponent();
            PopulateYearAndMonth();
        }
  
        private void PopulateYearAndMonth()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear - 25; year <= currentYear + 25; year++)
            {
                YearComboBox.Items.Add(year);
            }

            for (int month = 1; month <= 12; month++)
            {
                MonthComboBox.Items.Add(month.ToString("D2"));
            }
    
            YearComboBox.SelectedItem = currentYear;
            MonthComboBox.SelectedItem = DateTime.Now.Month.ToString("D2");
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!float.TryParse(hghfrc.Text, out float high) &&
                !float.TryParse(lfrc.Text, out float low) &&
                !float.TryParse(mfrc.Text, out float medium) &&
                !float.TryParse(outliers.Text, out float outlier))
            {
                MessageBox.Show("Please enter at least one value among High, Low, Medium, or Outliers.");
                return;
            }

            string MeterNum = MeterNumber.Text;
            if(string.IsNullOrWhiteSpace(MeterNum))
            {
                MessageBox.Show("Meter Number is required");
                return;
            }

            string gateway = GatewayNumber.Text;
            if (string.IsNullOrWhiteSpace(gateway))
            {
                MessageBox.Show("Gateway Number is required.");
                return;
            }

            if (!int.TryParse(CustomerId.Text, out int customerId))
            {
                MessageBox.Show("Invalid Customer ID");
                return;
            }

            string year = YearComboBox.SelectedItem?.ToString();
            string month = MonthComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month))
            {
                MessageBox.Show("Please select both year and month.");
                return;
            }

            string formattedMonth = $"['{year}-{month}']";

            string env = (cmbTableName.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(env) || env == "Select Environment")
            {
                MessageBox.Show("Please select a valid environment.");
                return;
            }

            string connectionString = env == "Development Environment" ? developmentEnvironment : testingEnvironment;

            var entries = new List<(string Category, float Value)>();
            if (float.TryParse(hghfrc.Text, out high)) entries.Add(("High Flow Rate Consumption", high));
            if (float.TryParse(lfrc.Text, out low)) entries.Add(("Low Flow Rate Consumption", low));
            if (float.TryParse(mfrc.Text, out medium)) entries.Add(("Medium Flow Rate Consumption", medium));
            if (float.TryParse(outliers.Text, out outlier)) entries.Add(("Outliers", outlier));
      
            busyIndicator.IsBusy = true;

            try
            {
                await Task.Run(() =>
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        foreach (var (category, value) in entries)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO public.""WaterClassifySummary""(
                                    ""WaterClassify"", ""MeterNumber"", ""Gatewaymac"",
                                    ""Total_Volume"", ""CustomerId"", ""Month"", ""CreatedDate"")
                                VALUES (@wc, @mn, @gw, @tv, @cid, @mon, @cdt);", conn))
                            {
                                cmd.Parameters.AddWithValue("wc", category);
                                cmd.Parameters.AddWithValue("mn", MeterNum);
                                cmd.Parameters.AddWithValue("gw", gateway);
                                cmd.Parameters.AddWithValue("tv", value);
                                cmd.Parameters.AddWithValue("cid", customerId);
                                cmd.Parameters.AddWithValue("mon", formattedMonth);
                                cmd.Parameters.AddWithValue("cdt", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                });

                MessageBox.Show("Data inserted successfully.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
    }
}
