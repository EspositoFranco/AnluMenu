namespace AnluMenu
{
    /// <summary>
    /// No-op <see cref="IUIAudio"/> used as the default when no provider is wired.
    /// Lets the menu run silently in tests, prototypes, or projects without audio yet.
    /// </summary>
    public sealed class NullUIAudio : IUIAudio
    {
        public void PlayHover() { }
        public void PlayClick() { }
        public void PlaySlider() { }
        public void SetVolume(VolumeChannel channel, float linearValue) { }
    }
}
