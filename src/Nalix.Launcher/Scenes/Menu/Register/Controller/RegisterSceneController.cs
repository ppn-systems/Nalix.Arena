using Nalix.Common.Protocols;
using Nalix.Communication.Enums;
using Nalix.Portal.Scenes.Menu.Register.View;
using Nalix.Portal.Scenes.Shared.Controller;
using Nalix.Portal.Services.Abstractions;

namespace Nalix.Portal.Scenes.Menu.Register.Controller;

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

    protected override System.String MapErrorMessage(ProtocolCode code) => code switch
    {
        ProtocolCode.UNSUPPORTED_PACKET => "Client/server version mismatch detected.",
        ProtocolCode.INVALID_USERNAME => "Invalid credentials.",
        ProtocolCode.WEAK_PASSWORD => "Password is too weak. Choose a stronger one.",
        ProtocolCode.ALREADY_EXISTS => "Unable to complete registration.",
        ProtocolCode.VALIDATION_FAILED => "Please verify all fields are correct.",
        ProtocolCode.INTERNAL_ERROR => "Server error. Please try again later.",
        ProtocolCode.NONE => "None",
        _ => "Registration failed. Try again later."
    };
}
