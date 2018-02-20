using InteractionsIQ.Commons.Cache;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Configuration
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
