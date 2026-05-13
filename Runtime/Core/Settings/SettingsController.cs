using System.Collections.Generic;
using UnityEngine;


namespace AnluMenu
{
    /// <summary>
    /// Auto-discovers <see cref="ISettingsModule"/> components in its children and
    /// orchestrates Initialize / Apply / Reset across all of them.
    /// </summary>
    /// <remarks>
    /// Add modules as child GameObjects (or on the same GameObject) with a component
    /// implementing ISettingsModule. The controller picks them up on Awake — no manual
    /// registration needed. Storage defaults to <see cref="PlayerPrefsStorage"/> unless
    /// a custom <see cref="ISettingsStorage"/> MonoBehaviour is wired in the Inspector.
    /// </remarks>
    public class SettingsController : MonoBehaviour
    {
        [Header("Storage")]
        [Tooltip("Optional MonoBehaviour implementing ISettingsStorage. Defaults to PlayerPrefs.")]
        [SerializeField] private MonoBehaviour _storageProvider;

        /// <summary>Active settings storage. Defaults to <see cref="PlayerPrefsStorage"/>.</summary>
        public ISettingsStorage Storage { get; private set; } = new PlayerPrefsStorage();

        private readonly List<ISettingsModule> _modules = new();

        private void Awake()
        {
            if (_storageProvider is ISettingsStorage s) Storage = s;
            GetComponentsInChildren(true, _modules);
            foreach (var m in _modules) m.Initialize(Storage);
        }

        /// <summary>Replace the storage backend at runtime (e.g. after login to a cloud save service).</summary>
        public void SetStorage(ISettingsStorage storage) => Storage = storage ?? new PlayerPrefsStorage();

        /// <summary>Re-applies every module's current state and saves.</summary>
        public void ApplyAll()
        {
            foreach (var m in _modules) m.Apply();
            Storage.Save();
        }

        /// <summary>Resets every module to defaults and saves.</summary>
        public void ResetAll()
        {
            foreach (var m in _modules) m.ResetToDefaults();
            Storage.Save();
        }
    }
}
