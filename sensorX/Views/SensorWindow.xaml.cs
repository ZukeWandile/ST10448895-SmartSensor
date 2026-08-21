using System;
using System.Collections.Generic;
using System.Windows;

namespace sensorX.Views
{

    // Interaction logic for SensorWindow.xaml

    public partial class SensorWindow : Window
    {
        public SensorWindow()
        {
            InitializeComponent();
        }


        // Handles the Back button click event to close or navigate away.
    
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

 
        // Handles generating random values for the sensor form fields.
 
        private void BtnGenerateRandom_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();

            // Generate random MAC Address
            byte[] macBytes = new byte[6];
            random.NextBytes(macBytes);
            txtMacAddress.Text = string.Join(":", Array.ConvertAll(macBytes, b => b.ToString("X2")));

            // Generate random Node ID
            txtNodeId.Text = random.Next(100, 999).ToString();

            // Set a default sample location
            txtLocation.Text = "Zone-" + random.Next(1, 10);
        }

    
        // Handles registering the sensor data.
        
        private void BtnRegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMacAddress.Text) || string.IsNullOrWhiteSpace(txtNodeId.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Sensor registered successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}