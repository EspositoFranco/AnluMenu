using UnityEngine;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Display settings module: fullscreen toggle and VSync toggle.
    /// Persists to <see cref="ISettingsStorage"/> and applies to Screen / QualitySettings on change.
    /// </summary>
    public class DisplaySettingsModule : MonoBehaviour, ISettingsModule
    {
        [Tooltip("Toggle for fullscreen mode. Leave empty if not used.")]
        [SerializeField] private Toggle _fullscreenToggle;

        [Tooltip("Toggle for vertical sync. Leave empty if not used.")]
        [SerializeField] private Toggle _vsyncToggle;

        private ISettingsStorage _storage;

        public void Initialize(ISettingsStorage storage)
        {
            _storage = storage;

            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.SetIsOnWithoutNotify(_storage.GetInt(PlayerPrefsKeys.Fullscreen, 1) == 1);
                _fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
            if (_vsyncToggle != null)
            {
                _vsyncToggle.SetIsOnWithoutNotify(_storage.GetInt(PlayerPrefsKeys.VSync, 1) == 1);
                _vsyncToggle.onValueChanged.AddListener(SetVSync);
            }
            ApplyState();
        }

        public void Apply() => ApplyState();

        public void ResetToDefaults()
        {
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = true;
            if (_vsyncToggle != null) _vsyncToggle.isOn = true;
            ApplyState();
        }

        private void SetFullscreen(bool on)
        {
            _storage.SetInt(PlayerPrefsKeys.Fullscreen, on ? 1 : 0);
            Screen.fullScreen = on;
        }

        private void SetVSync(bool on)
        {
            _storage.SetInt(PlayerPrefsKeys.VSync, on ? 1 : 0);
            QualitySettings.vSyncCount = on ? 1 : 0;
        }

        private void ApplyState()
        {
            Screen.fullScreen = _storage.GetInt(PlayerPrefsKeys.Fullscreen, 1) == 1;
            QualitySettings.vSyncCount = _storage.GetInt(PlayerPrefsKeys.VSync, 1) == 1 ? 1 : 0;
        }
    }
}
