using System;
using System.Collections.Generic;
using RedisEndpoint;
using StackExchange.Redis;
namespace EventExtractors
{
    public class PooledSubscriber
    {

        protected static List<Subscriber> subscribers;
        protected int? subscriberID;
        public static int SubsciberCount => subscribers.Count;

        public PooledSubscriber(string url, ushort port)
        {
            if (subscribers == null) subscribers = new List<Subscriber>(); 
            subscribers.Add(new Subscriber(url, port));
            subscriberID = SubsciberCount-1;
        }
        public void TrySubscribeTo(string channelName, out bool success)
        {
            success = (subscriberID == null ? false : true);
            if (success == true) subscribers[subscriberID.Value].SubscribeTo(channelName);
        }
        public ChannelMessageQueue GetMsqInstance() => subscribers[subscriberID.Value].msgQueue;

    }
}
