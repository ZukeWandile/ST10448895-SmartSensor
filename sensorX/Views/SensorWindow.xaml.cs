using sensorX.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace sensorX.Views
{

    // Interaction logic for SensorWindow.xaml

    public partial class SensorWindow : Window
    {
        // Used for generating fake sensor data
        private readonly Random _random = new Random();

        // Exposes the collection of sensors from the SensorStore for data binding in the UI.
        public ObservableCollection<Sensor> Sensors => SensorStore.Sensors;
        // Holds the currently selected/last registered sensor
        public static Sensor? ActiveSensor;
        public SensorWindow()
        {
            InitializeComponent();
            dgSensors.ItemsSource = Sensors;
        }


        // Handles the Back button click event to close or navigate away.
    
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new SensorDashBoard();
            sensorDashBoard.Show();
            this.Close();
        }


        // Handles generating random values for the sensor form fields.

        private void BtnGenerateRandom_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomSensor();
        }

        private void GenerateRandomSensor()
        {
            byte[] mac = new byte[6];// Generate a random MAC address 
            _random.NextBytes(mac);// Fill the byte array with random values

            txtMacAddress.Text = string.Join(":", mac.Select(b => b.ToString("X2")));// Convert each byte to a two-digit hexadecimal string and join with colons

            string[] locations =
            {
                "Reception",
                "Boardroom",
                "Server Room",
                "Office A",
                "Warehouse",
                "Lab B"
            };

            txtLocation.Text = locations[_random.Next(locations.Length)];// Randomly select a location from the predefined list

            cmbCategory.SelectedIndex = _random.Next(cmbCategory.Items.Count);// Randomly select a category from the ComboBox

            txtNodeId.Text = $"NODE-{_random.Next(100, 999)}";// Generate a random Node ID in the format "NODE-XXX" where XXX is a random number between 100 and 999
        }

        // Handles registering the sensor data.

        private void BtnRegisterSensor_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            string category = "";

            if (cmbCategory.SelectedItem is ComboBoxItem item)
                category = item.Content.ToString()!;

            // Create the sensor
            Sensor sensor = new Sensor
            {
                MacAddress = txtMacAddress.Text.Trim(),
                Location = txtLocation.Text.Trim(),
                Category = category,
                NodeId = txtNodeId.Text.Trim(),
                DateRegistered = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            // Save it
            Sensors.Add(sensor);

            // Make it the active sensor for telemetry
            ActiveSensor = sensor;

            MessageBox.Show(
                $"Sensor {sensor.NodeId} registered successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ClearForm();
        }
        private bool ValidateInputs()
        {
            string mac = txtMacAddress.Text.Trim();

            if (!Regex.IsMatch(mac, @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
            {
                MessageBox.Show(
                    "Enter a valid MAC Address.\nExample: AA:BB:CC:DD:EE:FF",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtMacAddress.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show(
                    "Please enter a location.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtLocation.Focus();
                return false;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select a category.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                cmbCategory.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNodeId.Text))
            {
                MessageBox.Show(
                    "Please enter a Node ID.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtNodeId.Focus();
                return false;
            }

            return true;
        }

        // Clears the registration form.
        private void ClearForm()
        {
            txtMacAddress.Clear();
            txtLocation.Clear();
            txtNodeId.Clear();

            cmbCategory.SelectedIndex = 0;
        }
    }
}