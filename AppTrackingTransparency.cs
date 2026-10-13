using DotNative;
using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.AppTrackingTransparency;

// ─────────────────────────────────────────────────────────────
// Public API
// ─────────────────────────────────────────────────────────────

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
    Task<TrackingStatus> StatusAsync(
        CancellationToken cancellationToken = default);

    Task<TrackingStatus> RequestAsync(
        CancellationToken cancellationToken = default);

    Task<string> AdvertisingIdentifierAsync(
        CancellationToken cancellationToken = default);
}

// ─────────────────────────────────────────────────────────────
// Dependency Injection
// ─────────────────────────────────────────────────────────────

public static class AppTrackingTransparencyServices
{
    public static IServiceCollection AddAppTrackingTransparency(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IAppTrackingTransparency, AppTrackingTransparencyService>();
        return services;
    }
}

// ─────────────────────────────────────────────────────────────
// Implementation
// ─────────────────────────────────────────────────────────────

internal sealed class AppTrackingTransparencyService(
    IPlatformChannels channels,
    PresentationTarget target
) : IAppTrackingTransparency
{
    private const string ChannelName = "dotnative.tracking-transparency";
    private const string UnavailableIdentifier = "00000000-0000-0000-0000-000000000000";

    private bool IsiOS => target.Platform == NativePlatform.IOS;

    private Task<object?> InvokeAsync(
        string method,
        CancellationToken cancellationToken)
    {
        return channels.Get(ChannelName)
            .InvokeAsync(method, null, cancellationToken);
    }

    public async Task<TrackingStatus> StatusAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsiOS)
            return TrackingStatus.NotSupported;

        return Parse(await InvokeAsync("status", cancellationToken).ConfigureAwait(false));
    }

    public async Task<TrackingStatus> RequestAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsiOS)
            return TrackingStatus.NotSupported;

        return Parse(await InvokeAsync("request", cancellationToken).ConfigureAwait(false));
    }

    public async Task<string> AdvertisingIdentifierAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsiOS)
            return UnavailableIdentifier;

        var value = await InvokeAsync("advertisingIdentifier", cancellationToken)
            .ConfigureAwait(false);

        return value is string identifier &&
               identifier.Length is > 0 and <= 64
            ? identifier
            : throw new InvalidDataException("Invalid advertising identifier.");
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
