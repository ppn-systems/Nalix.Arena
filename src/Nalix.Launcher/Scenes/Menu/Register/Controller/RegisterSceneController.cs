using Nalix.Common.Protocols;
using Nalix.Communication.Enums;
using Nalix.Launcher.Scenes.Menu.Register.View;
using Nalix.Launcher.Scenes.Shared.Controller;
using Nalix.Launcher.Services.Abstractions;

namespace Nalix.Launcher.Scenes.Menu.Register.Controller;

internal sealed class RegisterSceneController
    : CredentialsSceneController<RegisterView>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public RegisterSceneController(
        RegisterView view,
        IThemeProvider theme,
        ISceneNavigator nav,
        IParallaxPresetProvider parallaxPreset)
        : base(view, theme, nav, parallaxPreset)
    {
    }

    protected override OpCommand Command => OpCommand.REGISTER;
    protected override System.String SuccessMessage => "Welcome!";

    protected override System.String MapErrorMessage(ProtocolCode code) => code switch
    {
        ProtocolCode.UNAUTHENTICATED => "Invalid username or password.",
        ProtocolCode.ACCOUNT_LOCKED => "Too many failed attempts. Please wait and try again.",
        ProtocolCode.ACCOUNT_SUSPENDED => "Your account is suspended.",
        ProtocolCode.VALIDATION_FAILED => "Please check your username and password again.",
        ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch.",
        ProtocolCode.CANCELLED => "Register cancelled.",
        ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
        _ => "Register failed."
    };
}
