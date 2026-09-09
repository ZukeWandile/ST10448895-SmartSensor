using Microsoft.Extensions.Configuration;
using sensorX.Services;
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
    public partial class SignUpWindow : Window
    {
        // Service used to handle Firebase authentication operations
        private readonly FirebaseAuthService _authService;

        public SignUpWindow()
        {
            InitializeComponent();

            // Load settings from the appsettings.json file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Bind configuration values to the FirebaseSettings model
            FirebaseSettings settings = new();
            configuration.GetSection("Firebase").Bind(settings);

            // Initialize the authentication service with loaded settings
            _authService = new FirebaseAuthService(settings);
        }

        private async void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve user input from the form fields
            // string Name = txtFullName.Text.Trim();
            string Email = txtEmail.Text.Trim();
            string Password = txtPassword.Password;
            string PasswordC = txtConfirmPassword.Password;

            // Validate that no required fields are left blank
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(PasswordC))
            {
                MessageBox.Show("Please fill in all the fields.",
                    "Sign Up", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate email format
            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
            }
            catch
            {
                MessageBox.Show("Please enter a valid email.");
                return;
            }

            // Ensure password meets minimum length requirement
            if (Password.Length < 9)
            {
                MessageBox.Show("Password must be at least 9 characters long.",
                   "Try Again", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ensure password contains at least one numeric digit
            if (!Password.Any(char.IsDigit))
            {
                MessageBox.Show("Password must contain at least one digit.",
                   "Try Again", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ensure password contains at least one uppercase letter
            if (!Password.Any(char.IsUpper))
            {
                MessageBox.Show("Password must contain at least one uppercase letter.",
                   "Try Again", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if password and confirmation match
            if (Password != PasswordC)
            {
                MessageBox.Show("Passwords don't match.",
                    "Try Again", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Temporarily disable the button to prevent multiple click submissions
            var createButton = (Button)sender;
            createButton.IsEnabled = false;

            try
            {
                // Attempt to create the user account in Firebase
                var result = await _authService.SignUpAsync(Email, Password);

                if (result.IsSuccess)
                {
                    // Try to send a verification email using the token returned by Firebase
                    bool emailSent = await _authService.SendVerificationEmailAsync(result.IdToken!);

                    if (emailSent)
                    {
                        MessageBox.Show(
                            "Account created successfully!\n\nA verification email has been sent to your inbox.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Account created, but the verification email could not be sent.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Open the main gateway window and close the sign-up screen
                    GatewayWindow gw = new GatewayWindow();
                    gw.Show();

                    this.Close();
                }
                else
                {
                    // Show Firebase's actual reason for failure (e.g. "email already exists")
                    MessageBox.Show(result.ErrorMessage, "Sign Up Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during the process
                MessageBox.Show(
                    ex.Message,
                    "Unexpected Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the create account button once execution finishes
                createButton.IsEnabled = true;
            }
        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            // Open Login Window & Close Sign Up Window
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}