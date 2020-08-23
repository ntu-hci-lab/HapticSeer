using RedisEndpoint;
using System;

namespace BF1Detectors
{
    public class StateObject
    {
        private byte realHP = 100;
        private DateTime lastHitSignal = DateTime.Now;
        private DateTime lastBloodLossSignal = DateTime.Now;
        private double lastHitAngle;
        private string bloodInlet, hitInlet, incomingOutlet;

        public Publisher publisher;
        public byte RealHP { get => realHP; set => realHP = value; }
        public DateTime LastHitSignal { get => lastHitSignal; set => lastHitSignal = value; }
        public DateTime LastBloodLossSignal { get => lastBloodLossSignal; set => lastBloodLossSignal = value; }
        public double LastHitAngle { get => lastHitAngle; set => lastHitAngle = value; }
        public string BloodInlet { get => bloodInlet; set => bloodInlet = value; }
        public string HitInlet { get => hitInlet; set => hitInlet = value; }
        public string IncomingOutlet { get => incomingOutlet; set => incomingOutlet = value; }
        public StateObject(Publisher publisher) => this.publisher = publisher;
    }
}
