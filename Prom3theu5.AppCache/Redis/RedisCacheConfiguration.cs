namespace Prom3theu5.RedisObjectCache
{
    public sealed class RedisCacheConfiguration
    {
        public static RedisCacheConfiguration Instance
        {
            get { return new RedisCacheConfiguration(); }
        }

        public RedisConnectionConfiguration Connection
        {
            get { return new RedisConnectionConfiguration(); }
        }
    }

    public class RedisConnectionConfiguration
    {
        public string Host { get { return "127.0.0.1"; } }
        public int Port { get { return 6379; } }
        public string AccessKey { get { return ""; } }
        public bool Ssl { get { return false; } }
        public int DatabaseId { get { return 0; } }
        public int ConnectionTimeoutInMilliseconds { get { return 5000; } }
        public int OperationTimeoutInMilliseconds { get { return 5000; } }
    }
}
