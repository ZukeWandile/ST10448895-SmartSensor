using System;
using System.Collections.Generic;
using sensorX.Models;

namespace sensorX.Services
{
    public class MotionDataGenerator
    {
        // Random number generator used to simulate continuous motion shifts
        private readonly Random _random = new();

      
        // Generates a jagged array representing simulated motion coordinate frames over time.
        /// <param name="numberOfFrames">The total number of coordinate frames to generate.</param>
        // <returns>A jagged array of floats where each entry contains [X, Y] coordinates.</returns>
        public float[][] GenerateFrames(int numberOfFrames)
        {
            float[][] frames = new float[numberOfFrames][];

            // Initial starting position within the simulated room
            float x = 1.0f;
            float y = 1.0f;

            for (int i = 0; i < numberOfFrames; i++)
            {
                // Introduce small random movements per frame update
                x += (float)(_random.NextDouble() * 1.0 - 0.2);
                y += (float)(_random.NextDouble() * 1.0 - 0.2);

                // Ensure coordinates remain within the simulated room bounds (0 to 10)
                x = Math.Clamp(x, 0, 10);
                y = Math.Clamp(y, 0, 10);

                // Store the calculated coordinates into the current frame
                frames[i] = new float[] { x, y };
            }

            return frames;
        }

       
        // Converts a raw jagged array of coordinate frames into a linked list of MotionPoint objects.
        /// <param name="rawFrames">The raw [X, Y] coordinate frames to convert.</param>
        // <returns>A list of MotionPoint instances linked together sequentially.</returns>
        public List<MotionPoint> ConvertToMotionPoints(float[][] rawFrames)
        {
            List<MotionPoint> points = new();

            MotionPoint? previous = null;

            foreach (float[] frame in rawFrames)
            {
                // Create a new motion point maintaining a reference to the previous node
                MotionPoint current = new MotionPoint
                {
                    X = frame[0],
                    Y = frame[1],
                    Previous = previous
                };

                points.Add(current);

                // Update the previous reference for the next iteration
                previous = current;
            }

            return points;
        }
    }
}