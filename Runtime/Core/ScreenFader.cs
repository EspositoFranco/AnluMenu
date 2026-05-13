using System.Collections;
using UnityEngine;

namespace AnluMenu
{
    /// <summary>
    /// Reusable fade utility. Drives a CanvasGroup's alpha between 0 and 1.
    /// Uses unscaled time so it works while paused.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [Tooltip("Default fade duration in seconds when no override is provided.")]
        [SerializeField] private float _defaultDuration = 0.4f;

        private CanvasGroup _group;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
        }

        /// <summary>Fades from current alpha to 1 (opaque). Blocks raycasts while fading in.</summary>
        public IEnumerator FadeIn(float? duration = null)
            => Fade(_group.alpha, 1f, duration ?? _defaultDuration, true);

        /// <summary>Fades from current alpha to 0 (transparent). Releases raycasts when done.</summary>
        public IEnumerator FadeOut(float? duration = null)
            => Fade(_group.alpha, 0f, duration ?? _defaultDuration, false);

        private IEnumerator Fade(float from, float to, float duration, bool blockDuringFade)
        {
            _group.blocksRaycasts = blockDuringFade;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            _group.alpha = to;
            _group.blocksRaycasts = to > 0.5f;
        }
    }
}
