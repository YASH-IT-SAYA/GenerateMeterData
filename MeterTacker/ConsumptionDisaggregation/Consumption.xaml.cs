using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Npgsql;

namespace MeterTacker.ConsumptionDisaggregation
{
    public partial class Consumption : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            log.Info("Add Fixture button clicked");
            FixtureEntries.Add(new FixtureEntry());
        }
        private void RemoveFixtureRow_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Remove Fixture button clicked");
            if (FixtureEntries.Any())
            {
                FixtureEntries.RemoveAt(FixtureEntries.Count - 1);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Data Add button clicked");
            if (!int.TryParse(ci.Text, out int customerId))
            {
                log.Info("Entered the wrong customerID");
                MessageBox.Show("Invalid Customer ID.");
                return;
            }

            if (std.SelectedDate == null)
            {
                log.Info("Didn't selected Table");
                MessageBox.Show("Please select a valid date.");
                return;
            }

            string env = (cmbTableName.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(env) || env == "Select Environment")
            {
                log.Info("Didn't selected Envirnoment");
                MessageBox.Show("Please select a valid environment.");
                return;
            }

            if (FixtureEntries.Count == 0 || FixtureEntries.Any(f => string.IsNullOrWhiteSpace(f.FixtureName) || f.UnitConsumption <= 0))
            {
                log.Info("Entered the valid Fixture Name and Unit Consumption for all rows");
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
                            using (var cmd = new NpgsqlCommand(
                              @"SELECT public.consumption_disaggregation_add(@name, @consumption, @status, @date, @custid)", conn))
                            {
                                cmd.CommandType = CommandType.Text;
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
                log.Info("Data Inserted Successfully");
                MessageBox.Show("Data inserted successfully.");
                this.Close();
            }
            catch (Exception ex)
            {

                log.Error($"Error inserting Data {ex.Message}");
                MessageBox.Show("Error inserting data: " + ex.Message);
            }
            finally
            {
                busyIndicator.IsBusy = false;
            }
        }
    }
}
