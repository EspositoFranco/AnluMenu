# AnluMenu Package

> Author: **Franco Esposito**

A reusable, opinionated menu system for Unity. Ships with:

- **MenuController** — local panel switching with navigation stack and gamepad auto-select.
- **MenuEvents** — static event bus for cross-cutting menu signals (open settings, pause, etc).
- **MenuActions** + **MenuActionButton** — reusable menu actions (Quit, LoadScene, Restart, Open/Close Settings, Pause) callable from any scene without per-button glue code.
- **TabController** — generic tab/panel switcher reusable anywhere (settings, inventory, in-game HUDs).
- **PauseController** — pause overlay, mode-aware (freezes time only in single-player), listens to `MenuEvents`.
- **SettingsController** — owns the settings panel (Show/Hide/Toggle), auto-discovers `ISettingsModule` children, extensible storage via `ISettingsStorage`.
- **SceneLoader** — async scene load with fade + progress bar + minimum display time.
- **SplashSequence** — multi-panel fade-in/out splash for studio logos, skippable.
- **ConfirmationDialog** + **ConfirmAction** — reusable modal popup, plus a drop-in component to prepend confirmations to any button.
- **IUIAudio** abstraction so the package never references a concrete audio system.
- Optional **Netcode for GameObjects** integration via a separate assembly that
  auto-loads when NGO is installed.

The package is **deliberately small**. It does the wiring; you provide the UI.

---

## Installation

