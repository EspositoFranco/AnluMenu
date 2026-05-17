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
    /// <remarks>
    /// This controller handles local navigation only. For Quit / LoadScene / RestartScene
    /// use <see cref="MenuActionButton"/> (inspector) or <see cref="MenuActions"/> (code) —
    /// those are reusable from any scene, including pause and gameplay.
    /// </remarks>
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
