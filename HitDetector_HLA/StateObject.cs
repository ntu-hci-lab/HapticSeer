using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class StateObject
    {
        private byte realHP = 100;
        private DateTime lastBloodLossSignal = DateTime.Now;
        private DateTime? lastHPBurst = null;

        public Publisher publisher;
        public byte RealHP { get => realHP; set => realHP = value; }
        public string bloodInlet, hitOutlet;
        public DateTime LastBloodLossSignal { get => lastBloodLossSignal; set => lastBloodLossSignal = value; }
        public DateTime? LastHPBurst { get => lastHPBurst; set => lastHPBurst = value; }

        public StateObject(Publisher publisher) => this.publisher = publisher;
    }
}
