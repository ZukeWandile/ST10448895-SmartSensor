using Microsoft.Extensions.Configuration;
using sensorX.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace sensorX.Views
{

    public partial class LoginWindow : Window
    {
        private readonly FirebaseAuthService _authService;
        public LoginWindow()
        {
            InitializeComponent();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            FirebaseSettings settings = new();
            configuration.GetSection("Firebase").Bind(settings);

            _authService = new FirebaseAuthService(settings);
        }

        // Handles the Sign In action.
        private async void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Sign In",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loginButton = (Button)sender;
            loginButton.IsEnabled = false;

            try
            {
                var result = await _authService.SignInAsync(email, password);

                if (result.IsSuccess)
                {
                    MessageBox.Show($"Welcome, {result.Email}!");

                    GatewayWindow gw = new GatewayWindow();
                    gw.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage,
                        "Sign In Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Unexpected Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                loginButton.IsEnabled = true;
            }
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