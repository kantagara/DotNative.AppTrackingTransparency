# DotNative.AppTrackingTransparency

Asks for the iOS tracking permission and reads the advertising identifier.

```csharp
builder.Services.AddAppTrackingTransparency();
var status = await services.AppTrackingTransparency.RequestAsync();
```

| Platform | Status |
| --- | --- |
| iOS | `ATTrackingManager` and `ASIdentifierManager` |
| Android, macOS, Windows, Linux | `TrackingStatus.NotSupported` |

The iOS app must set `NSUserTrackingUsageDescription` in `Info.plist`.
The dialog appears only when the status is not determined. A second request
returns the stored answer.

If iOS reports denied while the status is still not determined, the plugin
waits until the app becomes active and asks again. That covers the iOS 17.4
prompt that can be dismissed before the system records an answer.

## Service access

Import `DotNative.AppTrackingTransparency` and register
`AddAppTrackingTransparency()` before building the provider.
`services.AppTrackingTransparency` is the C# 14 property on `net10.0`.
`services.AppTrackingTransparency()` is the method on `net9.0`.
