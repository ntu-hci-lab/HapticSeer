using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace BF1Detectors
{
    public class StateObject
    {
        private byte triggerState = 0;
        private ushort bulletCount = 0;
        private bool isAutoFire = false;
        private Nullable<DateTime> lastTriggerEnter = null;
        private Nullable<DateTime> lastTriggerExit = null;
        private string bulletInlet, xinputInlet, pulseInlet, fireOutlet;
        public Publisher publisher;
        public byte TriggerState { get => triggerState; set => triggerState = value; }
        public ushort BulletCount { get => bulletCount; set => bulletCount = value; }
        public bool IsAutoFire { get => isAutoFire; set => isAutoFire = value; }
        public Nullable<DateTime> LastTriggerEnter { get => lastTriggerEnter; set => lastTriggerEnter = value; }
        public Nullable<DateTime> LastTriggerExit { get => lastTriggerExit; set => lastTriggerExit = value; }
        public string FireOutlet { get => fireOutlet; set => fireOutlet = value; }
        public string BulletInlet { get => bulletInlet; set => bulletInlet = value; }
        public string XinputInlet { get => xinputInlet; set => xinputInlet = value; }
        public string PulseInlet { get => pulseInlet; set => pulseInlet = value; }

        public StateObject(Publisher publisher) => this.publisher = publisher;

    }
}
