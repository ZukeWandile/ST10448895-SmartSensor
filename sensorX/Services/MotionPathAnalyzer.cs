using sensorX.Models;

namespace sensorX.Services
{
    public class MotionPathAnalyzer
    {
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