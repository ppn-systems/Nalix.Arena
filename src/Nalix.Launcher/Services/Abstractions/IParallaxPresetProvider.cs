using Nalix.Portal.Services.Dtos;

namespace Nalix.Portal.Services.Abstractions;

public interface IParallaxPresetProvider
{
    ParallaxPreset GetByVariant(System.Int32 variant);
}
