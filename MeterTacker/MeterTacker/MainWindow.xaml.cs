using System.Windows;
using MeterTacker.CheckoutData;
using System;


namespace MeterTacker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            if (username == "U" && password == "P")
            {
                try
                {
                    GetData getData = new GetData();
                    getData.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to Login{ex.Message}", "Failed");
                }
            }
            else
            {
                MessageBox.Show("Invalid Password or Username");
            }
        }
        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Placeholder1.Visibility = Visibility.Hidden;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Placeholder2.Visibility = Visibility.Hidden;
        }
        private void Placeholder2_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }
        private void Placeholder1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UsernameTextBox.Focus();
        }
    }
}
