namespace Prom3theu5.RedisObjectCache
{
    internal class RedisCacheKey
    {
        internal int Hash { get; set; }
        internal string Key { get; set; }
        internal string StateKey { get; set; }

        internal RedisCacheKey(string key)
        {    
            Key = key;
            StateKey = $"{Key}_STATE";
            Hash = key.GetHashCode();
        }    
    }
}
