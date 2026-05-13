using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Switches between menu panels (Home, Play, Settings, Exit, ...).
    /// Maintains a navigation stack so <see cref="Back()"/> returns to the previous panel
    /// without hardcoding destinations. Buttons connect via standard Unity OnClick events.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("Panel shown when the menu opens (e.g. Home).")]
        [SerializeField] private GameObject _initialPanel;

        [Tooltip("All panels managed by this controller. Only one is active at a time.")]
        [SerializeField] private List<GameObject> _allPanels = new();

        [Header("Audio")]
        [Tooltip("Optional MonoBehaviour implementing IUIAudio. Leave empty for silent menus.")]
        [SerializeField] private MonoBehaviour _audioProvider;

        [Tooltip("If true, every Slider under this Canvas plays the slider tick on value change.")]
        [SerializeField] private bool _autoWireSliderSounds = true;

        [Tooltip("Minimum seconds between slider tick sounds while dragging. Prevents overlap.")]
        [SerializeField] private float _sliderSoundThrottle = 0.08f;

        [Header("Navigation")]
        [Tooltip("If true, auto-selects the first Selectable in each panel when shown (gamepad/keyboard support).")]
        [SerializeField] private bool _autoSelectFirstButton = true;

        private float _lastSliderSoundTime = -1f;
        private readonly Stack<GameObject> _history = new();

        /// <summary>Active audio output. Defaults to <see cref="NullUIAudio"/>.</summary>
        public IUIAudio Audio { get; private set; } = new NullUIAudio();

        /// <summary>Currently visible panel, or null before <see cref="Start"/>.</summary>
        public GameObject CurrentPanel { get; private set; }

        /// <summary>
        /// Fires when QuitGame is called on platforms where Application.Quit() does not work
        /// (e.g. WebGL). Subscribe to show a "close this tab" message or redirect to a URL.
        /// </summary>
        public static event Action OnQuitRequested;

        private void Awake()
        {
            if (_audioProvider is IUIAudio audio) Audio = audio;
        }

        private void Start()
        {
            if (_initialPanel) ShowOnly(_initialPanel);
            if (_autoWireSliderSounds) WireSliderSounds();
        }

        /// <summary>Shows the given panel, pushes the current one to the back stack.</summary>
        public void Show(GameObject panel)
        {
            if (CurrentPanel != null && CurrentPanel != panel)
                _history.Push(CurrentPanel);
            ShowOnly(panel);
            Audio.PlayClick();
            SelectFirstButton(panel);
        }

        /// <summary>Returns to the previous panel in the navigation stack.</summary>
        public void Back()
        {
            if (_history.Count == 0) return;
            var previous = _history.Pop();
            ShowOnly(previous);
            Audio.PlayClick();
            SelectFirstButton(previous);
        }

        /// <summary>
        /// Navigates directly to a specific panel, clearing the history.
        /// Use for explicit "go Home" buttons or legacy wiring.
        /// </summary>
        public void Back(GameObject panel)
        {
            _history.Clear();
            ShowOnly(panel);
            Audio.PlayClick();
            SelectFirstButton(panel);
        }

        /// <summary>Triggers an async scene load through <see cref="SceneLoader"/>.</summary>
        public void LoadScene(string sceneName)
        {
            Audio.PlayClick();
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(sceneName);
            else Debug.LogError($"[MenuController] No SceneLoader.Instance found. Add a SceneLoader to the scene.");
        }

        /// <summary>
        /// Quits the application. Handles platform differences:
        /// Editor stops play mode, Desktop quits, WebGL fires <see cref="OnQuitRequested"/>.
        /// </summary>
        public void QuitGame()
        {
            Audio.PlayClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            if (OnQuitRequested != null) { OnQuitRequested.Invoke(); return; }
            Debug.LogWarning("[MenuController] Application.Quit() does not work on WebGL. " +
                "Subscribe to MenuController.OnQuitRequested to handle this (e.g. redirect to a URL).");
#else
            Application.Quit();
#endif
        }

        /// <summary>Replace the audio output at runtime (e.g. after instantiating an adapter).</summary>
        public void SetAudio(IUIAudio audio) => Audio = audio ?? new NullUIAudio();

        private void ShowOnly(GameObject panel)
        {
            foreach (var p in _allPanels)
                if (p != null) p.SetActive(p == panel);
            CurrentPanel = panel;
        }

        private void SelectFirstButton(GameObject panel)
        {
            if (!_autoSelectFirstButton || panel == null) return;
            var selectable = panel.GetComponentInChildren<Selectable>(false);
            if (selectable != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        private void WireSliderSounds()
        {
            foreach (var slider in GetComponentsInChildren<Slider>(true))
                slider.onValueChanged.AddListener(_ => PlaySliderThrottled());
        }

        private void PlaySliderThrottled()
        {
            float now = Time.unscaledTime;
            if (now - _lastSliderSoundTime < _sliderSoundThrottle) return;
            _lastSliderSoundTime = now;
            Audio.PlaySlider();
        }
    }
}
