using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public class StateObject
    {
        private byte triggerState = 0;
        private ushort bulletCount = 0;

        public byte TriggerState { get => triggerState; set => triggerState = value; }
        public ushort BulletCount { get => bulletCount; set => bulletCount = value; }
    }
}
