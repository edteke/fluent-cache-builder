# Fluent Cache Builder

A cache builder that wraps MemoryCache with a fluent interface
for easy building cache entries.

## Setup

``` c#
public class Startup
{
	public void ConfigureServices(IServiceCollection services)
    {
		...

		services.AddMemoryCache();
	}
}
```

## Creating Entries

``` c#
	public class Repository<TEntity>
        where TEntity : class, new()
    {
        public CacheBuilder CacheBuilder { get; private set; }

        public Repository(CacheBuilder cacheBuilder)
        {
			// Ask for a cacheBuilder instance to be injected
            CacheBuilder = cacheBuilder;
        }

		public async Task<TEntity> FindByKeyFromCacheAsync(object key)
        {
            return await CacheBuilder
				.ForObjectOfType<TEntity>()
				.WithKey(key.ToString())
                .ExpiresWithin(TimeSpan.FromMinutes(5))
                .BuildValueFromAsync(async () =>
                {
                    return await FindByKeyAsync(key);
                })
                .GetFromCacheAsync();
        }

		public async Task<TEntity> FindByKeyAsync(object key)
		{
			...
		}

	}
```
