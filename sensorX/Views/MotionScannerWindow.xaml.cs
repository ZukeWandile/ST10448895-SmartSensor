using sensorX.Models;
using sensorX.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace sensorX.Views
{

    public partial class MotionScannerWindow : Window
    {
        // Helper fields for simulation and data tracking
        private readonly Random _random = new();
        private readonly DispatcherTimer _scanTimer;
        private readonly List<MotionPoint> _motionPoints = new();
        private readonly MotionPathAnalyzer _pathAnalyzer;

        // Currently selected sensor and point data
        private Sensor? _selectedSensor;
        private MotionPoint? _currentPoint;

        // Simulation coordinates (0-10 range)
        private double _currentX = 5;
        private double _currentY = 5;

        // Stores the previous point drawn on the canvas to render path lines
        private Point? _previousCanvasPoint;

        public MotionScannerWindow()
        {
            InitializeComponent();

            // Initialize path analyzer service
            _pathAnalyzer = new MotionPathAnalyzer();

            // Set up a timer to simulate periodic sensor scanning (every 500ms)
            _scanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _scanTimer.Tick += ScanTimer_Tick;

            // Populate the dropdown with motion sensors
            LoadMotionSensors();
        }

        // Loads available motion sensors into the UI ComboBox
        private void LoadMotionSensors()
        {
            var motionSensors = SensorStore.Sensors
                .Where(sensor => sensor.Category.Equals("Motion Sensor", StringComparison.OrdinalIgnoreCase))
                .ToList();

            SensorComboBox.ItemsSource = motionSensors;

            // Handle UI state if no sensors exist
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

        // Handles sensor selection updates from the dropdown
        private void SensorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSensor = SensorComboBox.SelectedItem as Sensor;

            if (_selectedSensor == null)
                return;

            SensorInfoText.Text = $"Location: {_selectedSensor.Location} | Node: {_selectedSensor.NodeId}";

            // Reset state when switching sensors
            ResetScanner();
        }

        // Starts the motion scan simulation
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

            // Clear previous scan data
            _motionPoints.Clear();
            _currentPoint = null;
            ClearMotionPath();

            PointCountText.Text = "0";
            DistanceText.Text = "0.00 m";

            // Start timer and update button states
            _scanTimer.Start();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            StatusText.Text = "ACTIVE";
            StatusIndicator.Fill = Brushes.Green;
        }

        // Stops the scan via button click
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopScanning();
        }

        // Stops the timer and resets UI status indicators
        private void StopScanning()
        {
            _scanTimer.Stop();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;
        }

        // Timer tick event to trigger new simulated readings
        private void ScanTimer_Tick(object? sender, EventArgs e)
        {
            GenerateMotionReading();
        }

        // Generates random simulation readings for motion, confidence, and position
        private void GenerateMotionReading()
        {
            // 80% chance to detect a person
            bool personDetected = _random.Next(100) < 80;

            // Calculate confidence level based on detection status
            double confidence = personDetected
                ? _random.NextDouble() * 10 + 90
                : _random.NextDouble() * 20 + 60;

            // Generate small random positional shifts
            double movementX = (_random.NextDouble() - 0.5) * 1.2;
            double movementY = (_random.NextDouble() - 0.5) * 1.2;

            _currentX += movementX;
            _currentY += movementY;

            // Clamp positions within bounds (0 to 10)
            if (_currentX < 0) _currentX = 0;
            if (_currentX > 10) _currentX = 10;
            if (_currentY < 0) _currentY = 0;
            if (_currentY > 10) _currentY = 10;

            double xPosition = _currentX;
            double yPosition = _currentY;
            double speed = personDetected ? _random.NextDouble() * 3 : 0;

            // Update UI metrics display
            MovementText.Text = personDetected ? "DETECTED" : "NO MOVEMENT";
            ConfidenceText.Text = $"{confidence:F1}%";
            XPositionText.Text = $"{xPosition:F2}";
            YPositionText.Text = $"{yPosition:F2}";
            SpeedText.Text = $"{speed:F2} m/s";

            // Record data and draw path if a person was detected
            if (personDetected)
            {
                AddMotionPoint((float)xPosition, (float)yPosition);
                DrawMotionPath(xPosition, yPosition);
            }

            // Update human graphic location on the Canvas
            UpdateHumanDisplay(personDetected, xPosition, yPosition);
        }

        // Stores tracked coordinates and calculates accumulated distance
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

            // Calculate total distance traveled using path analyzer service
            double distance = _pathAnalyzer.CalculateDistance(_currentPoint);

            PointCountText.Text = _motionPoints.Count.ToString();
            DistanceText.Text = $"{distance:F2} m";
        }

        // Renders visual path lines on the WPF Canvas element
        private void DrawMotionPath(double x, double y)
        {
            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            // Scale coordinates from 0-10 domain to Canvas screen pixels
            double canvasX = (x / 10) * canvasWidth;
            double canvasY = (y / 10) * canvasHeight;

            Point currentPoint = new Point(canvasX, canvasY);

            // Connect previous point to current point with a line
            if (_previousCanvasPoint.HasValue)
            {
                Line pathLine = new Line
                {
                    X1 = _previousCanvasPoint.Value.X,
                    Y1 = _previousCanvasPoint.Value.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y,
                    Stroke = Brushes.DeepSkyBlue,
                    StrokeThickness = 3
                };

                MotionCanvas.Children.Add(pathLine);
            }

            _previousCanvasPoint = currentPoint;
        }

        // Updates the position and visual state of the target avatar on the UI
        private void UpdateHumanDisplay(bool detected, double x, double y)
        {
            if (!detected)
            {
                HumanFigure.Visibility = Visibility.Hidden;
                DetectionText.Text = "NO MOVEMENT";
                DetectionText.Foreground = Brushes.Gray;

                // Break the path until the next detection event occurs
                _previousCanvasPoint = null;
                return;
            }

            HumanFigure.Visibility = Visibility.Visible;

            // Convert simulated coordinates into Canvas coordinates (adjusted for target icon size)
            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            double left = (x / 10) * Math.Max(0, canvasWidth - 60);
            double top = (y / 10) * Math.Max(0, canvasHeight - 100);

            Canvas.SetLeft(HumanFigure, left);
            Canvas.SetTop(HumanFigure, top);

            DetectionText.Text = "HUMAN DETECTED";
            DetectionText.Foreground = Brushes.LightGreen;
        }

        // Resets UI elements and internal state back to defaults
        private void ResetScanner()
        {
            _scanTimer.Stop();

            _motionPoints.Clear();
            _currentPoint = null;

            ClearMotionPath();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;

            MovementText.Text = "--";
            ConfidenceText.Text = "-- %";
            XPositionText.Text = "--";
            YPositionText.Text = "--";
            SpeedText.Text = "-- m/s";

            PointCountText.Text = "0";
            DistanceText.Text = "0.00 m";

            HumanFigure.Visibility = Visibility.Hidden;

            DetectionText.Text = "NO MOVEMENT";
            DetectionText.Foreground = Brushes.Gray;
        }

        // Removes line objects from the UI canvas and resets origin positions
        private void ClearMotionPath()
        {
            for (int i = MotionCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (MotionCanvas.Children[i] is Line)
                {
                    MotionCanvas.Children.RemoveAt(i);
                }
            }

            _previousCanvasPoint = null;

            _currentX = 5;
            _currentY = 5;
        }

        // Ensures active timers are stopped when window is closed
        protected override void OnClosed(EventArgs e)
        {
            _scanTimer.Stop();
            base.OnClosed(e);
        }

        // Opens the dashboard window and closes this window
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new();
            sensorDashBoard.Show();
            Close();
        }

        // Clears line data and resets active position UI text
        private void ClearPathButton_Click(object sender, RoutedEventArgs e)
        {
            _motionPoints.Clear();
            _currentPoint = null;

            ClearMotionPath();

            XPositionText.Text = "--";
            YPositionText.Text = "--";
            SpeedText.Text = "-- m/s";

            DetectionText.Text = "NO MOVEMENT";
        }
    }
}