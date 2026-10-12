using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.AppTrackingTransparency;

public static class AppTrackingTransparencyServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public IAppTrackingTransparency AppTrackingTransparency =>
            services.GetRequiredService<IAppTrackingTransparency>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static IAppTrackingTransparency AppTrackingTransparency(this IServiceProvider services) =>
        services.GetRequiredService<IAppTrackingTransparency>();
#endif
}
