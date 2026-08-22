using System;
using sensorX.Models;

namespace sensorX.Services
{
    public class MotionDataGenerator
    {
        private readonly Random _random = new Random();

        public float[][] GenerateFrames(int numberOfFrames)
        {
            float[][] frames = new float[numberOfFrames][];

            float x = 1.0f;
            float y = 1.0f;

            for (int i = 0; i < numberOfFrames; i++)
            {
                x += (float)(_random.NextDouble() * 1.0 - 0.2);

                y += (float)(_random.NextDouble() * 1.0 - 0.2);

                x = Math.Clamp(x, 0, 10); //Don't let X go outside simulated room.
                y = Math.Clamp(y, 0, 10);

                frames[i] = new float[] { x,y };
            }

            return frames;
        }

        public List<MotionPoint> ConvertToMotionPoints(float[][] rawFrames)// 
        {
            List<MotionPoint> points = new();

            MotionPoint? previous = null;

            foreach (float[] frame in rawFrames)
            {
                MotionPoint current = new MotionPoint
                {
                    X = frame[0],
                    Y = frame[1],
                    Previous = previous
                };

                points.Add(current);

                previous = current;
            }

            return points;
        }

    }
}