using Microsoft.Extensions.DependencyInjection;

namespace Ontec.Core.Cache
{
    public class CacheDepedenciesConfig
    {
        public static void  ConfigureDependencies(IServiceCollection services)
        {
            _=services.AddMemoryCache();

            _=services.AddTransient<ICacheService, CacheService>();
        }
    }
}