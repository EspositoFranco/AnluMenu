using UnityEngine;

namespace AnluMenu
{
/// <summary>
/// Per-scene settings overlay singleton. Opens/closes a settings panel on demand.
/// Works from any MonoBehaviour in the same scene via SettingsOverlay.Instance.
///
/// Each scene (MainMenu, Game) can have its own instance. Settings values persist
/// automatically via PlayerPrefs regardless of which scene's panel was last used.
///
/// Wire _panel to the Settings panel GameObject that contains a SettingsController.
/// </summary>
public class SettingsOverlay : MonoBehaviour
{
    public static SettingsOverlay Instance { get; private set; }

    [Tooltip("Root GameObject of the settings panel.")]
    [SerializeField] private GameObject _panel;

    [Tooltip("Optional MonoBehaviour implementing IUIAudio.")]
    [SerializeField] private MonoBehaviour _audioProvider;

    public bool IsOpen { get; private set; }

    private IUIAudio _audio = new NullUIAudio();

    private void Awake()
    {
        Instance = this;
        if (_audioProvider is IUIAudio a) _audio = a;
        if (_panel) _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show()
    {
        IsOpen = true;
        if (_panel) _panel.SetActive(true);
        _audio.PlayClick();
    }

    public void Hide()
    {
        IsOpen = false;
        if (_panel) _panel.SetActive(false);
        _audio.PlayClick();
    }

    public void Toggle() { if (IsOpen) Hide(); else Show(); }
}
}
