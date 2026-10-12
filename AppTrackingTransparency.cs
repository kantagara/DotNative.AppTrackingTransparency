using DotNative;
using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.AppTrackingTransparency;

public enum TrackingStatus
{
    NotDetermined,
    Restricted,
    Denied,
    Authorized,
    NotSupported,
}

public interface IAppTrackingTransparency
{
    Task<TrackingStatus> StatusAsync(CancellationToken cancellationToken = default);

    Task<TrackingStatus> RequestAsync(CancellationToken cancellationToken = default);

    Task<string> AdvertisingIdentifierAsync(CancellationToken cancellationToken = default);
}

public static class AppTrackingTransparencyServices
{
    public static IServiceCollection AddAppTrackingTransparency(this IServiceCollection services)
    {
        services.TryAddSingleton<IAppTrackingTransparency, RoutingTrackingTransparency>();
        return services;
    }
}

internal sealed class RoutingTrackingTransparency(
    IPlatformChannels channels,
    PresentationTarget target
) : IAppTrackingTransparency
{
    public Task<TrackingStatus> StatusAsync(CancellationToken cancellationToken = default) =>
        target.Platform == NativePlatform.IOS
            ? new ChannelTrackingTransparency(channels).StatusAsync(cancellationToken)
            : Task.FromResult(TrackingStatus.NotSupported);

    public Task<TrackingStatus> RequestAsync(CancellationToken cancellationToken = default) =>
        target.Platform == NativePlatform.IOS
            ? new ChannelTrackingTransparency(channels).RequestAsync(cancellationToken)
            : Task.FromResult(TrackingStatus.NotSupported);

    public Task<string> AdvertisingIdentifierAsync(CancellationToken cancellationToken = default) =>
        target.Platform == NativePlatform.IOS
            ? new ChannelTrackingTransparency(channels).AdvertisingIdentifierAsync(cancellationToken)
            : Task.FromResult("00000000-0000-0000-0000-000000000000");
}

internal sealed class ChannelTrackingTransparency(IPlatformChannels channels)
{
    public async Task<TrackingStatus> StatusAsync(CancellationToken cancellationToken) =>
        Parse(
            await channels
                .Get("dotnative.tracking-transparency")
                .InvokeAsync("status", null, cancellationToken)
                .ConfigureAwait(false)
        );

    public async Task<TrackingStatus> RequestAsync(CancellationToken cancellationToken) =>
        Parse(
            await channels
                .Get("dotnative.tracking-transparency")
                .InvokeAsync("request", null, cancellationToken)
                .ConfigureAwait(false)
        );

    public async Task<string> AdvertisingIdentifierAsync(CancellationToken cancellationToken)
    {
        var value = await channels
            .Get("dotnative.tracking-transparency")
            .InvokeAsync("advertisingIdentifier", null, cancellationToken)
            .ConfigureAwait(false);
        if (value is not string identifier || identifier.Length is 0 or > 64)
            throw new InvalidDataException("Invalid advertising identifier.");
        return identifier;
    }

    private static TrackingStatus Parse(object? value) =>
        value switch
        {
            "notDetermined" => TrackingStatus.NotDetermined,
            "restricted" => TrackingStatus.Restricted,
            "denied" => TrackingStatus.Denied,
            "authorized" => TrackingStatus.Authorized,
            "notSupported" => TrackingStatus.NotSupported,
            _ => throw new InvalidDataException("Invalid tracking status."),
        };
}
