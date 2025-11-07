using Nalix.Common.Protocols;
using Nalix.Communication.Enums;
using Nalix.Launcher.Scenes.Menu.Login.View;
using Nalix.Launcher.Scenes.Shared.Controller;
using Nalix.Launcher.Services.Abstractions;

namespace Nalix.Launcher.Scenes.Menu.Login.Controller;

internal sealed class LoginSceneController(
    LoginView view,
    IThemeProvider theme,
    ISceneNavigator nav,
    IParallaxPresetProvider parallaxPreset) : CredentialsSceneController<LoginView>(view, theme, nav, parallaxPreset)
{
    protected override OpCommand Command => OpCommand.LOGIN;
    protected override System.String SuccessMessage => "Welcome!";

    protected override System.String MapErrorMessage(ProtocolCode code) => code switch
    {
        ProtocolCode.UNAUTHENTICATED => "Invalid username or password.",
        ProtocolCode.ACCOUNT_LOCKED => "Too many failed attempts. Please wait and try again.",
        ProtocolCode.ACCOUNT_SUSPENDED => "Your account is suspended.",
        ProtocolCode.VALIDATION_FAILED => "Please fill both username and password.",
        ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch.",
        ProtocolCode.CANCELLED => "Login cancelled.",
        ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
        _ => "Login failed."
    };
}
