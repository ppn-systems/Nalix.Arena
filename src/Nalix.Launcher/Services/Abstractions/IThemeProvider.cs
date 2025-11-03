using Nalix.Launcher.Services.Dtos;

namespace Nalix.Launcher.Services.Abstractions;

// Cấp theme thuần DTO
public interface IThemeProvider
{
    ThemeDto Current { get; }
}
