using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class StateObject
    {
        private ushort curSpeed;
        private double curHandler;
        private double curAccelX;
        private double curAccelY;
        public Publisher publisher;

        public ushort CurSpeed { get => curSpeed; set => curSpeed = value; }
        public double CurHandler { get => curHandler; set => curHandler = value; }
        public double CurAccelX { get => curAccelX; set => curAccelX = value; }
        public double CurAccelY { get => curAccelY; set => curAccelY = value; }

        public StateObject(Publisher publisher) => this.publisher = publisher;

    }
}
