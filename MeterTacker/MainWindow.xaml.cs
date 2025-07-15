﻿using System.Windows;
using MeterTacker.CheckoutData;
using System;
using MeterTacker.SwitchingWindow;
using log4net;

namespace MeterTacker
{
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
        {
            try
            {
                InitializeComponent();
            } 
            catch (Exception)
            {
                throw;
            }
        }    
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Login button clicked");
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            if (username == "Maxlink3120" && password == "MLIS@3120")
            {
                try
                {
                    Switching Switching = new Switching();
                    Switching.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to Login {ex.Message}");
                    MessageBox.Show($"Failed to Login{ex.Message}", "Failed");
                }
            }
            else
            {
                log.Error("Invalid Password or Username");
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
