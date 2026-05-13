# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-05-12

### Added
- `ISettingsStorage` interface + `PlayerPrefsStorage` default implementation. Settings modules no longer call PlayerPrefs directly.
- `ResolutionSettingsModule` — auto-populates dropdown from `Screen.resolutions` with duplicate filtering.
- `QualitySettingsModule` — auto-populates dropdown from `QualitySettings.names`.
- `PauseController`: optional `PlayerInput` action map switching (Player ↔ UI) on pause/resume.
- `PauseController`: optional cursor lock/visibility management (`_manageCursor`).
- `SettingsController`: `SetStorage()` method for runtime storage replacement.
- `PlayerPrefsKeys`: added `Resolution` and `Quality` constants.

### Changed
- **BREAKING:** `ISettingsModule.Initialize()` now receives `ISettingsStorage` parameter. Custom modules must update their signature.
- `AudioSettingsModule`: refactored to use `ISettingsStorage` instead of direct `PlayerPrefs` calls.
- `DisplaySettingsModule`: refactored to use `ISettingsStorage` instead of direct `PlayerPrefs` calls.
- `SettingsController`: now owns and injects `ISettingsStorage` into all modules.

## [1.1.0] - 2026-05-12

### Added
- `SplashSequence`: skip support — any key/click/tap skips current panel or entire sequence (configurable).
- `MenuController`: navigation stack — `Show()` pushes, `Back()` pops. No more hardcoding back targets.
- `MenuController`: auto-select first `Selectable` in panel for gamepad/keyboard navigation.
- `MenuController`: `OnQuitRequested` static event for WebGL quit handling.
- `SceneLoader`: configurable minimum loading duration to prevent loading screen flashing.
- `ConfirmationDialog`: reusable modal popup with title, message, confirm/cancel callbacks.

### Changed
- `MenuController.QuitGame()`: now platform-aware — handles WebGL (fires event) and Editor (stops play mode) explicitly.
- `MenuController.Back(GameObject)`: now clears navigation history (acts as "go home").
- `SceneLoader`: scene activation is always held until both load completion AND minimum time are satisfied.

## [1.0.0] - 2026-05-12

### Changed
- Renamed namespace from `AbyssalTides.Menu` to `AnluMenu` for cross-project reusability.
- Renamed assemblies: `AbyssalTides.Menu.Core` → `AnluMenu.Core`, `AbyssalTides.Menu.Netcode` → `AnluMenu.Netcode`.
- Renamed define constraint: `ABYSSAL_MENU_USE_NETCODE` → `ANLU_MENU_USE_NETCODE`.

### Added
- `SettingsOverlay.cs` moved into the package (`Runtime/Core/Settings/`). Previously lived in project-specific game scripts.

### Fixed
- README folder structure now matches actual file layout.
