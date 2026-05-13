using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnluMenu
{
    /// <summary>
    /// Resolution settings module: populates a TMP_Dropdown with <see cref="Screen.resolutions"/>,
    /// filters duplicates, and applies via <see cref="Screen.SetResolution"/>.
    /// </summary>
    public class ResolutionSettingsModule : MonoBehaviour, ISettingsModule
    {
        [Tooltip("Dropdown for resolution selection.")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        private ISettingsStorage _storage;
        private readonly List<Resolution> _resolutions = new();

        public void Initialize(ISettingsStorage storage)
        {
            _storage = storage;
            if (_resolutionDropdown == null) return;

            PopulateResolutions();

            int saved = _storage.GetInt(PlayerPrefsKeys.Resolution, GetCurrentResolutionIndex());
            if (saved >= _resolutions.Count) saved = _resolutions.Count - 1;
            _resolutionDropdown.SetValueWithoutNotify(saved);
            _resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        public void Apply()
        {
            if (_resolutionDropdown != null) SetResolution(_resolutionDropdown.value);
        }

        public void ResetToDefaults()
        {
            int defaultIndex = _resolutions.Count - 1;
            if (_resolutionDropdown != null) _resolutionDropdown.value = defaultIndex;
            SetResolution(defaultIndex);
        }

        private void PopulateResolutions()
        {
            _resolutions.Clear();
            var options = new List<string>();
            var seen = new HashSet<string>();

            foreach (var res in Screen.resolutions)
            {
                string key = $"{res.width}x{res.height}";
                if (!seen.Add(key)) continue;
                _resolutions.Add(res);
                options.Add(key);
            }

            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(options);
        }

        private int GetCurrentResolutionIndex()
        {
            for (int i = 0; i < _resolutions.Count; i++)
            {
                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                    return i;
            }
            return _resolutions.Count - 1;
        }

        private void SetResolution(int index)
        {
            if (index < 0 || index >= _resolutions.Count) return;
            var res = _resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            _storage.SetInt(PlayerPrefsKeys.Resolution, index);
        }
    }
}
