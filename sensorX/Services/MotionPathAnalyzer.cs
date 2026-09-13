using System.Collections.Generic;
using sensorX.Models;

namespace sensorX.Services
{
    public class MotionPathAnalyzer
    {
        // Overload: accepts a List<MotionPoint> instead of a single MotionPoint.
        // It simply finds the most recent point and hands off to the other version below.
        public double CalculateDistance(List<MotionPoint> points)
        {
            if (points == null || points.Count == 0)
                return 0;

            return CalculateDistance(points[^1]);
        }

        public double CalculateDistance(MotionPoint point)
        {
            // Base case
            if (point.Previous == null)
            {
                return 0;
            }

            // Calculate distance between this point and the previous point.
            double dx = point.X - point.Previous.X;
            double dy = point.Y - point.Previous.Y;

            double distance = Math.Sqrt((dx * dx) + (dy * dy));

            // Recursively calculate the distance travelled before this point.
            return distance + CalculateDistance(point.Previous);
        }
    }
}