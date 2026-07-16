# Roundly

Roundly is a native Windows utility built with WinUI 3 and Windows App SDK. It lives in the notification area and draws four tiny click-through, topmost overlays at each monitor corner so sharp physical display edges visually match Windows' rounded design language.

## Run

Open `Roundly.sln` in Visual Studio and run or deploy the `Roundly` project for `x64`.

You can also build from a Developer PowerShell:

```powershell
dotnet build -c Release
```

## Behavior

- The settings window uses WinUI controls.
- The project uses MSIX tooling so Visual Studio can install/deploy it during testing.
- The taskbar remains clean; the app is controlled from the notification area.
- Corner overlays are click-through and do not activate, resize, move, or clip other app windows.
- Roundly and enabled state are saved under `%AppData%\Roundly\settings.json`.
- The overlay updates on startup, settings changes, and foreground app changes.

## Website

The promotional site is hosted at `https://roundly.northwindlab.website`.

## Microsoft Store

Store submission notes, including the `runFullTrust` restricted-capability justification, live in `docs/microsoft-store-submission.md`.
