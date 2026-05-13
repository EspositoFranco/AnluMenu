using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnluMenu
{
    /// <summary>
    /// Quality settings module: populates a TMP_Dropdown with <see cref="QualitySettings.names"/>
    /// and applies via <see cref="QualitySettings.SetQualityLevel"/>.
    /// </summary>
    public class QualitySettingsModule : MonoBehaviour, ISettingsModule
    {
        [Tooltip("Dropdown for quality level selection.")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        private ISettingsStorage _storage;

        public void Initialize(ISettingsStorage storage)
        {
            _storage = storage;
            if (_qualityDropdown == null) return;

            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            int saved = _storage.GetInt(PlayerPrefsKeys.Quality, QualitySettings.GetQualityLevel());
            if (saved >= QualitySettings.names.Length) saved = QualitySettings.names.Length - 1;
            _qualityDropdown.SetValueWithoutNotify(saved);
            _qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        public void Apply()
        {
            if (_qualityDropdown != null) SetQuality(_qualityDropdown.value);
        }

        public void ResetToDefaults()
        {
            int defaultLevel = QualitySettings.names.Length / 2;
            if (_qualityDropdown != null) _qualityDropdown.value = defaultLevel;
            SetQuality(defaultLevel);
        }

        private void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level);
            _storage.SetInt(PlayerPrefsKeys.Quality, level);
        }
    }
}
