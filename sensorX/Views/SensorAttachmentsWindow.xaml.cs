using System.Diagnostics;
using System.Windows;
using sensorX.Models;
using sensorX.Services;

namespace sensorX.Views
{
    //  responsible for viewing, adding, and opening file attachments linked to a specific sensor
    public partial class SensorAttachmentsWindow : Window
    {
        // Maximum number of attachments allowed per sensor
        private const int MaxAttachments = 10;
        private readonly Sensor _sensor;
        private readonly AttachmentStorageService _storage = new();

        // Constructor initializes the window and binds the sensor's attachment data to the UI
        public SensorAttachmentsWindow(Sensor sensor)
        {
            InitializeComponent();
            _sensor = sensor;

            // Set the window header to include the sensor's name and bind the list view to its attachments
            txtHeader.Text = $"Attachments — {sensor.DisplayName}";
            lvAttachments.ItemsSource = _sensor.Attachments;

            // Update the UI counter for the current number of files
            RefreshCount();
        }

        // Handles the click event for adding a new file attachment
        private void BtnAttach_Click(object sender, RoutedEventArgs e)
        {
            // Check if the sensor has already reached the maximum allowed attachment limit
            if (_sensor.Attachments.Count >= MaxAttachments)
            {
                MessageBox.Show($"Maximum of {MaxAttachments} attachments per sensor.",
                    "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open a file selection dialog filtered to supported file types
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Attach a file to this sensor",
                Filter = "All supported files|*.jpg;*.jpeg;*.png;*.txt;*.log;*.json;*.cfg|All files|*.*",
                Multiselect = false
            };

            // If the user selects a file and clicks OK, save and add it to the sensor
            if (dialog.ShowDialog() == true)
            {
                var attachment = _storage.SaveAttachment(dialog.FileName, _sensor.MacAddress);
                _sensor.Attachments.Add(attachment); // ObservableCollection automatically updates the ListView UI
                RefreshCount();
            }
        }

        // Handles double-clicking an attachment to open it with the system's default application
        private void LvAttachments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lvAttachments.SelectedItem is SensorAttachment attachment)
            {
                Process.Start(new ProcessStartInfo(attachment.StoredPath) { UseShellExecute = true });
            }
        }

        // Updates the text display tracking current file count vs the maximum limit
        private void RefreshCount()
        {
            txtCount.Text = $"{_sensor.Attachments.Count} / {MaxAttachments} files";
        }
        // Handles the click event for the Back button, closing this window and returning to the caller
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}