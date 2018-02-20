using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InteractionsIQ.Commons.Cache
{
    /// <summary>
    /// Provides a fluent nice way to build cache entries for the specified provider.
    /// </summary>
    public class CacheBuilderSettings<T>
        where T : class
    {
        private readonly IMemoryCache _cache;

        private Func<Task<T>> _valueBuilderAsync;
        private Func<T> _valueBuilder;
        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;
        private Action _cacheMiss;
        private bool _useHashedkeys;
        private string _key;
        private bool _allowNullValuesInCache;

        internal CacheBuilderSettings(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Indicates if null values from the builder action should be added to the cache.
        /// </summary>
        public CacheBuilderSettings<T> AllowNullValuesInCache(bool value = true)
        {
            _allowNullValuesInCache = value;
            return this;
        }

        /// <summary>
        /// Controls whether cache keys will be used as provided or hashed
        /// </summary>
        public CacheBuilderSettings<T> UseHashedKeys(bool value = true)
        {
            _useHashedkeys = value;
            return this;
        }

        /// <summary>
        /// Specifies the cache key. Scoped to the object type.
        /// </summary>
        public CacheBuilderSettings<T> WithKey(string key)
        {
            if (_useHashedkeys)
            {
                _key = Hash($"{typeof(T).FullName}{key}");
            }
            else
            {
                _key = $"{typeof(T).FullName}{key}";
            }


            return this;
        }

        /// <summary>
        /// Specifies the cache key scoped to the object type. Keys will be concatenated. 
        /// Order matters
        /// </summary>
        public CacheBuilderSettings<T> WithKey(params string[] keys)
        {
            return WithKey(string.Concat(keys));
        }

        private string Hash(string text)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashRaw = md5.ComputeHash(Encoding.ASCII.GetBytes(text));

                StringBuilder sb = new StringBuilder(hashRaw.Length * 2);

                for (int i = 0; i < hashRaw.Length; i++)
                {
                    sb.Append(hashRaw[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Specifies a sliding expiration for the cache item
        /// </summary>
        public CacheBuilderSettings<T> ExpiresWithin(TimeSpan slidingExpiration)
        {
            _slidingExpiration = slidingExpiration;
            return this;
        }

        /// <summary>
        /// Specifies an absolute expiration for the cache item
        /// </summary>
        public CacheBuilderSettings<T> ExpiresAt(DateTime absoluteExpiration)
        {
            _absoluteExpiration = absoluteExpiration;
            return this;
        }

        /// <summary>
        /// Specifies a value builder function to build the value when it's needed
        /// </summary>
        public CacheBuilderSettings<T> BuildValueFrom(Func<T> valueBuilder)
        {
            _valueBuilder = valueBuilder;
            return this;
        }

        /// <summary>
        /// Specifies a value builder async function to build the value when it's needed
        /// </summary>
        public CacheBuilderSettings<T> BuildValueFromAsync(Func<Task<T>> valueBuilderAsync)
        {
            _valueBuilderAsync = valueBuilderAsync;
            return this;
        }

        /// <summary>
        /// Specifies a method to call when a cache is missed
        /// </summary>
        public CacheBuilderSettings<T> CacheMiss(Action missAction)
        {
            _cacheMiss = missAction;
            return this;
        }

        /// <summary>
        /// Determines if the key is in the cache.
        /// </summary>
        public bool IsInCache()
        {
            object val;
            return _cache.TryGetValue(_key, out val);
        }

        private void EnsureKey()
        {
            if (string.IsNullOrEmpty(_key))
            {
                throw new InvalidOperationException("Cache key must be specified");
            }
        }

        /// <summary>
        /// Invalidates the cache entry, and removes the item from the cache
        /// </summary>
        public void InvalidateCache()
        {
            if (IsInCache())
            {
                _cache.Remove(_key);
            }
        }

        /// <summary>
        /// Gets the object from the cache
        /// </summary>
        public T GetFromCache()
        {
            EnsureKey();

            T obj;

            if (_cache.TryGetValue(_key, out obj))
            {
                return obj;
            }
            else
            {
                _cacheMiss?.Invoke();

                obj = _valueBuilder();

                if (!_allowNullValuesInCache && obj == null)
                {
                    return null;
                }

                var e = new MemoryCacheEntryOptions();

                if (_absoluteExpiration.HasValue)
                {
                    e.AbsoluteExpiration = _absoluteExpiration;
                }

                if (_slidingExpiration.HasValue)
                {
                    e.SlidingExpiration = _slidingExpiration;
                }

                _cache.Set(_key, obj, e);

                return obj;
            }
        }

        /// <summary>
        /// Gets the object from the cache
        /// </summary>
        public async Task<T> GetFromCacheAsync()
        {
            EnsureKey();

            T obj;

            if (_cache.TryGetValue(_key, out obj))
            {
                return obj;
            }
            else
            {
                _cacheMiss?.Invoke();

                obj = await _valueBuilderAsync();

                if (!_allowNullValuesInCache && obj == null)
                {
                    return null;
                }

                var e = new MemoryCacheEntryOptions();

                if (_absoluteExpiration.HasValue)
                {
                    e.AbsoluteExpiration = _absoluteExpiration;
                }

                if (_slidingExpiration.HasValue)
                {
                    e.SlidingExpiration = _slidingExpiration;
                }

                _cache.Set(_key, obj, e);


                return obj;
            }
        }
    }
}
