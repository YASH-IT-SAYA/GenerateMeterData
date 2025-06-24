using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Npgsql;

namespace MeterTacker.WaterClassifySummary
{
    public partial class WaterClassify : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string developmentEnvironment = ConfigurationManager.ConnectionStrings["developmentEnvironment"].ConnectionString;
        private string testingEnvironment = ConfigurationManager.ConnectionStrings["testingEnvironment"].ConnectionString;
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
            if (!decimal.TryParse(hghfrc.Text, out decimal high) &&
                !decimal.TryParse(lfrc.Text, out decimal low) &&
                !decimal.TryParse(mfrc.Text, out decimal medium) &&
                !decimal.TryParse(outliers.Text, out decimal outlier))
            {
                log.Info("Please enter at least one value among High, Low, Medium, or Outliers.");
                MessageBox.Show("Please enter at least one value among High, Low, Medium, or Outliers.");
                return;
            }

            string MeterNum = MeterNumber.Text;
            if(string.IsNullOrWhiteSpace(MeterNum))
            {
                log.Info("Meter Number is required");
                MessageBox.Show("Meter Number is required");
                return;
            }

            string gateway = GatewayNumber.Text;
            if (string.IsNullOrWhiteSpace(gateway))
            {
                log.Info("Gateway number is required");
                MessageBox.Show("Gateway Number is required.");
                return;
            }

            if (!int.TryParse(CustomerId.Text, out int customerId))
            {
                log.Info("Invalid Customer ID");
                MessageBox.Show("Invalid Customer ID");
                return;
            }

            string year = YearComboBox.SelectedItem?.ToString();
            string month = MonthComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month))
            {
                log.Info("Please select both year and month");
                MessageBox.Show("Please select both year and month.");
                return;
            }

            string formattedMonth = $"['{year}-{month}']";

            string env = (cmbTableName.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(env) || env == "Select Environment")
            {
                log.Info("Please select a valid environment.");
                MessageBox.Show("Please select a valid environment.");
                return;
            }

            string connectionString = env == "Development Environment" ? developmentEnvironment : testingEnvironment;

            var entries = new List<(string Category, decimal Value)>();
            if (decimal.TryParse(hghfrc.Text, out high)) entries.Add(("High Flow Rate Consumption", high));
            if (decimal.TryParse(lfrc.Text, out low)) entries.Add(("Low Flow Rate Consumption", low));
            if (decimal.TryParse(mfrc.Text, out medium)) entries.Add(("Medium Flow Rate Consumption", medium));
            if (decimal.TryParse(outliers.Text, out outlier)) entries.Add(("Outliers", outlier));
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
                log.Info("Data inserted successfully.");
                MessageBox.Show("Data inserted successfully.");
                this.Close();
            }
            catch (Exception ex)
            {
                log.Error($"Error while data inserting ");
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
    }
}
