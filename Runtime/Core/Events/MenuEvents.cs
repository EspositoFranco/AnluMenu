using System;

namespace AnluMenu
{
    /// <summary>
    /// Static event bus for cross-cutting menu signals. Any system (gameplay scripts,
    /// pause UI, main menu) can raise these requests without holding a reference to
    /// the receiver. Receivers (SettingsController, PauseController, ...) self-register.
    /// </summary>
    /// <remarks>
    /// Use this for cross-cutting requests only. For local navigation inside a single
    /// menu (Home -> Settings -> Back, tab switching) keep using direct references —
    /// events lose the explicitness and history stack you get with direct calls.
    /// </remarks>
    public static class MenuEvents
    {
        // Settings panel
        public static event Action OnOpenSettingsRequested;
        public static event Action OnCloseSettingsRequested;
        public static event Action OnToggleSettingsRequested;

        // Pause overlay
        public static event Action OnPauseRequested;
        public static event Action OnResumeRequested;
        public static event Action OnTogglePauseRequested;

        // Lifecycle notifications (for analytics / audio / feedbacks)
        public static event Action<string> OnPanelOpened;
        public static event Action<string> OnPanelClosed;

        public static void RaiseOpenSettings()   => OnOpenSettingsRequested?.Invoke();
        public static void RaiseCloseSettings()  => OnCloseSettingsRequested?.Invoke();
        public static void RaiseToggleSettings() => OnToggleSettingsRequested?.Invoke();

        public static void RaisePause()       => OnPauseRequested?.Invoke();
        public static void RaiseResume()      => OnResumeRequested?.Invoke();
        public static void RaiseTogglePause() => OnTogglePauseRequested?.Invoke();

        public static void RaisePanelOpened(string id) => OnPanelOpened?.Invoke(id);
        public static void RaisePanelClosed(string id) => OnPanelClosed?.Invoke(id);
    }
}
