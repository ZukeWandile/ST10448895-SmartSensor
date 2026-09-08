using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading; // needed for DispatcherTimer
using sensorX.Services;         // ApiClient class

namespace sensorX.Views
{
    public partial class GatewayWindow : Window
    {
        // API client instance 
        // check API_X's Properties/launchSettings.json for the  port
        private readonly ApiClient _api = new ApiClient("https://localhost:7100");

        // Timer to keep re-checking API status every few seconds while window is open
        private readonly DispatcherTimer _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };

        public GatewayWindow()
        {
            InitializeComponent();

            // every time the timer ticks, re-check the API status
            _pollTimer.Tick += async (s, e) => await CheckApiStatus();
        }

        // Opens the Sensor Ingestion feature and closes this gateway window
        private void Sensor_Ingestion_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new SensorDashBoard();
            sensorDashBoard.Show();
            this.Close();
        }

        // Runs once when the window first opens
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckApiStatus(); // check immediately on load
            _pollTimer.Start();     // then keep checking every 5 seconds
        }

        // Does the API check and updates the status text/color
        private async System.Threading.Tasks.Task CheckApiStatus()
        {
            bool online = await _api.IsApiOnlineAsync();

            if (online)
            {
                txtApiStatus.Text = "API Connected";
                txtApiStatus.Foreground = Brushes.LimeGreen;
            }
            else
            {
                txtApiStatus.Text = "API Offline";
                txtApiStatus.Foreground = Brushes.Red;
            }
        }
    }
}