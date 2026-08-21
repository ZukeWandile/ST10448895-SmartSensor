using System;
using System.Windows;
using System.Windows.Input;

namespace sensorX.Views
{

    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Handles the Sign In action.
        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail?.Text?.Trim() ?? string.Empty;
            string password = TxtPassword?.Password ?? string.Empty;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //  Replace with  actual authentication logic / ViewModel command call
            MessageBox.Show($"Welcome back, {email}!", "Login Successful",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            // Open main GateWays
             GatewayWindow GW = new GatewayWindow();
             GW.Show();
             this.Close();
        }

        // Handles the Continue as Guest action.
       
        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Continuing as guest .", "Guest Access",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: Navigate to guest session view
            GatewayWindow GW = new GatewayWindow();
            GW.Show();
            this.Close();
        }

  
        // Allows the user to drag the window by clicking anywhere on the background.
    
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Opens the SignUpWindow when "Create an account" is clicked.
        
        private void SignUp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SignUpWindow signUp = new SignUpWindow();
            signUp.Show();
            this.Close();
        }
    }
}