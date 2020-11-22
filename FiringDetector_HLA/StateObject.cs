using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace HLADetectors
{
    public class StateObject
    {
        private bool triggerState = false;
        private ushort bulletCount = 0;


        public Publisher publisher;
        public string openvrInlet, bulletInlet, fireOutlet;
        public bool TriggerState { get => triggerState; set => triggerState = value; }
        public ushort BulletCount { get => bulletCount; set => bulletCount = value; }
        public StateObject(Publisher publisher) => this.publisher = publisher;

    }
}
