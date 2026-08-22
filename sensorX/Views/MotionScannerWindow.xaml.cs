using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using sensorX.Models;
using sensorX.Services;

namespace sensorX.Views
{
    public partial class MotionScannerWindow : Window
    {
        private readonly Random _random = new();
        private readonly DispatcherTimer _scanTimer;
        private readonly List<MotionPoint> _motionPoints = new();
        private readonly MotionPathAnalyzer _pathAnalyzer;

        private Sensor? _selectedSensor;
        private MotionPoint? _currentPoint;

        public MotionScannerWindow()
        {
            InitializeComponent();

            _pathAnalyzer = new MotionPathAnalyzer();

            _scanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _scanTimer.Tick += ScanTimer_Tick;

            LoadMotionSensors();
        }

        private void LoadMotionSensors()
        {
            var motionSensors = SensorStore.Sensors
                .Where(sensor => sensor.Category.Equals("Motion Sensor", StringComparison.OrdinalIgnoreCase))
                .ToList();

            SensorComboBox.ItemsSource = motionSensors;

            if (motionSensors.Count == 0)
            {
                SensorInfoText.Text = "No registered motion sensors available.";
                StartButton.IsEnabled = false;
            }
            else
            {
                SensorComboBox.SelectedIndex = 0;
                SensorInfoText.Text = $"{motionSensors.Count} motion sensor(s) registered.";
            }
        }

        private void SensorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSensor = SensorComboBox.SelectedItem as Sensor;

            if (_selectedSensor == null)
                return;

            SensorInfoText.Text = $"Location: {_selectedSensor.Location} | Node: {_selectedSensor.NodeId}";

            ResetScanner();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSensor == null)
            {
                MessageBox.Show(
                    "Please select a registered motion sensor.",
                    "No Sensor Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _scanTimer.Start();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            StatusText.Text = "ACTIVE";
            StatusIndicator.Fill = Brushes.Green;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopScanning();
        }

        private void StopScanning()
        {
            _scanTimer.Stop();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;
        }

        private void ScanTimer_Tick(object? sender, EventArgs e)
        {
            GenerateMotionReading();
        }

        private void GenerateMotionReading()
        {
            bool personDetected = _random.Next(100) < 80;

            double confidence = personDetected
                ? _random.NextDouble() * 10 + 90
                : _random.NextDouble() * 20 + 60;

            double xPosition = _random.NextDouble() * 10;
            double yPosition = _random.NextDouble() * 10;
            double speed = personDetected ? _random.NextDouble() * 3 : 0;

            MovementText.Text = personDetected ? "DETECTED" : "NO MOVEMENT";
            ConfidenceText.Text = $"{confidence:F1}%";
            XPositionText.Text = $"{xPosition:F2}";
            YPositionText.Text = $"{yPosition:F2}";
            SpeedText.Text = $"{speed:F2} m/s";

            if (personDetected)
            {
                AddMotionPoint((float)xPosition, (float)yPosition);
            }

            UpdateHumanDisplay(personDetected, xPosition, yPosition);
        }

        private void AddMotionPoint(float x, float y)
        {
            MotionPoint currentPoint = new MotionPoint
            {
                X = x,
                Y = y,
                Previous = _currentPoint
            };

            _motionPoints.Add(currentPoint);
            _currentPoint = currentPoint;

            double distance = _pathAnalyzer.CalculateDistance(_currentPoint);

            Console.WriteLine($"Points: {_motionPoints.Count} | Distance: {distance:F2} m");
        }

        private void UpdateHumanDisplay(bool detected, double x, double y)
        {
            if (!detected)
            {
                HumanFigure.Visibility = Visibility.Hidden;
                DetectionText.Text = "NO MOVEMENT";
                DetectionText.Foreground = Brushes.Gray;
                return;
            }

            HumanFigure.Visibility = Visibility.Visible;

            // Convert simulated coordinates into Canvas coordinates.
            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            double left = (x / 10) * Math.Max(0, canvasWidth - 80);
            double top = (y / 10) * Math.Max(0, canvasHeight - 150);

            Canvas.SetLeft(HumanFigure, left);
            Canvas.SetTop(HumanFigure, top);

            DetectionText.Text = "HUMAN DETECTED";
            DetectionText.Foreground = Brushes.LightGreen;
        }

        private void ResetScanner()
        {
            _scanTimer.Stop();

            _motionPoints.Clear();
            _currentPoint = null;

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;

            MovementText.Text = "--";
            ConfidenceText.Text = "-- %";
            XPositionText.Text = "--";
            YPositionText.Text = "--";
            SpeedText.Text = "-- m/s";

            HumanFigure.Visibility =Visibility.Hidden;

            DetectionText.Text ="NO MOVEMENT";

            DetectionText.Foreground =Brushes.Gray;
        }
        protected override void OnClosed(EventArgs e)
        {
            _scanTimer.Stop();

            base.OnClosed(e);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new();
            sensorDashBoard.Show();
            Close();
        }
    }
}