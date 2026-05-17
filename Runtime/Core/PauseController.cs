using UnityEngine;
using UnityEngine.InputSystem;

namespace AnluMenu
{
    /// <summary>
    /// Toggleable pause overlay. In SinglePlayer it freezes Time.timeScale.
    /// In Multiplayer it only shows the overlay — the server clock keeps running.
    /// Optionally switches Input System action maps and manages cursor state.
    /// </summary>
    /// <remarks>
    /// Listens to <see cref="MenuEvents"/> so gameplay scripts can request pause without
    /// holding a reference. For Quit / Restart / "Main Menu" buttons inside the pause UI,
    /// add a <see cref="MenuActionButton"/> on each button instead of extending this class.
    /// </remarks>
    public class PauseController : MonoBehaviour
    {
        [Tooltip("Root GameObject of the pause UI. Toggled on/off by this controller.")]
        [SerializeField] private GameObject _pauseMenu;

        [Tooltip("Keyboard key that toggles pause. Set to None to disable the local keyboard shortcut.")]
        [SerializeField] private Key _toggleKey = Key.Escape;

        [Tooltip("Optional audio output (same MonoBehaviour-implementing-IUIAudio pattern as MenuController).")]
        [SerializeField] private MonoBehaviour _audioProvider;

        [Header("Action Maps (optional)")]
        [Tooltip("Optional PlayerInput component. If set, switches between gameplay and UI action maps on pause/resume.")]
        [SerializeField] private PlayerInput _playerInput;

        [Tooltip("Name of the gameplay action map (must match your InputActions asset).")]
        [SerializeField] private string _gameplayActionMap = "Player";

        [Tooltip("Name of the UI action map (must match your InputActions asset).")]
        [SerializeField] private string _uiActionMap = "UI";

        [Header("Cursor (optional)")]
        [Tooltip("If true, shows and unlocks cursor on pause, hides and locks on resume. Useful for first/third person games.")]
        [SerializeField] private bool _manageCursor;

        public bool IsPaused { get; private set; }

        private const string PanelId = "pause";

        private IUIAudio _audio = new NullUIAudio();

        private void Awake()
        {
            if (_audioProvider is IUIAudio a) _audio = a;
            if (_pauseMenu) _pauseMenu.SetActive(false);
        }

        private void OnEnable()
        {
            MenuEvents.OnPauseRequested       += Pause;
            MenuEvents.OnResumeRequested      += Resume;
            MenuEvents.OnTogglePauseRequested += Toggle;
        }

        private void OnDisable()
        {
            MenuEvents.OnPauseRequested       -= Pause;
            MenuEvents.OnResumeRequested      -= Resume;
            MenuEvents.OnTogglePauseRequested -= Toggle;

            // Safety: never leave the engine paused if this controller is destroyed.
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_toggleKey == Key.None) return;
            if (Keyboard.current != null && Keyboard.current[_toggleKey].wasPressedThisFrame)
                Toggle();
        }

        /// <summary>Toggles pause state.</summary>
        public void Toggle() { if (IsPaused) Resume(); else Pause(); }

        /// <summary>Pauses the game (freezes time only in SinglePlayer).</summary>
        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            if (_pauseMenu) _pauseMenu.SetActive(true);
            _audio.PlayClick();

            if (GameModeContext.Current == GameMode.SinglePlayer)
                Time.timeScale = 0f;

            if (_playerInput) _playerInput.SwitchCurrentActionMap(_uiActionMap);

            if (_manageCursor)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            MenuEvents.RaisePanelOpened(PanelId);
        }

        /// <summary>Resumes the game and restores Time.timeScale.</summary>
        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            if (_pauseMenu) _pauseMenu.SetActive(false);
            _audio.PlayClick();
            Time.timeScale = 1f;

            if (_playerInput) _playerInput.SwitchCurrentActionMap(_gameplayActionMap);

            if (_manageCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            MenuEvents.RaisePanelClosed(PanelId);
        }
    }
}
