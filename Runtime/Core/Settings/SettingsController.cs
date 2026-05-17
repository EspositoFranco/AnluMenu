using System.Collections.Generic;
using UnityEngine;


namespace AnluMenu
{
    /// <summary>
    /// Owns the settings panel: shows/hides it, auto-discovers <see cref="ISettingsModule"/>
    /// components under its <see cref="_panel"/>, and orchestrates Initialize/Apply/Reset.
    /// Listens to <see cref="MenuEvents"/> so any system (main menu, pause, gameplay)
    /// can open settings without holding a reference.
    /// </summary>
    /// <remarks>
    /// Place this on an always-active root (e.g. the Canvas root) and wire
    /// <see cref="_panel"/> to the actual settings panel GameObject that holds the
    /// module components as children. The panel starts hidden.
    /// Storage defaults to <see cref="PlayerPrefsStorage"/> unless a custom
    /// <see cref="ISettingsStorage"/> MonoBehaviour is wired in the Inspector.
    /// </remarks>
    public class SettingsController : MonoBehaviour
    {
        [Header("Panel")]
        [Tooltip("Root GameObject of the settings panel. Toggled by Show/Hide and the MenuEvents bus.")]
        [SerializeField] private GameObject _panel;

        [Header("Storage")]
        [Tooltip("Optional MonoBehaviour implementing ISettingsStorage. Defaults to PlayerPrefs.")]
        [SerializeField] private MonoBehaviour _storageProvider;

        [Header("Audio")]
        [Tooltip("Optional MonoBehaviour implementing IUIAudio.")]
        [SerializeField] private MonoBehaviour _audioProvider;

        /// <summary>Active settings storage. Defaults to <see cref="PlayerPrefsStorage"/>.</summary>
        public ISettingsStorage Storage { get; private set; } = new PlayerPrefsStorage();

        public bool IsOpen { get; private set; }

        private const string PanelId = "settings";

        private readonly List<ISettingsModule> _modules = new();
        private IUIAudio _audio = new NullUIAudio();

        private void Awake()
        {
            if (_storageProvider is ISettingsStorage s) Storage = s;
            if (_audioProvider is IUIAudio a) _audio = a;

            // Discover modules from the panel subtree (controller can live above it).
            var searchRoot = _panel != null ? _panel : gameObject;
            searchRoot.GetComponentsInChildren(true, _modules);
            foreach (var m in _modules) m.Initialize(Storage);

            if (_panel) _panel.SetActive(false);
        }

        private void OnEnable()
        {
            MenuEvents.OnOpenSettingsRequested   += Show;
            MenuEvents.OnCloseSettingsRequested  += Hide;
            MenuEvents.OnToggleSettingsRequested += Toggle;
        }

        private void OnDisable()
        {
            MenuEvents.OnOpenSettingsRequested   -= Show;
            MenuEvents.OnCloseSettingsRequested  -= Hide;
            MenuEvents.OnToggleSettingsRequested -= Toggle;
        }

        /// <summary>Replace the storage backend at runtime (e.g. after login to a cloud save service).</summary>
        public void SetStorage(ISettingsStorage storage) => Storage = storage ?? new PlayerPrefsStorage();

        /// <summary>Shows the settings panel.</summary>
        public void Show()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (_panel) _panel.SetActive(true);
            _audio.PlayClick();
            MenuEvents.RaisePanelOpened(PanelId);
        }

        /// <summary>Hides the settings panel.</summary>
        public void Hide()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (_panel) _panel.SetActive(false);
            _audio.PlayClick();
            MenuEvents.RaisePanelClosed(PanelId);
        }

        /// <summary>Toggles the settings panel visibility.</summary>
        public void Toggle() { if (IsOpen) Hide(); else Show(); }

        /// <summary>Re-applies every module's current state and saves.</summary>
        public void ApplyAll()
        {
            foreach (var m in _modules) m.Apply();
            Storage.Save();
        }

        /// <summary>Resets every module to defaults and saves.</summary>
        public void ResetAll()
        {
            foreach (var m in _modules) m.ResetToDefaults();
            Storage.Save();
        }
    }
}
