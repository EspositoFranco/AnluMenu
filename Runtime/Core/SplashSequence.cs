using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AnluMenu
{
    /// <summary>
    /// Plays a list of CanvasGroup panels in sequence (fade in → hold → fade out → next).
    /// Fires <see cref="OnCompleted"/> when the last panel finishes.
    /// Uses unscaled time so it runs even if Time.timeScale is 0.
    /// Optionally skippable via any key/tap/click.
    /// </summary>
    public class SplashSequence : MonoBehaviour
    {
        [Tooltip("Panels to play in order. Each must be a UI GameObject (CanvasGroup added if missing).")]
        [SerializeField] private List<GameObject> _panels = new();

        [Tooltip("Seconds the panel stays fully visible between fades.")]
        [SerializeField] private float _holdDuration = 1.5f;

        [Tooltip("Seconds for each fade in / fade out.")]
        [SerializeField] private float _fadeDuration = 0.4f;

        [Tooltip("Optional GameObject activated when the sequence completes (e.g. the Home panel).")]
        [SerializeField] private GameObject _onCompleteShow;

        [Tooltip("If false, you must call Play() manually.")]
        [SerializeField] private bool _playOnStart = true;

        [Header("Skip")]
        [Tooltip("If true, the player can skip by pressing any key, clicking, or tapping.")]
        [SerializeField] private bool _skippable = true;

        [Tooltip("If true, skip jumps to the end of the entire sequence. If false, skip advances to the next panel.")]
        [SerializeField] private bool _skipEntireSequence;

        /// <summary>Fired when the entire sequence finishes.</summary>
        public event Action OnCompleted;

        private bool _skipRequested;
        private bool _isRunning;

        private void Start()
        {
            if (_playOnStart) Play();
        }

        private void Update()
        {
            if (!_isRunning || !_skippable || _skipRequested) return;

            bool pressed = false;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) pressed = true;
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) pressed = true;
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) pressed = true;

            if (pressed) _skipRequested = true;
        }

        /// <summary>Starts the splash sequence from the first panel.</summary>
        public void Play()
        {
            _skipRequested = false;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // Grace frame: ignore input that was already held when the sequence started.
            yield return null;
            _isRunning = true;

            foreach (var panel in _panels)
            {
                if (panel == null) continue;
                _skipRequested = false;

                var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                panel.SetActive(true);

                // Fade in (interruptible)
                yield return TweenInterruptible(cg, 0f, 1f, _fadeDuration);

                // Hold (interruptible)
                if (!_skipRequested)
                {
                    float held = 0f;
                    while (held < _holdDuration && !_skipRequested)
                    {
                        held += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }

                // Fade out (fast if skipping)
                float outDuration = _skipRequested ? Mathf.Min(_fadeDuration * 0.25f, 0.1f) : _fadeDuration;
                yield return Tween(cg, cg.alpha, 0f, outDuration);
                panel.SetActive(false);

                if (_skipRequested && _skipEntireSequence) break;
            }

            // Safety: hide any remaining panels if sequence was skipped.
            foreach (var panel in _panels)
            {
                if (panel == null || !panel.activeSelf) continue;
                var cg = panel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
                panel.SetActive(false);
            }

            _isRunning = false;
            if (_onCompleteShow) _onCompleteShow.SetActive(true);
            OnCompleted?.Invoke();
        }

        private IEnumerator TweenInterruptible(CanvasGroup cg, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration && !_skipRequested)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            cg.alpha = to;
        }

        private static IEnumerator Tween(CanvasGroup cg, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            cg.alpha = to;
        }
    }
}
