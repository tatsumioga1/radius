# Microsoft Store Submission Notes

Use this as the working text for Partner Center during Microsoft Store submission.

## Restricted Capability

Roundly declares the `runFullTrust` restricted capability.

### Partner Center Justification

Roundly is a packaged WinUI 3 desktop utility that runs as a medium-integrity desktop app. It uses `runFullTrust` so it can create native, click-through, topmost desktop overlay windows at the four physical screen corners and expose a standard Windows notification-area icon/context menu.

The overlays are purely visual masks. They do not capture the screen, read window contents, intercept keyboard input, intercept mouse input, install drivers, install services, elevate privileges, communicate over the network, or modify other applications. Roundly stores only local user preferences, such as enabled state, corner radius, and startup preference, under the user's `%AppData%\Roundly` folder.

The app also uses the packaged desktop startup task extension so users can opt in to opening Roundly on sign-in. Startup is disabled by default and can be toggled from the app settings or tray menu.

Without `runFullTrust`, the MSIX package cannot support the required desktop window, tray icon, and startup behavior for this native Windows utility.

## Capability Inventory

- `runFullTrust`: Required for packaged desktop execution and native shell/window integration.

Roundly intentionally does not declare:

- `internetClient`
- `internetClientServer`
- `privateNetworkClientServer`
- `broadFileSystemAccess`
- `graphicsCapture`
- `graphicsCaptureProgrammatic`
- `webcam`
- `microphone`
- `location`
- contacts, pictures, videos, music, appointments, or removable storage capabilities

## Privacy Summary

Roundly does not collect, transmit, sell, or share personal data. It does not include telemetry, analytics, crash reporting, advertising SDKs, or network calls.

Local files written by the app:

- `%AppData%\Roundly\settings.json`
- `%LocalAppData%\Roundly\logs\roundly.log`

These files remain on the user's device and are used only for app preferences and basic local troubleshooting.

## Store Listing Short Description

Round the visual corners of sharp-edged monitors with a tiny native Windows utility.

## Store Listing Description

Roundly is a lightweight Windows utility that makes sharp physical monitor edges feel more at home with Windows 11. It draws tiny click-through overlays at the four screen corners, stays out of the taskbar, and is controlled from the notification area.

Features:

- Adjustable corner radius
- Click-through overlays that do not block other apps
- System tray settings and exit controls
- Optional open on startup
- No telemetry, no network access, and no background polling

## Certification Notes

- The app should be submitted as packaged MSIX for Microsoft Store distribution.
- The Windows App SDK runtime can be Store-installed for framework-dependent packages.
- Keep startup disabled by default.
- Keep the capability list limited to `runFullTrust`.
- Use `https://roundly.northwindlab.website/privacy.html` as the privacy policy URL after the site route is live.
