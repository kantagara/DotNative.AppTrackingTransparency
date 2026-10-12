# Source origin

This package is an independently authored DotNative plugin.
It calls `ATTrackingManager` and `ASIdentifierManager` directly.
Status names follow Apple's authorization values. The retry after a premature
denial follows the same iOS 17.4 behavior handled by the Flutter
`app_tracking_transparency` plugin.
