using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class StateObject
    {
        private byte triggerState = 0;
        private ushort bulletCount = 0;
        private DateTime lastTriggerExit = DateTime.Now;
        public Publisher publisher;

        public StateObject(Publisher publisher) => this.publisher = publisher;
        public byte TriggerState { get => triggerState; set => triggerState = value; }
        public ushort BulletCount { get => bulletCount; set => bulletCount = value; }
        public DateTime LastTriggerExit { get => lastTriggerExit; set => lastTriggerExit = value; }
    }
}
