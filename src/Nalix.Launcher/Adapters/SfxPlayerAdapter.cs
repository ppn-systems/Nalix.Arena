using Nalix.Portal;
using Nalix.Portal.Services.Abstractions;

namespace Nalix.Portal.Adapters;

internal sealed class SfxPlayerAdapter : ISfxPlayer
{
    public void Play(System.String sfxId) => Assets.Sfx.Play(sfxId);
}
