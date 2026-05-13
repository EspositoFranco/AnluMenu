# AnluMenu Package

> Author: **Franco Esposito**

A reusable, opinionated menu system for Unity. Ships with:

- **MenuController** — panel switching with navigation stack and gamepad auto-select.
- **SceneLoader** — async scene load with fade + progress bar + minimum display time.
- **PauseController** — pause overlay, mode-aware (freezes time only in single-player).
- **SplashSequence** — multi-panel fade-in/out splash for studio logos, skippable.
- **SettingsController** + extensible **ISettingsModule** — Audio, Display, and whatever you add.
- **SettingsOverlay** — per-scene singleton to show/hide the settings panel from anywhere.
- **ConfirmationDialog** — reusable modal popup with callbacks.
- **IUIAudio** abstraction so the package never references a concrete audio system.
- Optional **Netcode for GameObjects** integration via a separate assembly that
  auto-loads when NGO is installed.

The package is **deliberately small**. It does the wiring; you provide the UI.

---

## Folder structure

```
Packages/com.anlu.menu/
├── package.json                              ← UPM manifest
├── README.md                                 ← this file
├── AUTHORS.md
├── CHANGELOG.md
└── Runtime/
    ├── Core/                                 ← AnluMenu.Core asmdef (no NGO dependency)
    │   ├── GameMode.cs
    │   ├── ScreenFader.cs
    │   ├── SceneLoader.cs
    │   ├── MenuController.cs
    │   ├── PauseController.cs
    │   ├── SplashSequence.cs
    │   ├── ConfirmationDialog.cs
    │   ├── Audio/
    │   │   ├── IUIAudio.cs
    │   │   └── NullUIAudio.cs
    │   └── Settings/
    │       ├── PlayerPrefsKeys.cs
    │       ├── ISettingsModule.cs
    │       ├── SettingsController.cs
    │       ├── SettingsOverlay.cs
    │       └── Modules/
    │           ├── AudioSettingsModule.cs
    │           └── DisplaySettingsModule.cs
    └── Netcode/                              ← AnluMenu.Netcode asmdef (compiled only when NGO present)
        ├── NetcodeSceneLoadStrategy.cs
        └── NetcodeBootstrap.cs
```

### Two assemblies, one switch

`AnluMenu.Core` has **zero** dependencies on Netcode for GameObjects. Drop it
into any Unity project (multiplayer or not) and it compiles.

`AnluMenu.Netcode` declares a `versionDefines` entry on
`com.unity.netcode.gameobjects` that emits the symbol `ANLU_MENU_USE_NETCODE`,
which is also its `defineConstraints` gate. Translation: **the Netcode assembly
only compiles when NGO is installed**. Uninstall NGO → the assembly silently drops
out of the build, and `SceneLoader` falls back to the default local strategy
without any code changes.

---

## Quick start (5 minutes)

1. Open your MainMenu scene.
2. Create a Canvas with these child panels (any layout): **Home**, **Play**, **Settings**, **Exit**, **Loading**.
3. On the Canvas GameObject, add `MenuController`. In the Inspector:
   - **Initial Panel** → drag the Home panel.
   - **All Panels** → drag every panel managed by this controller.
4. On a separate root GameObject (e.g. `_Loaders`), add `SceneLoader`. Drag the Loading panel into **Loading Screen**, the progress Slider into **Progress Bar**, and (optionally) a `ScreenFader` into **Fader**.
5. On the Settings panel, add `SettingsController`. Add `AudioSettingsModule` and `DisplaySettingsModule` as child components and wire your sliders/toggles in the Inspector.
6. Add a `SettingsOverlay` component to a root GameObject in each scene where you want settings access. Wire its `_panel` to the Settings panel.
7. Wire button OnClick events (see next section).
8. Hit Play.

The Menu has no Audio yet. It's silent by design until you provide an `IUIAudio` adapter (see [Audio integration](#audio-integration)).

---

## Wiring buttons

Buttons connect via the standard **Unity OnClick UnityEvent**. No runtime "FixButtons" hack — if a reference breaks in the prefab, fix the prefab.

| Button             | Target component        | Method                | Parameter          |
|--------------------|-------------------------|-----------------------|--------------------|
| Play               | MenuController          | `LoadScene(string)`   | scene name         |
| Settings           | MenuController          | `Show(GameObject)`    | Settings panel ref |
| Back (stack)       | MenuController          | `Back()`              | —                  |
| Back (explicit)    | MenuController          | `Back(GameObject)`    | Home panel ref     |
| Quit               | MenuController          | `QuitGame()`          | —                  |
| Resume (in pause)  | PauseController         | `Resume()`            | —                  |
| Apply (settings)   | SettingsController      | `ApplyAll()`          | —                  |
| Reset (settings)   | SettingsController      | `ResetAll()`          | —                  |
| Settings (toggle)  | SettingsOverlay          | `Toggle()`            | —                  |

---

## Audio integration

The Menu package **never references a concrete audio system**. You bridge it via
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

Drop `MMSoundManagerUIAudio` on a GameObject in the scene, then assign that
GameObject to the **Audio Provider** field on `MenuController`,
`PauseController`, and `AudioSettingsModule`.

### UI juice — use Feel, not the menu

For button hover scale, click bounce, panel slide-in, screen shake — **use
MMFeedbacks (Feel)**, not this package. Add an `MMF_Player` next to each button
and trigger it from the button's `Pointer Enter` / `Pointer Click` events. The
menu package doesn't manage feedbacks; Feel does it better.

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

