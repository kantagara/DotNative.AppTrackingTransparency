import AdSupport
import AppTrackingTransparency
import UIKit

@MainActor final class TrackingTransparencyPlugin {
    private static var instance: TrackingTransparencyPlugin?
    private var observer: NSObjectProtocol?

    static func register() {
        if instance == nil { instance = TrackingTransparencyPlugin() }
    }

    private init() {
        let channel = NativeChannels.channel("dotnative.tracking-transparency")
        channel.handle("status") { [weak self] _, reply in
            reply.success(.string(self?.current() ?? "notSupported"))
        }
        channel.handle("request") { [weak self] _, reply in
            self?.request(reply) ?? reply.success(.string("notSupported"))
        }
        channel.handle("advertisingIdentifier") { _, reply in
            reply.success(.string(ASIdentifierManager.shared().advertisingIdentifier.uuidString))
        }
    }

    private func current() -> String {
        if #available(iOS 14, *) {
            return Self.name(ATTrackingManager.trackingAuthorizationStatus)
        }
        return "notSupported"
    }

    private func request(_ reply: PluginReply) {
        guard #available(iOS 14, *) else {
            reply.success(.string("notSupported"))
            return
        }
        removeObserver()
        ATTrackingManager.requestTrackingAuthorization { status in
            Task { @MainActor in
                if status == .denied,
                    ATTrackingManager.trackingAuthorizationStatus == .notDetermined
                {
                    self.wait(reply)
                    return
                }
                reply.success(.string(Self.name(status)))
            }
        }
    }

    private func wait(_ reply: PluginReply) {
        removeObserver()
        observer = NotificationCenter.default.addObserver(
            forName: UIApplication.didBecomeActiveNotification,
            object: nil,
            queue: .main
        ) { [weak self] _ in
            Task { @MainActor in self?.request(reply) }
        }
    }

    private func removeObserver() {
        if let observer {
            NotificationCenter.default.removeObserver(observer)
            self.observer = nil
        }
    }

    @available(iOS 14, *)
    private static func name(_ status: ATTrackingManager.AuthorizationStatus) -> String {
        switch status {
        case .notDetermined: return "notDetermined"
        case .restricted: return "restricted"
        case .denied: return "denied"
        case .authorized: return "authorized"
        @unknown default: return "notSupported"
        }
    }
}
