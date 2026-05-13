using System;

namespace AnluMenu
{
    /// <summary>
    /// Identifies the current play context.
    /// Used by mode-aware components (e.g. PauseController) to alter behavior.
    /// </summary>
    public enum GameMode
    {
        SinglePlayer,
        Multiplayer
    }

    /// <summary>
    /// Holds the global GameMode. Set this when the player enters a multiplayer
    /// session (host start / client connect) and reset it on disconnect.
    /// </summary>
    /// <remarks>
    /// This is intentionally a static class instead of a singleton MonoBehaviour:
    /// it is read-mostly state with no per-frame work, no scene lifecycle, and
    /// must survive scene loads.
    /// </remarks>
    public static class GameModeContext
    {
        /// <summary>Fires when <see cref="Current"/> changes.</summary>
        public static event Action<GameMode> OnChanged;

        /// <summary>Current play context. Defaults to SinglePlayer.</summary>
        public static GameMode Current { get; private set; } = GameMode.SinglePlayer;

        /// <summary>Sets the current mode and notifies subscribers if it changed.</summary>
        public static void Set(GameMode mode)
        {
            if (Current == mode) return;
            Current = mode;
            OnChanged?.Invoke(mode);
        }
    }
}
