using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using sensorX.Models;
using sensorX.Services;

namespace sensorX.Views
{
    public partial class MotionScannerWindow : Window
    {
        

        // Simulation timers and tracking data
        private readonly Random _random = new();
        private readonly DispatcherTimer _scanTimer;
        private readonly List<MotionPoint> _motionPoints = new();
        private readonly MotionPathAnalyzer _pathAnalyzer;
        private DispatcherTimer? _replayTimer;

        // Active sensor and point state
        private Sensor? _selectedSensor;
        private MotionPoint? _currentPoint;
        private MotionNode? _motionSensorNode;
        private Point? _previousReplayPoint;

        // Simulation coordinates (bounded between 0 and 10)
        private double _currentX = 5;
        private double _currentY = 5;

        // Canvas point trackers and batch storage
        private Point? _previousCanvasPoint;
        private float[][]? _lastMotionBatch;
        private int _replayIndex;

        // Metrics tracking
        private DateTime _scanStartTime;
        private int _detectionCount;
        private double _totalSpeed;
        private double _maximumSpeed;
        private double _totalConfidence;

        // Centering offsets for canvas human figure
        private const double HumanFigureHalfWidth = 30;
        private const double HumanFigureVerticalOffset = 50;

      

       

        public MotionScannerWindow()
        {
            InitializeComponent();

            _pathAnalyzer = new MotionPathAnalyzer();

            // Set up scan simulation timer (500ms intervals)
            _scanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _scanTimer.Tick += ScanTimer_Tick;

            LoadMotionSensors();
        }

        // Populates the dropdown with registered motion sensors
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

        

       

        // Handles dropdown selection changes
        private void SensorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSensor = SensorComboBox.SelectedItem as Sensor;

            if (_selectedSensor == null)
                return;

            SensorInfoText.Text = $"Location: {_selectedSensor.Location} | Node: {_selectedSensor.NodeId}";
            ResetScanner();
        }

        // Starts the live scan session
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

            BuildMotionHierarchy();

            if (!ValidateMotionHierarchy())
            {
                MessageBox.Show(
                    "Motion sensor hierarchy validation failed.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _motionPoints.Clear();
            _currentPoint = null;

            ClearMotionPath();

            // Reset scan statistics
            _detectionCount = 0;
            _totalSpeed = 0;
            _maximumSpeed = 0;
            _totalConfidence = 0;
            _scanStartTime = DateTime.Now;

            ClearStatisticsDisplay();

            _scanTimer.Start();

            // Update UI state for active scanning
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StatusText.Text = "SCANNING";
            StatusIndicator.Fill = Brushes.LimeGreen;
        }

        // Stops live scanning
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopScanning();
        }

        // Halts scan timer and saves raw motion batch
        private void StopScanning()
        {
            _scanTimer.Stop();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;

            _lastMotionBatch = ConvertMotionPointsToRawBatch();
        }

