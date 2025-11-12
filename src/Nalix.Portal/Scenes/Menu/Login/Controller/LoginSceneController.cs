using Nalix.Common.Protocols;
using Nalix.Protocol.Enums;
using Nalix.Portal.Scenes.Menu.Login.View;
using Nalix.Portal.Scenes.Shared.Controller;
using Nalix.Portal.Services.Abstractions;

namespace Nalix.Portal.Scenes.Menu.Login.Controller;

internal sealed class LoginSceneController(
    LoginView view,
    IThemeProvider theme,
    ISceneNavigator nav,
    IParallaxPresetProvider parallaxPreset) : CredentialsSceneController<LoginView>(view, theme, nav, parallaxPreset)
{
    protected override OpCommand Command => OpCommand.LOGIN;

    protected override System.String MapErrorMessage(ProtocolCode code) => code switch
    {
        ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch.",
        ProtocolCode.UNAUTHENTICATED => "Invalid username or password.",
        ProtocolCode.ACCOUNT_LOCKED => "Too many failed attempts. Please wait and try again.",
        ProtocolCode.ACCOUNT_SUSPENDED => "Your account is suspended.",
        ProtocolCode.VALIDATION_FAILED => "Please fill both username and password.",
        ProtocolCode.CANCELLED => "Login cancelled.",
        ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
        _ => "Login failed."
    };
}
