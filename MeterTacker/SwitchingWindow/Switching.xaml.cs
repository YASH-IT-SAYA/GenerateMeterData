using MeterTacker.ConsumptionDisaggregation;
using MeterTacker.WaterClassifySummary;
using MeterTacker.CheckoutData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MeterTacker.GetDataWUpdate;

namespace MeterTacker.SwitchingWindow
{
    public partial class Switching : Window
    {
        public Switching()
        {
            InitializeComponent();
        }

        private void AddWaterClassiFy_Click(object sender, RoutedEventArgs e)
        {
            WaterClassify waterClassify = new WaterClassify();
            waterClassify.ShowDialog();
        }

        private void Consumption_Click(object sender, RoutedEventArgs e)
        {
            GetDataWithUpdate getDataWithUpdate = new GetDataWithUpdate();
            getDataWithUpdate.ShowDialog();
        }

        private void UsageByFixture_Click(object sender, RoutedEventArgs e)
        {
            Consumption consumption = new Consumption();
            consumption.ShowDialog();
        }
    }
}
