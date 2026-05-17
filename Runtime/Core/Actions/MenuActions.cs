using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnluMenu
{
    /// <summary>
    /// Static façade for common menu actions reusable from any scene or controller.
    /// Lives outside MenuController so Pause, gameplay scripts, or any button can
    /// call Quit / Restart / LoadScene without depending on a MainMenu-only controller.
    /// </summary>
    public static class MenuActions
    {
        /// <summary>
        /// Fires when <see cref="Quit"/> is called on platforms where Application.Quit()
        /// does not work (e.g. WebGL). Subscribe to redirect to a URL or show a message.
        /// </summary>
        public static event Action OnQuitRequested;

        /// <summary>
        /// Quits the application. Editor stops play mode, Desktop quits,
        /// WebGL fires <see cref="OnQuitRequested"/>.
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            if (OnQuitRequested != null) { OnQuitRequested.Invoke(); return; }
            Debug.LogWarning("[MenuActions] Application.Quit() is a no-op on WebGL. " +
                "Subscribe to MenuActions.OnQuitRequested to handle this.");
#else
            Application.Quit();
#endif
        }

        /// <summary>Triggers an async scene load via <see cref="SceneLoader"/>.</summary>
        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[MenuActions] LoadScene called with empty scene name.");
                return;
            }
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[MenuActions] No SceneLoader.Instance found. Add a SceneLoader to the bootstrap scene.");
                return;
            }
            // Ensure timeScale is normal before loading (in case we paused first).
            if (Time.timeScale == 0f) Time.timeScale = 1f;
            SceneLoader.Instance.Load(sceneName);
        }

        /// <summary>
        /// Reloads the currently active scene. Bypasses the SceneLoader's configured minimum
        /// loading duration so the restart feels instant (fade only, no artificial wait).
        /// </summary>
        public static void RestartCurrentScene()
        {
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[MenuActions] No SceneLoader.Instance found. Add a SceneLoader to the bootstrap scene.");
                return;
            }
            if (Time.timeScale == 0f) Time.timeScale = 1f;
            SceneLoader.Instance.Load(SceneManager.GetActiveScene().name, 0f);
        }
    }
}