The package folder is `Anlu.menu/` (renamed from `com.anlu.menu/` — Unity uses the
`name` field in `package.json`, so the folder name doesn't matter to Unity).

### Option A — Local file reference (recommended while iterating)

In your consumer project, edit `Packages/manifest.json` and add a line under
`dependencies`:

```json
{
  "dependencies": {
    "com.anlu.menu": "file:../../Anlu Packages/Anlu.menu",
    "...": "..."
  }
}
```

Adjust the path so it points from your project's `Packages/` folder to the
`Anlu.menu/` folder. Unity will resolve the package on next focus.

### Option B — Embed inside your project

Copy the entire `Anlu.menu/` folder into your project's `Packages/` directory:

```
YourGame/
└── Packages/
    └── Anlu.menu/
        ├── package.json
        └── Runtime/
```

Unity treats anything under `Packages/` with a `package.json` as an embedded
package. No `manifest.json` edit needed.

### Option C — Git URL (when you push the repo)

```json
"com.anlu.menu": "https://github.com/EspositoFranco/Anlu.menu.git"
```

### Dependencies

The package declares `com.unity.textmeshpro` and `com.unity.inputsystem`. Unity
resolves them automatically. The Netcode integration compiles only if
`com.unity.netcode.gameobjects` is installed (zero work on your side).

---

## Folder structure

```
Anlu.menu/
├── package.json                              ← UPM manifest (com.anlu.menu)
├── README.md
├── AUTHORS.md
├── CHANGELOG.md
└── Runtime/
    ├── Core/                                 ← Menu.Core asmdef (no NGO dependency)
    │   ├── GameMode.cs
    │   ├── ScreenFader.cs
    │   ├── SceneLoader.cs
    │   ├── MenuController.cs
    │   ├── PauseController.cs
    │   ├── SplashSequence.cs
    │   ├── ConfirmationDialog.cs
    │   ├── Actions/
    │   │   ├── MenuActions.cs
    │   │   └── MenuActionButton.cs
    │   ├── Events/
    │   │   └── MenuEvents.cs
    │   ├── UI/
    │   │   ├── TabController.cs
    │   │   └── ConfirmAction.cs
    │   ├── Audio/
    │   │   ├── IUIAudio.cs
    │   │   └── NullUIAudio.cs
    │   └── Settings/
    │       ├── PlayerPrefsKeys.cs
    │       ├── PlayerPrefsStorage.cs
    │       ├── ISettingsModule.cs
    │       ├── ISettingsStorage.cs
    │       ├── SettingsController.cs
    │       └── Modules/
    │           ├── AudioSettingsModule.cs
    │           ├── DisplaySettingsModule.cs
    │           ├── QualitySettingsModule.cs
    │           └── ResolutionSettingsModule.cs
    └── Netcode/                              ← Menu.Netcode asmdef (NGO-only)
        ├── NetcodeSceneLoadStrategy.cs
        └── NetcodeBootstrap.cs
```

### Two assemblies, one switch

`Menu.Core` has **zero** dependencies on Netcode for GameObjects. Drop it into
any Unity project (multiplayer or not) and it compiles.

`Menu.Netcode` declares a `versionDefines` entry on
`com.unity.netcode.gameobjects` that emits the symbol `ANLU_MENU_USE_NETCODE`,
also used as its `defineConstraints` gate. Translation: **the Netcode assembly
only compiles when NGO is installed**. Uninstall NGO → the assembly drops out
of the build, and `SceneLoader` falls back to the local strategy with zero
code changes.

---

## Architecture overview (v3)

Two communication patterns are used **on purpose**, and choosing the right one
matters:

| Pattern               | Use for                                            | Examples                             |
|-----------------------|----------------------------------------------------|--------------------------------------|
| **Direct references** | Local navigation inside one menu hierarchy         | `MenuController.Show(panel)`, `TabController.ShowTab(i)` |
| **`MenuEvents` bus**  | Cross-cutting requests (sender doesn't know receiver) | "Open settings from anywhere", "Pause requested from gameplay" |

**Rule of thumb:** direct ref when A and B live together in the same hierarchy
and the relation is obvious. Event when A shouldn't have to know B exists.

### `MenuActions` (static) vs `MenuActionButton` (component)

- `MenuActions.Quit()`, `MenuActions.LoadScene(name)`, `MenuActions.RestartCurrentScene()` — call from code.
- `MenuActionButton` — drop on a button, pick the action from a dropdown, wire the button's OnClick to `Trigger()`. Inspector-driven, no per-button code.

`MenuActionButton` also handles event-only actions (Open/Close/Toggle Settings,
Pause/Resume/TogglePause) by raising the corresponding `MenuEvents` request —
so any controller that's listening reacts without coupling.

---

## Quick start (5 minutes)

1. **MainMenu scene** — create a Canvas with panels: **Home**, **Play**, **Exit**.
2. On the Canvas, add `MenuController`. Set **Initial Panel** to Home and drag every panel into **All Panels**.
3. On a separate root (e.g. `_Loaders`), add `SceneLoader`. Wire **Loading Screen**, **Progress Bar**, and optionally a **Fader**.
4. Add a **Settings panel** under the Canvas. On the Canvas root (or any always-active root), add `SettingsController` and wire its **Panel** field to the Settings panel GameObject.
5. Under the Settings panel, drop your module components (`AudioSettingsModule`, `DisplaySettingsModule`, etc.) — `SettingsController` finds them automatically.
6. Add a `ConfirmationDialog` somewhere in the scene if you want confirmation popups (Quit, Reset settings, etc.).
7. **Wire buttons** (see next section).
8. Hit Play.

The menu is silent by design until you provide an `IUIAudio` adapter (see [Audio integration](#audio-integration)).

---

## Wiring buttons

Buttons connect via the standard **Unity OnClick UnityEvent**. The package
splits responsibilities so you never have to duplicate logic per scene:

### Local navigation (MainMenu only)

| Button             | Target component        | Method                | Parameter          |
|--------------------|-------------------------|-----------------------|--------------------|
| Go to Play panel   | MenuController          | `Show(GameObject)`    | Play panel ref     |
| Back (stack)       | MenuController          | `Back()`              | —                  |
| Back (explicit)    | MenuController          | `Back(GameObject)`    | Home panel ref     |

### Reusable actions (any scene — MainMenu, Pause, anywhere)

Drop a `MenuActionButton` on the button, pick the **Action** from the dropdown,
wire OnClick → `MenuActionButton.Trigger`:

| Action                   | What it does                                              | `Scene Name` field |
|--------------------------|-----------------------------------------------------------|--------------------|
| `Quit`                   | Application.Quit (handles Editor/WebGL)                   | —                  |
| `LoadScene`              | Async load via `SceneLoader`                              | required           |
| `RestartCurrentScene`    | Reloads the active scene                                  | —                  |
| `OpenSettings`           | Raises `MenuEvents.OnOpenSettingsRequested`               | —                  |
| `CloseSettings`          | Raises `MenuEvents.OnCloseSettingsRequested`              | —                  |
| `ToggleSettings`         | Raises `MenuEvents.OnToggleSettingsRequested`             | —                  |
| `Pause`                  | Raises `MenuEvents.OnPauseRequested`                      | —                  |
| `Resume`                 | Raises `MenuEvents.OnResumeRequested`                     | —                  |
| `TogglePause`            | Raises `MenuEvents.OnTogglePauseRequested`                | —                  |

The Settings and Pause buttons work in **any** scene — they raise events that
`SettingsController` and `PauseController` listen to. No need for a controller
reference, no per-scene singleton.

### Settings panel buttons

| Button             | Target component        | Method                |
|--------------------|-------------------------|-----------------------|
| Apply              | SettingsController      | `ApplyAll()`          |
| Reset to defaults  | SettingsController      | `ResetAll()`          |
| Close              | SettingsController      | `Hide()` (or use a `MenuActionButton` with `CloseSettings`) |

---

## Confirmation popups before destructive actions

Use `ConfirmAction` to prepend a "are you sure?" dialog without writing per-button code.

### Setup

1. Make sure a `ConfirmationDialog` exists in the scene (with its UI wired).
2. On the button (e.g. Quit, Restart, Back-to-MainMenu), add **two** components:
   - A `ConfirmAction` configured with title, message, confirm/cancel labels.
   - A `MenuActionButton` configured with the real action (e.g. `Quit`).
3. Wire the button's **OnClick → `ConfirmAction.Trigger`**.
4. On the `ConfirmAction`, wire **OnConfirmed → `MenuActionButton.Trigger`**.

The flow: click → dialog opens → confirm → action runs. Cancel closes the dialog
silently (or you can wire something to `OnCancelled` too).

If no `ConfirmationDialog.Instance` exists in the scene, `ConfirmAction` falls
back to invoking `OnConfirmed` directly — the button still works, just without
the confirmation.

---

## Pause menu with reusable actions

The pause menu is no longer coupled to `MenuController`. Build it like this:

1. Add `PauseController` to a root GameObject in gameplay scenes. Wire **Pause Menu** to the pause panel and pick a toggle key (default Escape).
2. Add the pause panel with your buttons: Resume, Restart Level, Settings, Quit.
3. Wire each button via `MenuActionButton` + optional `ConfirmAction`:

| Button              | Action                                  | Confirm? |
|---------------------|-----------------------------------------|----------|
| Resume              | `Resume`                                | no       |
| Open Settings       | `OpenSettings`                          | no       |
| Restart Level       | `RestartCurrentScene` or `LoadScene` with first-level scene name | yes (recommended) |
| Quit to Main Menu   | `LoadScene` with `"MainMenu"`           | yes      |
| Quit Game           | `Quit`                                  | yes      |

The Settings panel can live in the gameplay scene too — `SettingsController`
listens to `MenuEvents.OnOpenSettingsRequested` and shows itself when the event fires,
regardless of which scene's button triggered it.

### Restart: current scene vs first level

This is per-button, configurable:

- **Restart current scene** → `Action = RestartCurrentScene`.
- **Restart from first level** → `Action = LoadScene`, `Scene Name = "Level_01"` (or whatever your first level is named).

Want both buttons? Add two `MenuActionButton` components on separate buttons.

---

## Tabs (for Settings or anywhere)

`TabController` is a **generic tab switcher**, not tied to settings. Use it for
settings panels, in-game inventory tabs, lobby screens, anything.

### Setup

1. Create a panel that contains tab buttons (Audio, Display, Quality, ...) and tab content panels.
2. Add `TabController` to that panel.
3. Fill the **Tabs** list — for each tab:
   - **Button** → the tab's button.
   - **Panel** → the panel shown when the tab is active.
   - **Active Indicator** (optional) → a graphic (underline, highlight) shown only on the active tab.
4. Set **Initial Index** to the tab visible on startup.

`SettingsController` recursively scans its `_panel` subtree for modules, so each
tab-panel can hold its own modules and everything just works:

```
SettingsPanel
├── TabBar (TabController here)
│   ├── AudioTabButton
│   ├── DisplayTabButton
│   └── QualityTabButton
├── AudioTabPanel
│   └── AudioSettingsModule
├── DisplayTabPanel
│   └── DisplaySettingsModule
└── QualityTabPanel
    └── QualitySettingsModule
```

---

## Adding a new Settings module

To add (for example) a controls remapping module:

```csharp
using AnluMenu;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettingsModule : MonoBehaviour, ISettingsModule
{
    private const string Key = "menu.controls.invert_y";

    [SerializeField] private Toggle _invertYToggle;
    private ISettingsStorage _storage;

    public void Initialize(ISettingsStorage storage)
    {
        _storage = storage;
        _invertYToggle.SetIsOnWithoutNotify(_storage.GetInt(Key, 0) == 1);
        _invertYToggle.onValueChanged.AddListener(SetInvertY);
    }

    public void Apply() => SetInvertY(_invertYToggle.isOn);

    public void ResetToDefaults()
    {
        _invertYToggle.isOn = false;
        _storage.SetInt(Key, 0);
    }

    private void SetInvertY(bool value) => _storage.SetInt(Key, value ? 1 : 0);
}
```

Drop it anywhere under the Settings panel. `SettingsController` picks it up on
Awake — no manual registration. Add a constant to `PlayerPrefsKeys.cs` to keep
keys centralized.

---

## Audio integration

The menu package **never references a concrete audio system**. You bridge it via
`IUIAudio`. The default is `NullUIAudio` (silent).

### Example: MMSoundManager adapter (when you install Feel)

```csharp
using AnluMenu;
using MoreMountains.Tools;
using UnityEngine;

public class MMSoundManagerUIAudio : MonoBehaviour, IUIAudio
{
    [SerializeField] private AudioClip _hover, _click, _slider;

    public void PlayHover()  => Play(_hover);
    public void PlayClick()  => Play(_click);
    public void PlaySlider() => Play(_slider);

    public void SetVolume(VolumeChannel channel, float linear)
    {
        var track = channel switch
        {
            VolumeChannel.Music    => MMSoundManager.MMSoundManagerTracks.Music,
            VolumeChannel.Sfx      => MMSoundManager.MMSoundManagerTracks.Sfx,
            VolumeChannel.Ambience => MMSoundManager.MMSoundManagerTracks.Sfx,
            VolumeChannel.Dialogue => MMSoundManager.MMSoundManagerTracks.UI,
            _                      => MMSoundManager.MMSoundManagerTracks.Master
        };
        MMSoundManager.Instance.SetVolumeTrack(track, linear);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.UI);
    }
}
```

Drop it on a GameObject in the scene, then assign it to the **Audio Provider**
field on `MenuController`, `PauseController`, `SettingsController`,
`TabController`, and `AudioSettingsModule`.

### UI juice — use Feel, not the menu

For button hover scale, click bounce, panel slide-in, screen shake — **use
MMFeedbacks (Feel)**, not this package. Add an `MMF_Player` next to each button
and trigger it from the button's `Pointer Enter` / `Pointer Click` events. The
menu package doesn't manage feedbacks; Feel does it better.

You can also hook feedbacks to `MenuEvents.OnPanelOpened` / `OnPanelClosed` to
react globally whenever any panel shows or hides.

---

## Cross-cutting: `MenuEvents` for gameplay scripts

When a gameplay script needs to pause or open settings without holding a
reference to the UI:

```csharp
using AnluMenu;

public class GameOverHandler : MonoBehaviour
{
    public void OnPlayerDied()
    {
        MenuEvents.RaisePause();          // pause overlay shows itself
        // ... or
        MenuEvents.RaiseOpenSettings();   // settings panel shows itself
    }
}
```

`MenuEvents` also raises **notifications** that any system can listen to:

```csharp
private void OnEnable()  => MenuEvents.OnPanelOpened += HandleOpened;
private void OnDisable() => MenuEvents.OnPanelOpened -= HandleOpened;

private void HandleOpened(string panelId)
{
    // panelId is "settings", "pause", etc.
    AnalyticsService.Log($"panel_opened:{panelId}");
}
```

---

## Multiplayer integration

### How `SceneLoader` decides

`SceneLoader.Strategy.ShouldLoadNetworked()` is checked on every load:

- Returns **false** (default `LocalSceneLoadStrategy`) → uses `SceneManager.LoadSceneAsync` with progress bar.
- Returns **true** (`NetcodeSceneLoadStrategy`, registered automatically when NGO is installed and you are an active server) → delegates to `NetworkManager.SceneManager.LoadScene`. NGO synchronizes clients.

You don't have to do anything to enable this — the bootstrap registers itself via `[RuntimeInitializeOnLoadMethod]` if NGO is present.

### Tell the menu when you go online

When the player starts a host or joins a client, set the global mode:

```csharp
GameModeContext.Set(GameMode.Multiplayer);
```

When they disconnect:

```csharp
GameModeContext.Set(GameMode.SinglePlayer);
```

The `PauseController` reads this:
- **SinglePlayer** → pause freezes `Time.timeScale`.
- **Multiplayer** → pause shows the overlay only; the server clock keeps ticking.

### Custom strategy

```csharp
SceneLoader.Strategy = new MyCustomLobbyAwareStrategy();
```

The bootstrap only assigns a default — replacing it later is fine.

---

## SceneLoader from gameplay

`SceneLoader` survives scene loads (`DontDestroyOnLoad`). From any gameplay
script you can call:

```csharp
MenuActions.LoadScene("MainMenu");
```

…which delegates to `SceneLoader.Instance.Load(...)` and resets `Time.timeScale`
to 1 in case the game was paused.

---

## Splash sequence

`SplashSequence` plays a list of panels in order: fade in → hold → fade out → next.
Useful for studio logos / engine splashes before the main menu. Set
**On Complete Show** to your Home panel and the menu reveals itself when the
sequence finishes.

The whole sequence runs on **unscaled time** so it works even if `Time.timeScale = 0`.

Skip is enabled by default: any key, click, or tap advances to the next panel.
Set **Skip Entire Sequence** to `true` if you want one press to jump straight
to the menu.

---

## Quit on WebGL

`Application.Quit()` is a no-op on WebGL. `MenuActions.Quit()` handles this by
firing a static event instead:

```csharp
// In your WebGL bootstrap or UI manager:
MenuActions.OnQuitRequested += () =>
{
    Application.OpenURL("https://your-game-portal.com");
};
```

On Desktop builds, `Quit()` calls `Application.Quit()` directly.

---

## File reference

| File                                          | Responsibility |
|-----------------------------------------------|----------------|
| `GameMode.cs`                                 | SP vs MP global context + change event |
| `ScreenFader.cs`                              | CanvasGroup fade utility (unscaled time) |
| `SceneLoader.cs`                              | Async load + progress + fade + minimum display time |
| `MenuController.cs`                           | Local panel switching with navigation stack |
| `PauseController.cs`                          | Pause overlay (mode-aware), listens to `MenuEvents` |
| `SplashSequence.cs`                           | Skippable studio logo / splash sequence |
| `ConfirmationDialog.cs`                       | Reusable modal confirmation popup (per-scene singleton) |
| `Actions/MenuActions.cs`                      | Static façade: Quit, LoadScene, RestartCurrentScene |
| `Actions/MenuActionButton.cs`                 | One component, many actions — inspector-driven button wiring |
| `Events/MenuEvents.cs`                        | Static bus for cross-cutting menu signals |
| `UI/TabController.cs`                         | Generic tab switcher (settings, inventory, anywhere) |
| `UI/ConfirmAction.cs`                         | Wraps `ConfirmationDialog` with `UnityEvent OnConfirmed` |
| `Audio/IUIAudio.cs`                           | Audio integration contract |
| `Audio/NullUIAudio.cs`                        | Silent default implementation |
| `Settings/PlayerPrefsKeys.cs`                 | Centralized prefs keys |
| `Settings/PlayerPrefsStorage.cs`              | Default `ISettingsStorage` implementation |
| `Settings/ISettingsModule.cs`                 | Module extensibility contract |
| `Settings/ISettingsStorage.cs`                | Storage abstraction (swap for cloud saves, etc.) |
| `Settings/SettingsController.cs`              | Owns settings panel, discovers modules, orchestrates Apply/Reset |
| `Settings/Modules/AudioSettingsModule.cs`     | Up to 5 volume sliders |
| `Settings/Modules/DisplaySettingsModule.cs`   | Fullscreen + VSync toggles |
| `Settings/Modules/QualitySettingsModule.cs`   | Quality preset dropdown |
| `Settings/Modules/ResolutionSettingsModule.cs`| Resolution dropdown |
| `Netcode/NetcodeSceneLoadStrategy.cs`         | NGO-aware scene loading |
| `Netcode/NetcodeBootstrap.cs`                 | Auto-registers NGO strategy |

---

## Migration from v2 → v3

If you have a v2 setup, here's what changed:

| v2                                                      | v3                                                        |
|---------------------------------------------------------|-----------------------------------------------------------|
| `MenuController.QuitGame()` on Quit button              | `MenuActionButton` with `Action = Quit`                   |
| `MenuController.LoadScene("X")` on Play button          | `MenuActionButton` with `Action = LoadScene`, `Scene Name = "X"` |
| `MenuController.OnQuitRequested += ...` (WebGL)         | `MenuActions.OnQuitRequested += ...`                      |
| `SettingsOverlay` on root + `SettingsController` on panel | Single `SettingsController` on always-active root, `_panel` field points to the panel |
| `SettingsOverlay.Instance.Toggle()` from code           | `MenuEvents.RaiseToggleSettings()` (or `SettingsController` method directly) |
| Wired pause buttons referenced `MenuController` (broken in gameplay scenes) | Pause buttons use `MenuActionButton` — same wiring works in any scene |

`MenuController` is now **local navigation only**. If you wired Quit or
LoadScene to it in v2, those references will be broken on import — replace with
`MenuActionButton` per the table above.

---

## FAQ

**Q: Why two asmdefs?**
A: NGO is optional. The Netcode asmdef compiles only when NGO is installed (via
`versionDefines` on `com.unity.netcode.gameobjects`). Keeps `Menu.Core` portable.

**Q: Direct refs vs events — when do I use which?**
A: Direct ref for local navigation (a button inside the MainMenu showing another
panel of the same menu). Event for cross-cutting requests (a gameplay script
asking the UI to pause, a pause button asking to open settings). The rule:
if A and B don't live in the same hierarchy, use events.

**Q: Can I use this without TextMeshPro?**
A: No. The loading screen label is a `TMP_Text`. TMP is a default Unity package
in modern URP/HDRP templates, so this should never be a real blocker.

**Q: Why not a singleton MenuController?**
A: It lives in the MainMenu scene and doesn't need to persist across loads.
`SceneLoader` does persist (DontDestroyOnLoad) because it survives the load it
triggers.

**Q: How do I disable the slider tick sound on a specific Slider?**
A: Uncheck `Auto Wire Slider Sounds` on the `MenuController` and wire only the
sliders you want manually (call `IUIAudio.PlaySlider()` from `Slider.OnValueChanged`).

**Q: How do I add a confirmation popup to a button?**
A: Add a `ConfirmAction` component to the button, fill in the title/message,
wire `OnClick → ConfirmAction.Trigger`, and put the real action (e.g.
`MenuActionButton.Trigger`) in `ConfirmAction.OnConfirmed`. See the
[Confirmation popups](#confirmation-popups-before-destructive-actions) section.
