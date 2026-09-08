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

namespace sensorX.Views
{
    /// <summary>
    /// Interaction logic for GatewayWindow.xaml
    /// </summary>
    public partial class GatewayWindow : Window
    {
        public GatewayWindow()
        {
            InitializeComponent();
        }

        private void Sensor_Ingestion_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new SensorDashBoard();
            sensorDashBoard.Show();
            this.Close();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
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