Hook this from your existing `LobbyUI` (right after `StartHost()` / `StartClient()`).

### Custom strategy

If you outgrow the default behavior (e.g. you want to use Unity Lobby + Relay
with a different scene-load flow), implement `ISceneLoadStrategy` and assign it
yourself before the first `SceneLoader.Load` call:

```csharp
SceneLoader.Strategy = new MyCustomLobbyAwareStrategy();
```

The bootstrap only assigns a default — replacing it later is fine.

---

## Adding a new Settings module

Settings are extensible by design. To add (for example) a graphics quality dropdown:

```csharp
using AnluMenu;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsModule : MonoBehaviour, ISettingsModule
{
    private const string Key = "menu.graphics.quality";

    [SerializeField] private Dropdown _qualityDropdown;

    public void Initialize()
    {
        _qualityDropdown.SetValueWithoutNotify(
            PlayerPrefs.GetInt(Key, QualitySettings.GetQualityLevel()));
        _qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void Apply() => SetQuality(_qualityDropdown.value);

    public void ResetToDefaults()
    {
        _qualityDropdown.value = 2;
        PlayerPrefs.SetInt(Key, 2);
    }

    private void SetQuality(int level)
    {
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt(Key, level);
    }
}
```

Drop it as a child of the `SettingsController` GameObject. The controller picks
it up automatically on Awake — no manual registration. Add a corresponding
constant to `PlayerPrefsKeys.cs` to keep keys centralized.

---

## SceneLoader from gameplay

`SceneLoader` survives scene loads (`DontDestroyOnLoad`). From any gameplay
script you can:

```csharp
SceneLoader.Instance.Load("MainMenu");
```

…to return to the main menu (e.g. from a "Quit to Menu" button in the pause panel).

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

## Confirmation dialog

`ConfirmationDialog` is a per-scene singleton for modal confirmations (quit,
reset settings, discard changes, etc.). Wire the UI in the Inspector, then call:

```csharp
ConfirmationDialog.Instance.Show(
    "Quit Game",
    "Are you sure you want to quit?",
    onConfirm: () => Application.Quit(),
    onCancel: null,
    confirmText: "Yes",
    cancelText: "No"
);
```

The dialog hides itself after Confirm or Cancel and clears callbacks.

---

## Quit on WebGL

`Application.Quit()` is a no-op on WebGL. `MenuController.QuitGame()` handles
this by firing a static event instead:

```csharp
// In your WebGL bootstrap or UI manager:
MenuController.OnQuitRequested += () =>
{
    // Redirect to your website, or show a "close this tab" message.
    Application.OpenURL("https://your-game-portal.com");
};
```

On Desktop builds, `QuitGame()` calls `Application.Quit()` directly.

---

## File reference

| File                                          | Responsibility |
|-----------------------------------------------|----------------|
| `GameMode.cs`                                 | SP vs MP global context + change event |
| `ScreenFader.cs`                              | CanvasGroup fade utility (unscaled time) |
| `SceneLoader.cs`                              | Async load + progress + fade + minimum display time |
| `MenuController.cs`                           | Panel switching with navigation stack + gamepad auto-select |
| `PauseController.cs`                          | Pause overlay (mode-aware) |
| `SplashSequence.cs`                           | Skippable studio logo / splash sequence |
| `ConfirmationDialog.cs`                       | Reusable modal confirmation popup |
| `Audio/IUIAudio.cs`                           | Audio integration contract |
| `Audio/NullUIAudio.cs`                        | Silent default implementation |
| `Settings/PlayerPrefsKeys.cs`                 | Centralized prefs keys |
| `Settings/ISettingsModule.cs`                 | Extensibility contract |
| `Settings/SettingsController.cs`              | Auto-discovers and orchestrates modules |
| `Settings/SettingsOverlay.cs`                 | Per-scene singleton for settings panel toggle |
| `Settings/Modules/AudioSettingsModule.cs`     | Up to 5 volume sliders |
| `Settings/Modules/DisplaySettingsModule.cs`   | Fullscreen + VSync toggles |
| `Netcode/NetcodeSceneLoadStrategy.cs`         | NGO-aware scene loading |
| `Netcode/NetcodeBootstrap.cs`                 | Auto-registers NGO strategy |

---

## FAQ

**Q: Why two asmdefs?**
A: NGO is optional. The Netcode asmdef compiles only when NGO is installed (via
`versionDefines` on `com.unity.netcode.gameobjects`). Keeps `AnluMenu.Core` portable.

**Q: Can I use this without TextMeshPro?**
A: No. The loading screen label is a `TMP_Text`. TMP is a default Unity package
in modern URP/HDRP templates, so this should never be a real blocker.

**Q: Why not a singleton MenuController?**
A: It lives in the MainMenu scene and doesn't need to persist across loads.
`SceneLoader` does persist (DontDestroyOnLoad) because it survives the load it
triggers.

**Q: Why does `IUIAudio` take an enum instead of a `string` channel name?**
A: Compile-time safety. Strings drift; enums catch misuse at the call site.

**Q: How do I disable the slider tick sound on a specific Slider?**
A: Uncheck `Auto Wire Slider Sounds` on the `MenuController` and wire only the
sliders you want manually (call `IUIAudio.PlaySlider()` from `Slider.OnValueChanged`).

**Q: How do I add a new pause-menu button (e.g. "Quit to Main Menu")?**
A: On its OnClick: call `MenuController.LoadScene("MainMenu")`. Make sure the
button also calls `PauseController.Resume()` first to restore `Time.timeScale`.
