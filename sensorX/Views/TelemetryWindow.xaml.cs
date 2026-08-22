using sensorX.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace sensorX.Views
{
    public partial class TelemetryWindow : Window
    {
        private readonly Random _random = new Random();

        private readonly DispatcherTimer _telemetryTimer;

        // Generic telemetry collections
        private readonly List<TelemetryPacket<float>> _temperatureHistory = new();

        private readonly List<TelemetryPacket<int>> _powerHistory = new();

        private readonly List<TelemetryPacket<bool>> _valveHistory = new();

        // Collection used by the DataGrid
        private readonly List<TelemetryHistoryItem> _displayHistory = new();


        public TelemetryWindow()
        {
            InitializeComponent();
            // Initialize the telemetry timer
            _telemetryTimer = new DispatcherTimer
            {
                // Set the timer interval to 1 second
                Interval = TimeSpan.FromSeconds(1)
            };
            // Attach the Tick event handler
            _telemetryTimer.Tick += TelemetryTimer_Tick;
            // Bind the DataGrid to the display history collection
            TelemetryGrid.ItemsSource = _displayHistory;
        }

        // Event handler for the Start button click event 
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _telemetryTimer.Start();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            StatusText.Text = "ACTIVE";
            StatusText.Foreground = Brushes.Green;

            StatusIndicator.Fill = Brushes.Green;
        }

        // Event handler for the Stop button click event
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _telemetryTimer.Stop();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusText.Foreground = Brushes.Gray;

            StatusIndicator.Fill = Brushes.Gray;
        }

        // Event handler for the telemetry timer tick event
        private void TelemetryTimer_Tick(object? sender, EventArgs e)
        {
            GenerateTelemetry();
        }


        private void GenerateTelemetry()
        {
            // Generate simulated values

            float temperature =
                (float)(_random.NextDouble() * 10 + 20);

            int power = _random.Next(400, 2000);

            bool valve =  _random.Next(2) == 1;


            // Create generic telemetry packets

            TelemetryPacket<float> temperaturePacket = new TelemetryPacket<float>(temperature);

            TelemetryPacket<int> powerPacket = new TelemetryPacket<int>(power);

            TelemetryPacket<bool> valvePacket = new TelemetryPacket<bool>(valve);
                


            // Store packets

            _temperatureHistory.Add(temperaturePacket);

            _powerHistory.Add(powerPacket);

            _valveHistory.Add(valvePacket);


            // Update current values

            TemperatureText.Text = $"{temperature:F1} °C";

            PowerText.Text = $"{power:N0} W";

            ValveText.Text = valve ? "OPEN" : "CLOSED";


            // Add to history table

            TelemetryHistoryItem historyItem = new TelemetryHistoryItem
                {
                    Time = temperaturePacket.Timestamp.ToString("HH:mm:ss"),

                    Temperature = $"{temperature:F1} °C",

                    Power = $"{power:N0} W",

                    Valve = valve ? "OPEN" : "CLOSED"
                };

            _displayHistory.Insert(0, historyItem);// Insert at the beginning to show the latest entry first

            TelemetryGrid.Items.Refresh();// Refresh the DataGrid to show the new entry
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new SensorDashBoard();
            sensorDashBoard.Show();
            this.Close();
        }
    }
}