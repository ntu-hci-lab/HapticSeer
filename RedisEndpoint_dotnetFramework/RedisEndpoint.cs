using System;
using System.Threading.Tasks;
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
        public Publisher(string url, ushort port) : base(url, port)
        {
            connection = multiplexer.GetSubscriber();
        }
        public void Publish(string channelName, string msg)
        {
            connection.PublishAsync(channelName, msg, flags: CommandFlags.FireAndForget);
        }
    }

    public class Subscriber : RedisEndpoint
    {
        public ChannelMessageQueue msgQueue;
        public Subscriber(string url, ushort port) : base(url, port)
        {
            connection = multiplexer.GetSubscriber();
        }
        public void SubscribeTo(string channelName)
        {
            msgQueue = connection.Subscribe(channelName);
        }
    }


}
