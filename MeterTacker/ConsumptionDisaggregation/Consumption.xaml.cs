using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Npgsql;

namespace MeterTacker.ConsumptionDisaggregation
{
    public partial class Consumption : Window
    {
        private string developmentEnvironment = ConfigurationManager.ConnectionStrings["developmentEnvironment"].ConnectionString;
        private string testingEnvironment = ConfigurationManager.ConnectionStrings["testingEnvironment"].ConnectionString;
        public ObservableCollection<FixtureEntry> FixtureEntries { get; set; } = new ObservableCollection<FixtureEntry>();

        public Consumption()
        {
            InitializeComponent();
    
            fixtureList.ItemsSource = FixtureEntries;
            unitConsumptionList.ItemsSource = FixtureEntries;

            FixtureEntries.Add(new FixtureEntry());
        }

        private void AddFixtureRow_Click(object sender, RoutedEventArgs e)
        {
            FixtureEntries.Add(new FixtureEntry());
        }   
        private void RemoveFixtureRow_Click(object sender, RoutedEventArgs e)
        {
            if (FixtureEntries.Any())
            {
                FixtureEntries.RemoveAt(FixtureEntries.Count - 1);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
         
            if (!int.TryParse(ci.Text, out int customerId))
            {
                MessageBox.Show("Invalid Customer ID.");
                return;
            }

            if (std.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid date.");
                return;
            }

            string env = (cmbTableName.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(env) || env == "Select Environment")
            {
                MessageBox.Show("Please select a valid environment.");
                return;
            }

            if (FixtureEntries.Count == 0 || FixtureEntries.Any(f => string.IsNullOrWhiteSpace(f.FixtureName) || f.UnitConsumption <= 0))
            {
                MessageBox.Show("Please enter valid Fixture Name and Unit Consumption for all rows.");
                return;
            }

            string connectionString = env == "Development Environment" ? developmentEnvironment : testingEnvironment;
            DateTime selectedDate = std.SelectedDate.Value;
            bool meterStatus = true;

            busyIndicator.IsBusy = true;

            try
            {
                await Task.Run(() =>
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        foreach (var fixture in FixtureEntries)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO public.consumption_disaggregation
                                (utility_name, utility_consumption, meter_status, today_date, ""customerId"")
                                VALUES (@name, @consumption, @status, @date, @custid)", conn))
                            {
                                cmd.Parameters.AddWithValue("name", fixture.FixtureName);
                                cmd.Parameters.AddWithValue("consumption", fixture.UnitConsumption);
                                cmd.Parameters.AddWithValue("status", meterStatus);
                                cmd.Parameters.AddWithValue("date", selectedDate);
                                cmd.Parameters.AddWithValue("custid", customerId);
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
                MessageBox.Show("Error inserting data: " + ex.Message);
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
    }
}
