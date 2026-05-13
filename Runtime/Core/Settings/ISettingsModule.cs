namespace AnluMenu
{
    /// <summary>
    /// Extensibility contract for settings. Add a new settings group (graphics,
    /// controls, accessibility, ...) by implementing this on a MonoBehaviour and
    /// dropping it as a child of the SettingsController GameObject.
    /// </summary>
    /// <remarks>
    /// SettingsController auto-discovers modules via GetComponentsInChildren.
    /// Storage is injected on <see cref="Initialize"/> — never call PlayerPrefs directly.
    /// </remarks>
    public interface ISettingsModule
    {
        /// <summary>Called once on Awake. Use <paramref name="storage"/> to read persisted values, wire UI, apply current state.</summary>
        void Initialize(ISettingsStorage storage);

        /// <summary>Forces a re-apply of the current values. Useful after a global reset.</summary>
        void Apply();

        /// <summary>Restores defaults and persists them.</summary>
        void ResetToDefaults();
    }
}
