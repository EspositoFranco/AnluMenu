using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Pluggable strategy used by <see cref="SceneLoader"/> to decide how a scene
    /// load is performed. The default ("local") strategy uses Unity's SceneManager.
    /// The AnluMenu.Netcode assembly registers a NGO-aware strategy automatically when
    /// the Netcode for GameObjects package is present.
    /// </summary>
    public interface ISceneLoadStrategy
    {
        /// <summary>True when the load should be delegated to a network manager.</summary>
        bool ShouldLoadNetworked();

        /// <summary>Called when ShouldLoadNetworked() returns true.</summary>
        void LoadNetworked(string sceneName);
    }

    internal sealed class LocalSceneLoadStrategy : ISceneLoadStrategy
    {
        public bool ShouldLoadNetworked() => false;
        public void LoadNetworked(string sceneName) { /* unreachable */ }
    }

    /// <summary>
    /// Persistent scene loader. Handles async load with progress bar, optional fade,
    /// optional "press any key to continue" gate at 90% load, and a configurable
    /// minimum loading duration to prevent the loading screen from flashing.
    /// </summary>
    /// <remarks>
    /// Place one instance in your bootstrap or MainMenu scene; it survives scene loads.
    /// In multiplayer, the host's loader triggers the load, and clients follow via NGO.
    /// </remarks>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        /// <summary>
        /// The active load strategy. Defaults to local SceneManager. Replace at runtime
        /// (e.g. from AnluMenu.Netcode bootstrap) to support multiplayer-aware loading.
        /// </summary>
        public static ISceneLoadStrategy Strategy { get; set; } = new LocalSceneLoadStrategy();

        [Header("Loading Screen")]
        [Tooltip("Root GameObject of the loading UI. Activated while loading.")]
        [SerializeField] private GameObject _loadingScreen;

        [Tooltip("Optional progress bar driven by the AsyncOperation.")]
        [SerializeField] private Slider _progressBar;

        [Tooltip("Optional label that shows the press-any-key prompt at 90%.")]
        [SerializeField] private TMP_Text _progressLabel;

        [Header("Fade")]
        [Tooltip("Optional fade overlay played before/after the load.")]
        [SerializeField] private ScreenFader _fader;

        [Tooltip("If true, the load pauses at 90% until any key is pressed.")]
        [SerializeField] private bool _requireKeyToEnter = false;

        [Tooltip("Text shown on the progress label when waiting for key press.")]
        [SerializeField] private string _pressKeyMessage = "Press any key to continue";

        [Header("Timing")]
        [Tooltip("Minimum seconds the loading screen stays visible. Prevents flashing on fast loads. Set to 0 to disable.")]
        [SerializeField] private float _minimumLoadingDuration = 1.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (_loadingScreen) _loadingScreen.SetActive(false);
        }

        /// <summary>
        /// Loads a scene asynchronously with fade and progress, using the configured
        /// <see cref="_minimumLoadingDuration"/>.
        /// </summary>
        public void Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(LoadRoutine(sceneName, _minimumLoadingDuration));
        }

        /// <summary>
        /// Loads a scene asynchronously, overriding the minimum loading duration for this call only.
        /// Pass <c>0f</c> for an instant load (e.g. level restart where no loading screen is needed).
        /// </summary>
        public void Load(string sceneName, float minimumDuration)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(LoadRoutine(sceneName, Mathf.Max(0f, minimumDuration)));
        }

        private IEnumerator LoadRoutine(string sceneName, float minimumDuration)
        {
            if (_fader) yield return _fader.FadeIn();

            // Multiplayer path: host triggers load via the registered strategy.
            if (Strategy.ShouldLoadNetworked())
            {
                Strategy.LoadNetworked(sceneName);
                yield break;
            }

            if (_loadingScreen) _loadingScreen.SetActive(true);

            float loadStartTime = Time.unscaledTime;

            var op = SceneManager.LoadSceneAsync(sceneName);
            // Hold scene activation until timing and input gates are satisfied.
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                if (_progressBar) _progressBar.value = progress;

                if (op.progress >= 0.9f)
                {
                    float elapsed = Time.unscaledTime - loadStartTime;
                    bool timeReached = elapsed >= minimumDuration;

                    if (_requireKeyToEnter)
                    {
                        if (timeReached && _progressLabel) _progressLabel.text = _pressKeyMessage;
                        if (timeReached && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                            op.allowSceneActivation = true;
                    }
                    else if (timeReached)
                    {
                        op.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            if (_loadingScreen) _loadingScreen.SetActive(false);
            if (_fader) yield return _fader.FadeOut();
        }
    }
}
