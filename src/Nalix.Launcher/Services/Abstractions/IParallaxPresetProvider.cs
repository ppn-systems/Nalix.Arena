using Nalix.Launcher.Services.Dtos;

namespace Nalix.Launcher.Services.Abstractions;

public interface IParallaxPresetProvider
{
    ParallaxPreset GetByVariant(System.Int32 variant);
}
