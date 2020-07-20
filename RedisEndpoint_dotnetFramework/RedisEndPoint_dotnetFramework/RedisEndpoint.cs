using System;
using StackExchange.Redis;

namespace RedisEndpoint 
{ 

    public class RedisEndpoint
    {
        
        protected static ConfigurationOptions redisConfiguration = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Password = "password",
            Ssl = false,
            ConnectTimeout = 6000,
            SyncTimeout = 6000
        };
        protected static ConnectionMultiplexer multiplexer;
        protected ISubscriber connection;

        public RedisEndpoint(string url, ushort port)
        {
            if (multiplexer == null)
            {
                redisConfiguration.EndPoints.Add(url, port);
                multiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            }
        }

        static int Main()
        {
            return 0;
        }
    }

    public class Publisher : RedisEndpoint
    {
        public Publisher(string url, ushort port) : base(url, port) {
            connection = multiplexer.GetSubscriber();
        }
        public void Publish(string channelName, string msg)
        {
            connection.PublishAsync(channelName, msg, flags: CommandFlags.FireAndForget);
        }
    }

    public class Subsciber : RedisEndpoint
    {
        private ChannelMessageQueue msgQueue;
        public Subsciber(string url, ushort port) : base(url, port) {
            connection = multiplexer.GetSubscriber();
        }
        public void SubscibeTo(string channelName)
        {
            msgQueue = connection.Subscribe(channelName);
        }
        public void SetHandler(Action<ChannelMessage> handler)
        {
            msgQueue.OnMessage(handler);
        }
    }

   
}
