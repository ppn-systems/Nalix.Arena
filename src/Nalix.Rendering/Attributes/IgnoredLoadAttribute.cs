namespace Nalix.Rendering.Attributes;

/// <summary>
/// Attribute to indicate that a class should be skipped during loading.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IgnoredLoadAttribute"/> class.
/// </remarks>
/// <param name="reason">The reason for skipping loading.</param>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public class IgnoredLoadAttribute(System.String reason) : System.Attribute
{
    /// <summary>
    /// Gets the reason why loading is skipped.
    /// </summary>+
    public System.String Reason { get; } = reason;
}
