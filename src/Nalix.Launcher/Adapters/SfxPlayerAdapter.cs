using Nalix.Launcher.Services.Abstractions;

namespace Nalix.Launcher.Adapters;

internal sealed class SfxPlayerAdapter : ISfxPlayer
{
    public void Play(System.String sfxId) => Assets.Sfx.Play(sfxId);
}
