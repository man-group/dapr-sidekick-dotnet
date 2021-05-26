using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprServiceCollectionExtensions
    {
        public static IServiceCollection TryAddHostedService<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IHostedService
        {
            // Only add the service if an existing implementation does not exist
            if (!services.HasAssignableService<IHostedService, TImplementation>())
            {
                services.AddHostedService<TImplementation>();
            }

            return services;
        }

        public static bool HasAssignableService<TService, TImplementation>(this IServiceCollection services)
        {
            // Check to see if any existing service of type TImplementation (or a subclass) has been registered.
            foreach (var service in services)
            {
                if (service.ServiceType != typeof(TService))
                {
                    continue;
                }

                if (typeof(TImplementation).IsAssignableFrom(service.ImplementationType))
                {
                    // Already assigned
                    return true;
                }
            }

            return false;
        }
    }
}
