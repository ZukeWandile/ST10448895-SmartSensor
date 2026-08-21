using System.Windows;

namespace sensorX.Views
{
  
    public partial class SensorDashBoard : Window
    {
        public SensorDashBoard()
        {
            InitializeComponent();
        }


        // Handles navigation clicks for the side menu items.

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                switch (element.Name)
                {
                    case "BtnSensorIngestion":
                        SensorWindow sw = new SensorWindow();
                        sw.Show();
                        this.Close();
                        break;

                    case "BtnLiveTelemetry":
                        MessageBox.Show("Live Telemetry view selected.", "Navigation",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        TelemetryWindow tw = new TelemetryWindow();
                        tw.Show();
                        this.Close();
                        break;

                    case "BtnMotionScanner":
                        MessageBox.Show("Motion Scanner view selected.", "Navigation",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        MotionScannerWindow msw = new MotionScannerWindow();
                        msw.Show();
                        this.Close();
                        break;
                }
            }
        }


        // Handles signing out and returning to the Login window.
        private void BtnSignOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to sign out?",
                "Sign Out",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}