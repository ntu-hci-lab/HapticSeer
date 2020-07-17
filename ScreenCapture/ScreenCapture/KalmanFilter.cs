using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class KalmanFilter
    {
        private double A, H, Q, R, P, x;

        public KalmanFilter(double A, double H, double Q, double R, double initial_P, double initial_x)
        {
            this.A = A;
            this.H = H;
            this.Q = Q;
            this.R = R;
            this.P = initial_P;
            this.x = initial_x;
        }

        public double Output(double input)
        {
            // time update - prediction
            x = A * x;
            P = A * P * A + Q;

            // measurement update - correction
            double K = P * H / (H * P * H + R);
            x = x + K * (input - H * x);
            P = (1 - K * H) * P;

            return x;
        }
    }
}
