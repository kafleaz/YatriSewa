using System;

namespace YatriSewa.Services
{
    public class KalmanFilter
    {
        private double q = 0.0001; // Process noise covariance
        private double r = 0.001; // Measurement noise covariance
        private double p = 1, k = 0;
        private double x; // Estimated value

        public KalmanFilter(double initialValue)
        {
            x = initialValue;
        }

        public double Update(double measurement)
        {
            // Prediction
            p = p + q;

            // Update step
            k = p / (p + r);
            x = x + k * (measurement - x);
            p = (1 - k) * p;

            return x;
        }
    }
}