        // Navigates back to dashboard
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            SensorDashBoard sensorDashBoard = new();
            sensorDashBoard.Show();
            Close();
        }

        

        // Scan timer tick event handler
        private void ScanTimer_Tick(object? sender, EventArgs e)
        {
            GenerateMotionReading();
        }

        // Simulates sensor readings and updates UI stats
        private void GenerateMotionReading()
        {
            bool personDetected = _random.Next(100) < 80;

            double confidence = personDetected
                ? _random.NextDouble() * 10 + 90
                : _random.NextDouble() * 20 + 60;

            double movementX = (_random.NextDouble() - 0.5) * 1.2;
            double movementY = (_random.NextDouble() - 0.5) * 1.2;

            _currentX = Math.Clamp(_currentX + movementX, 0, 10);
            _currentY = Math.Clamp(_currentY + movementY, 0, 10);

            double xPosition = _currentX;
            double yPosition = _currentY;
            double speed = personDetected ? _random.NextDouble() * 3 : 0;

            if (personDetected)
            {
                _detectionCount++;
                _totalSpeed += speed;

                if (speed > _maximumSpeed)
                    _maximumSpeed = speed;

                _totalConfidence += confidence;
            }

            // Update live metrics display
            MovementText.Text = personDetected ? "DETECTED" : "NO MOVEMENT";
            ConfidenceText.Text = $"{confidence:F1}%";
            XPositionText.Text = $"{xPosition:F2}";
            YPositionText.Text = $"{yPosition:F2}";
            SpeedText.Text = $"{speed:F2} m/s";

            if (personDetected)
            {
                AddMotionPoint((float)xPosition, (float)yPosition);
                DrawMotionPath(xPosition, yPosition);
            }

            UpdateHumanDisplay(personDetected, xPosition, yPosition);
            UpdateStatisticsDisplay();
        }

        // Adds tracked point and updates distance calculation
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

            PointCountText.Text = _motionPoints.Count.ToString();
            DistanceText.Text = $"{distance:F2} m";
        }

        // Renders visual path lines on canvas
        private void DrawMotionPath(double x, double y)
        {
            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            double canvasX = (x / 10) * canvasWidth;
            double canvasY = (y / 10) * canvasHeight;

            Point currentPoint = new Point(canvasX, canvasY);

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

        // Updates position of human icon on canvas
        private void UpdateHumanDisplay(bool detected, double x, double y)
        {
            if (!detected)
            {
                HumanFigure.Visibility = Visibility.Hidden;
                DetectionText.Text = "NO MOVEMENT";
                DetectionText.Foreground = Brushes.Gray;
                _previousCanvasPoint = null;
                return;
            }

            HumanFigure.Visibility = Visibility.Visible;

            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            double canvasX = (x / 10) * canvasWidth;
            double canvasY = (y / 10) * canvasHeight;

            double left = Math.Max(0, canvasX - HumanFigureHalfWidth);
            double top = Math.Max(0, canvasY - HumanFigureVerticalOffset);

            Canvas.SetLeft(HumanFigure, left);
            Canvas.SetTop(HumanFigure, top);

            DetectionText.Text = "HUMAN DETECTED";
            DetectionText.Foreground = Brushes.LightGreen;
        }

       
        // Begins recorded path replay
        private void ReplayPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastMotionBatch == null || _lastMotionBatch.Length == 0)
            {
                MessageBox.Show(
                    "There is no recorded motion path to replay.",
                    "No Path Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _scanTimer.Stop();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;

            ClearMotionPath();

            _replayIndex = 0;
            _previousReplayPoint = null;

            _replayTimer?.Stop();
            _replayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _replayTimer.Tick += ReplayTimer_Tick;
            _replayTimer.Start();

            StatusText.Text = "REPLAYING";
            StatusIndicator.Fill = Brushes.DeepSkyBlue;
        }

        // Replay timer tick handler
        private void ReplayTimer_Tick(object? sender, EventArgs e)
        {
            if (_lastMotionBatch == null)
                return;

            if (_replayIndex >= _lastMotionBatch.Length)
            {
                StopReplay();
                return;
            }

            float[] point = _lastMotionBatch[_replayIndex];
            DrawReplayPoint(point[0], point[1]);

            _replayIndex++;
        }

        // Draws lines and places human figure during replay
        private void DrawReplayPoint(double x, double y)
        {
            double canvasWidth = MotionCanvas.ActualWidth;
            double canvasHeight = MotionCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            double canvasX = (x / 10) * canvasWidth;
            double canvasY = (y / 10) * canvasHeight;

            Point currentPoint = new Point(canvasX, canvasY);

            if (_previousReplayPoint.HasValue)
            {
                Line replayLine = new Line
                {
                    X1 = _previousReplayPoint.Value.X,
                    Y1 = _previousReplayPoint.Value.Y,
                    X2 = currentPoint.X,
                    Y2 = currentPoint.Y,
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 3
                };

                MotionCanvas.Children.Add(replayLine);
            }

            Canvas.SetLeft(HumanFigure, Math.Max(0, canvasX - HumanFigureHalfWidth));
            Canvas.SetTop(HumanFigure, Math.Max(0, canvasY - HumanFigureVerticalOffset));

            HumanFigure.Visibility = Visibility.Visible;

            DetectionText.Text = "REPLAYING PATH";
            DetectionText.Foreground = Brushes.LimeGreen;

            XPositionText.Text = $"{x:F2}";
            YPositionText.Text = $"{y:F2}";

            _previousReplayPoint = currentPoint;
        }

        // Halts replay and resets status text
        private void StopReplay()
        {
            if (_replayTimer != null)
            {
                _replayTimer.Stop();
                _replayTimer.Tick -= ReplayTimer_Tick;
                _replayTimer = null;
            }

            _previousReplayPoint = null;
            StartButton.IsEnabled = true;

            StatusText.Text = "REPLAY COMPLETE";
            StatusIndicator.Fill = Brushes.Gray;

            DetectionText.Text = "PATH REPLAY COMPLETE";
            DetectionText.Foreground = Brushes.LightGreen;
        }

       

        // Updates panel scan statistics values
        private void UpdateStatisticsDisplay()
        {
            TimeSpan duration = DateTime.Now - _scanStartTime;

            ScanTimeText.Text = duration.ToString(@"mm\:ss");
            DetectionCountText.Text = _detectionCount.ToString();
            PointCountText.Text = _motionPoints.Count.ToString();

            if (_detectionCount > 0)
            {
                double averageSpeed = _totalSpeed / _detectionCount;
                double averageConfidence = _totalConfidence / _detectionCount;

                AverageSpeedText.Text = $"{averageSpeed:F2} m/s";
                MaximumSpeedText.Text = $"{_maximumSpeed:F2} m/s";
                AverageConfidenceText.Text = $"{averageConfidence:F1}%";
            }
            else
            {
                AverageSpeedText.Text = "0.00 m/s";
                MaximumSpeedText.Text = "0.00 m/s";
                AverageConfidenceText.Text = "0.0%";
            }
        }

        // Clears calculated scan statistics UI fields
        private void ClearStatisticsDisplay()
        {
            ScanTimeText.Text = "00:00";
            DetectionCountText.Text = "0";
            AverageSpeedText.Text = "0.00 m/s";
            MaximumSpeedText.Text = "0.00 m/s";
            AverageConfidenceText.Text = "0.0%";
        }

        // Resets scanner UI and internal tracking state
        private void ResetScanner()
        {
            _scanTimer.Stop();
            _replayTimer?.Stop();

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

            _detectionCount = 0;
            _totalSpeed = 0;
            _maximumSpeed = 0;
            _totalConfidence = 0;
            ClearStatisticsDisplay();

            _lastMotionBatch = null;
        }

        // Clears path lines from canvas and resets position defaults
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

        // Clears stored path data, statistics, and canvas elements
        private void ClearPathButton_Click(object sender, RoutedEventArgs e)
        {
            _scanTimer.Stop();
            _replayTimer?.Stop();

            _motionPoints.Clear();
            _currentPoint = null;
            _lastMotionBatch = null;

            ClearMotionPath();

            _detectionCount = 0;
            _totalSpeed = 0;
            _maximumSpeed = 0;
            _totalConfidence = 0;

            ClearStatisticsDisplay();

            PointCountText.Text = "0";
            DistanceText.Text = "0.00 m";
            XPositionText.Text = "--";
            YPositionText.Text = "--";
            SpeedText.Text = "-- m/s";

            DetectionText.Text = "NO MOVEMENT";
            DetectionText.Foreground = Brushes.Gray;

            HumanFigure.Visibility = Visibility.Hidden;

            StatusText.Text = "STOPPED";
            StatusIndicator.Fill = Brushes.Gray;

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        // Converts point collection to a raw float array batch
        private float[][] ConvertMotionPointsToRawBatch()
        {
            float[][] rawBatch = new float[_motionPoints.Count][];

            for (int i = 0; i < _motionPoints.Count; i++)
            {
                rawBatch[i] = new float[]
                {
                    (float)_motionPoints[i].X,
                    (float)_motionPoints[i].Y
                };
            }

            return rawBatch;
        }

        // Builds hierarchy node network
        private void BuildMotionHierarchy()
        {
            MotionNode facility = new MotionNode("Facility A");

            MotionNode zone = new MotionNode("Zone 1")
            {
                Parent = facility
            };

            MotionNode subZone = new MotionNode("SubZone B")
            {
                Parent = zone
            };

            _motionSensorNode = new MotionNode(_selectedSensor?.NodeId ?? "Motion Sensor")
            {
                Parent = subZone
            };
        }

        // Validates active hierarchy setup
        private bool ValidateMotionHierarchy()
        {
            if (_motionSensorNode == null)
                return false;

            MotionHierarchyValidator validator = new MotionHierarchyValidator();
            return validator.Validate(_motionSensorNode);
        }

        // Window closing cleanup
        protected override void OnClosed(EventArgs e)
        {
            _scanTimer.Stop();
            _replayTimer?.Stop();

            base.OnClosed(e);
        }

        
    }
}