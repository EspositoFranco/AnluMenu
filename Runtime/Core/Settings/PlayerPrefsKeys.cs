namespace AnluMenu
{
    /// <summary>
    /// Centralized PlayerPrefs key constants used by the built-in settings modules.
    /// Keep all keys here so they are easy to find, search, and avoid collisions.
    /// </summary>
    public static class PlayerPrefsKeys
    {
        // ── Audio ──────────────────────────────────────────────
        public const string MasterVolume   = "menu.audio.master";
        public const string MusicVolume    = "menu.audio.music";
        public const string SfxVolume      = "menu.audio.sfx";
        public const string AmbienceVolume = "menu.audio.ambience";
        public const string DialogueVolume = "menu.audio.dialogue";

        // ── Display ────────────────────────────────────────────
        public const string Fullscreen = "menu.display.fullscreen";
        public const string VSync      = "menu.display.vsync";

        // ── Graphics ───────────────────────────────────────────
        public const string Resolution = "menu.graphics.resolution";
        public const string Quality    = "menu.graphics.quality";
    }
}
