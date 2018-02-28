using InteractionsIQ.Commons.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Adds a cache builder using MemoryCache
        /// </summary>
        public static void AddCacheBuilder(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddTransient<CacheBuilder>();
        }
    }
}
