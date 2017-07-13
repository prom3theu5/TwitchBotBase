using System.Runtime.Caching;
using System;
using System.Threading.Tasks;

namespace Prom3theu5.AppCache
{
    public class CachingService : IAppCache
    {
        public CachingService() : this(MemoryCache.Default)
        { }

        public CachingService(ObjectCache cache)
        {
            ObjectCache = cache ?? throw new ArgumentNullException(nameof(cache));
            DefaultCacheDuration = 60 * 20;
        }

        public int DefaultCacheDuration { get; set; }

        private DateTimeOffset DefaultExpiryDateTime => DateTimeOffset.Now.AddSeconds(DefaultCacheDuration);

        public void Add<T>(string key, T item)
        {
            Add(key, item, DefaultExpiryDateTime);
        }

        public void Add<T>(string key, T item, DateTimeOffset expires)
        {
            Add(key, item, new CacheItemPolicy { AbsoluteExpiration = expires });
        }

        public void Add<T>(string key, T item, TimeSpan slidingExpiration)
        {
            Add(key, item, new CacheItemPolicy { SlidingExpiration = slidingExpiration });
        }

        public void Add<T>(string key, T item, CacheItemPolicy policy)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            ValidateKey(key);

            ObjectCache.Set(key, item, policy);
        }

        public T Get<T>(string key)
        {
            ValidateKey(key);

            var item = ObjectCache[key];

            return UnwrapLazy<T>(item);
        }

        private static T UnwrapLazy<T>(object item)
        {
            if (item is Lazy<T> lazy)
                return lazy.Value;

            if (item is T)
                return (T)item;

            return default(T);
        }


        public async Task<T> GetAsync<T>(string key)
        {
            ValidateKey(key);

            var item = ObjectCache[key];

            return await UnwrapAsyncLazys<T>(item);
        }

        private static async Task<T> UnwrapAsyncLazys<T>(object item)
        {
            if (item is AsyncLazy<T> asyncLazy)
                return await asyncLazy.Value;

            if (item is Task<T> task)
                return await task;

            if (item is Lazy<T> lazy)
                return lazy.Value;

            if (item is T)
                return (T)item;

            return default(T);
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory)
        {
            return GetOrAdd(key, addItemFactory, DefaultExpiryDateTime);
        }


        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, CacheItemPolicy policy)
        {
            ValidateKey(key);

            var newLazyCacheItem = new AsyncLazy<T>(addItemFactory);

            EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(policy);

            var existingCacheItem = ObjectCache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                return await UnwrapAsyncLazys<T>(existingCacheItem);
            }

            try
            {
                var result = newLazyCacheItem.Value;

                if (result.IsCanceled || result.IsFaulted)
                    ObjectCache.Remove(key);

                return await result;
            }
            catch
            {
                ObjectCache.Remove(key);
                throw;
            }
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, DateTimeOffset expires)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy { AbsoluteExpiration = expires });
        }


        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, addItemFactory, new CacheItemPolicy { SlidingExpiration = slidingExpiration });
        }

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, CacheItemPolicy policy)
        {
            ValidateKey(key);

            var newLazyCacheItem = new Lazy<T>(addItemFactory);

            EnsureRemovedCallbackDoesNotReturnTheLazy<T>(policy);

            var existingCacheItem = ObjectCache.AddOrGetExisting(key, newLazyCacheItem, policy);

            if (existingCacheItem != null)
            {
                return UnwrapLazy<T>(existingCacheItem);
            }

            try
            {
                return newLazyCacheItem.Value;
            }
            catch
            {
                ObjectCache.Remove(key);
                throw;
            }
        }


        public void Remove(string key)
        {
            ValidateKey(key);
            ObjectCache.Remove(key);
        }

        public ObjectCache ObjectCache { get; }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
        {
            return await GetOrAddAsync(key, addItemFactory, DefaultExpiryDateTime);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy { AbsoluteExpiration = expires });
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
        {
            return await GetOrAddAsync(key, addItemFactory, new CacheItemPolicy { SlidingExpiration = slidingExpiration });
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheLazy<T>(CacheItemPolicy policy)
        {
            if ((policy != null) && (policy.RemovedCallback != null))
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    if ((args != null) && (args.CacheItem != null))
                    {
                        if (args.CacheItem.Value is Lazy<T> item)
                            args.CacheItem.Value = item.IsValueCreated ? item.Value : default(T);
                    }
                    originallCallback(args);
                };
            }
        }

        private static void EnsureRemovedCallbackDoesNotReturnTheAsyncLazy<T>(CacheItemPolicy policy)
        {
            if ((policy != null) && (policy.RemovedCallback != null))
            {
                var originallCallback = policy.RemovedCallback;
                policy.RemovedCallback = args =>
                {
                    if ((args != null) && (args.CacheItem != null))
                    {
                        if (args.CacheItem.Value is AsyncLazy<T> item)
                            args.CacheItem.Value = item.IsValueCreated ? item.Value : Task.FromResult(default(T));
                    }
                    originallCallback(args);
                };
            }
        }

        private void ValidateKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Cache keys cannot be empty or whitespace");
        }
    }
}
