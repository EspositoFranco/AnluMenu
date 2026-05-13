namespace AnluMenu
{
    /// <summary>
    /// Logical volume channels exposed to the menu. Implementations map these to
    /// AudioMixer groups, MMSoundManager tracks, or any other audio backend.
    /// </summary>
    public enum VolumeChannel
    {
        Master,
        Music,
        Sfx,
        Ambience,
        Dialogue
    }

    /// <summary>
    /// Audio integration contract. The Menu package never references a concrete
    /// audio system — implement this interface in your project (or in a separate
    /// adapter package) to bridge the menu to MMSoundManager, AudioMixer, FMOD, etc.
    /// </summary>
    /// <remarks>
    /// Implement on a MonoBehaviour and assign it to MenuController.AudioProvider
    /// in the Inspector. The default <see cref="NullUIAudio"/> is silent.
    /// </remarks>
    public interface IUIAudio
    {
        /// <summary>Played when a button is hovered.</summary>
        void PlayHover();

        /// <summary>Played when a button is clicked or a panel changes.</summary>
        void PlayClick();

        /// <summary>Played when a slider value changes (UI tick).</summary>
        void PlaySlider();

        /// <summary>Sets the volume of a logical channel. <paramref name="linearValue"/> is in [0..1].</summary>
        void SetVolume(VolumeChannel channel, float linearValue);
    }
}
