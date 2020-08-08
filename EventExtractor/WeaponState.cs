using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    public class WeaponState
    {
        private bool triggerState = false;
        private ushort bulletCount = 0;
        
        public Publisher publisher;
        public bool TriggerState { get => triggerState; set => triggerState = value; }
        public ushort BulletCount { get => bulletCount; set => bulletCount = value; }

        public WeaponState(Publisher publisher) => this.publisher = publisher;

    }
}
