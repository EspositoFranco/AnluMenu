using UnityEngine;
using UnityEngine.UI;

namespace AnluMenu
{
    /// <summary>
    /// Audio settings module: up to 5 volume sliders (master / music / sfx / ambience / dialogue).
    /// Saves to <see cref="ISettingsStorage"/> and forwards changes to the registered <see cref="IUIAudio"/>.
    /// Until you wire an audio provider, the module just persists values silently.
    /// </summary>
    public class AudioSettingsModule : MonoBehaviour, ISettingsModule
    {
        [Header("Sliders (assign only what your UI exposes)")]
        [Tooltip("Master volume. Leave empty if not used.")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _ambienceSlider;
        [SerializeField] private Slider _dialogueSlider;

        [Header("Audio Output")]
        [Tooltip("MonoBehaviour implementing IUIAudio. Leave empty for persistence-only mode.")]
        [SerializeField] private MonoBehaviour _audioProvider;

        private IUIAudio _audio = new NullUIAudio();
        private ISettingsStorage _storage;

        public void Initialize(ISettingsStorage storage)
        {
            _storage = storage;

            if (_audioProvider is IUIAudio a)
            {
                _audio = a;
            }
            else
            {
                // _audioProvider not wired — happens when the IUIAudio lives on a DontDestroyOnLoad
                // object from another scene (can't be serialized as a cross-scene reference).
                // Search all MonoBehaviours at runtime so Game scene settings panels work too.
                foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                    if (mb is IUIAudio runtime) { _audio = runtime; break; }
            }

            Bind(_masterSlider,   PlayerPrefsKeys.MasterVolume,   VolumeChannel.Master);
            Bind(_musicSlider,    PlayerPrefsKeys.MusicVolume,    VolumeChannel.Music);
            Bind(_sfxSlider,      PlayerPrefsKeys.SfxVolume,      VolumeChannel.Sfx);
            Bind(_ambienceSlider, PlayerPrefsKeys.AmbienceVolume, VolumeChannel.Ambience);
            Bind(_dialogueSlider, PlayerPrefsKeys.DialogueVolume, VolumeChannel.Dialogue);
        }

        // Values are persisted as the user drags the slider, so Apply() is a no-op.
        public void Apply() { }

        public void ResetToDefaults()
        {
            ResetSlider(_masterSlider,   PlayerPrefsKeys.MasterVolume,   VolumeChannel.Master);
            ResetSlider(_musicSlider,    PlayerPrefsKeys.MusicVolume,    VolumeChannel.Music);
            ResetSlider(_sfxSlider,      PlayerPrefsKeys.SfxVolume,      VolumeChannel.Sfx);
            ResetSlider(_ambienceSlider, PlayerPrefsKeys.AmbienceVolume, VolumeChannel.Ambience);
            ResetSlider(_dialogueSlider, PlayerPrefsKeys.DialogueVolume, VolumeChannel.Dialogue);
        }

        private void Bind(Slider slider, string key, VolumeChannel channel)
        {
            if (slider == null) return;
            float saved = _storage.GetFloat(key, 1f);
            slider.SetValueWithoutNotify(saved);
            _audio.SetVolume(channel, saved);
            slider.onValueChanged.AddListener(v =>
            {
                _storage.SetFloat(key, v);
                _audio.SetVolume(channel, v);
            });
        }

        private void ResetSlider(Slider slider, string key, VolumeChannel channel)
        {
            if (slider == null) return;
            slider.value = 1f;
            _storage.SetFloat(key, 1f);
            _audio.SetVolume(channel, 1f);
        }
    }
}
