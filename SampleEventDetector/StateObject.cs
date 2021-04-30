using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace PC2Detectors {
    public class StateObject
    {
        private const double EPS = 0.01d, ANGLE_ALPHA = 0.9, SPEED_ALPHA = 0.7;
        private LinkedList<double> speed = new LinkedList<double>();
        private LinkedList<double> handlerAngle = new LinkedList<double>();
        private double[] angleArray = new double[5];
        private double carLength, lastAngle;
        private byte frameLength;
        public string xinputInlet, speedInlet, accXOutlet, accYOutlet;
        public Publisher publisher;


        public StateObject(Publisher publisher, byte frameLength = 4, double carLength = 1d, 
            string accXOutlet = null, string accYOutlet = null)
        {
            this.frameLength = frameLength;
            this.carLength = carLength;
            this.publisher = publisher;
            this.accXOutlet = accXOutlet;
            this.accYOutlet = accYOutlet;
            for (int i = 0; i < frameLength; i++)
            {
                speed.AddLast(0);
                handlerAngle.AddLast(0);
            }
        }
        public double Speed
        {
            get => speed.Last.Value; 
            set
            {
                speed.AddLast(value * 0.27777777777 * (SPEED_ALPHA) + speed.Last.Value * (1-SPEED_ALPHA));
                speed.RemoveFirst();
            }
        }
        public double Angle
        {
            get => handlerAngle.Last.Value;
            set
            {
                var smoothedAngle = handlerAngle.Last.Value * ANGLE_ALPHA + Math.PI * value / 180d * (1 - ANGLE_ALPHA);
                smoothedAngle = Math.Abs(smoothedAngle) < EPS ? 0 : smoothedAngle;
                handlerAngle.AddLast(smoothedAngle);
                handlerAngle.RemoveFirst();
            }
        }
        public double AccelY
        {
            get => -(speed.Last.Value - speed.First.Value) / ((frameLength-1) * 0.06);
        }
        public double AccelX
        {
            get 
            {
                return Math.Clamp(-Math.Pow(Speed, 2) / carLength * Math.Sin(Angle), -20, 20);
            }
        }
        public double LastAngle { get => lastAngle; set => lastAngle = value; }
    }
}
