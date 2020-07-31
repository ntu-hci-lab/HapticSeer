using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class HealthState
    {
        private byte realHP = 100;
        private DateTime lastHitSignal = DateTime.Now;
        private DateTime lastBloodLossSignal = DateTime.Now;

        public Publisher publisher;
        public byte RealHP { get => realHP; set => realHP = value; }
        public DateTime LastHitSignal { get => lastHitSignal; set => lastHitSignal = value; }
        public DateTime LastBloodLossSignal { get => lastBloodLossSignal; set => lastBloodLossSignal = value; }

        public HealthState(Publisher publisher) => this.publisher = publisher;
    }
}
