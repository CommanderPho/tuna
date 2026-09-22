# Tuna

<p align="center">
  <img src="assets/Tuna.png" width="128" alt="Tuna chick">
</p>

<p align="center">
  Windows Explorer history, quick access, and desktop pet utility
</p>

Tuna is built with .NET 8 and Avalonia UI. It records recently accessed files and folders, and lets you search and open those paths from a lightweight, borderless floating panel. It also includes an interactive desktop pet and is compatible with the Codex pet format.

## Features

- **Explorer history**: Automatically records recently accessed files and folders, with search, filters, pinning, and quick open.
- **Quick-access panel**: The panel can appear near the mouse, auto-hide on focus loss, stay on top, and wake a single running instance.
- **Resizable window**: Resize from any edge or corner; size is saved and restored on the next launch.
- **Desktop pet**: Drag, facing direction, hover animations, random actions, and a right-click context menu.
- **Codex pet compatibility**: Import compatible `.codex-pet` packs and show the pet name and full description.
- **Lightweight runtime**: NativeAOT single-file publish; pet animations run on demand to keep background usage low.
- **Native Windows interaction**: Uses Win32 APIs for activation, topmost, resizing, and quick wake behavior.

## Built-in pets

Four built-in pets are included:

| Pet | Description |
| --- | --- |
| Tuna chick | Default Tuna pet — an original yellow chick designed for the app; cannot be deleted |
| Doro | Built-in pet; can be deleted |
| GUGUGAGA | Built-in pet; can be deleted |
| ikkun | Built-in pet; can be deleted |

The pet context menu offers Hide pet, Settings, and Exit. Hover a pet note to see the full description.

## Codex pet resources

- [Codex Pets: create a Codex pet](https://codex-pet.org/zh/how-to-create-a-codex-pet/)
- [Codex Pet: pet library](https://codexpet.xyz/pets/)

## Data location

User settings, window size, and history are stored at:

```text
%APPDATA%\Tuna\history_config.json
```

Built-in pet assets live under:

```text
ExplorerHistoryTracker/Assets/Pets/
```

## Development environment

- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Avalonia UI

## Build

From the repository root:

```powershell
dotnet build .\ExplorerHistoryTracker\ExplorerHistoryTracker.csproj -c Release
```

You can also run `build.bat` at the repo root. The NativeAOT publish script is:

```text
ExplorerHistoryTracker/publish.ps1
```

## Project structure

```text
ExplorerHistoryTracker/      Application source
  Assets/Pets/               Built-in desktop pets
  Models/                    Data models
  Services/                  History, icons, and pet services
  ViewModels/                UI logic
assets/                      Project icons
publish-output/              Local publish output (not committed)
```

Issues and pull requests are welcome.
