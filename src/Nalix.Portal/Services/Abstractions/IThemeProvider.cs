using Nalix.Portal.Services.Dtos;

namespace Nalix.Portal.Services.Abstractions;

// Cấp theme thuần DTO
public interface IThemeProvider
{
    ThemeDto Current { get; }
}
