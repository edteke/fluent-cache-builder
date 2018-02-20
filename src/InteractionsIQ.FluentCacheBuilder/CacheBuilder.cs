using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractionsIQ.Commons.Cache
{
    /// <summary>
    /// Builds cache in a nice way
    /// </summary>
    public class CacheBuilder
    {
        private IMemoryCache _cache;

        public CacheBuilder(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Returns a cache builder for a particular provider
        /// to build cache entries in a nice way
        /// </summary>
        public CacheBuilderSettings<T> ForObjectOfType<T>() where T : class
        {
            return new CacheBuilderSettings<T>(_cache);
        }
    }
}
