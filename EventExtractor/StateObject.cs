using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class StateObject
    {
        private const double EPS = 5d;
        private LinkedList<double> speed = new LinkedList<double>();
        private LinkedList<double> handlerAngle = new LinkedList<double>();
        private double[] angleArray = new double[5];
        private double carLength, lastAngle;
        private byte frameLength;
        public Publisher publisher;


        public StateObject(Publisher publisher, byte frameLength = 4, double carLength = 1d)
        {
            this.frameLength = frameLength;
            this.carLength = carLength;
            this.publisher = publisher;
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
                speed.AddLast(value * 0.27777777777);
                speed.RemoveFirst();
            }
        }
        public double Angle
        {
            get => handlerAngle.Last.Value;
            set
            {
                handlerAngle.AddLast(Math.Abs(value)<EPS? 0:(Math.PI * value / 180d));
                handlerAngle.RemoveFirst();
            }
        }
        public double AccelX
        {
            get => (speed.Last.Value - speed.First.Value) / (frameLength / 30d);
        }
        public double AccelY
        {
            get 
            {
                handlerAngle.CopyTo(angleArray, 0);
                double w1 = (carLength / Math.Sin(angleArray[1]) - carLength / Math.Sin(angleArray[0])) * 30;
                double w2 = (carLength / Math.Sin(angleArray[3]) - carLength / Math.Sin(angleArray[2])) * 30;
                double secondD = (w2- w1) * 15;

                return Math.Clamp((-Math.Pow(Speed, 2) / (carLength / Math.Sin((angleArray[1] + angleArray[2]) / 2)) + secondD) / 80, -3d, 3d);
            }
        }
        public double LastAngle { get => lastAngle; set => lastAngle = value; }
    }
}
