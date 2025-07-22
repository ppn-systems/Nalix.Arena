using SFML.Graphics;

namespace Nalix.Rendering.Resources;

/// <summary>
/// Texture management class. Handles loading/unloading of unmanaged Texture resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the TextureLoader class.
/// </remarks>
/// <param name="assetRoot">Optional root path of the managed asset folder</param>
/// <param name="repeat">Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension</param>
/// <param name="smoothing">Determines if a smoothing should be applied onto newly loaded Textures</param>
public sealed class TextureLoader(System.String assetRoot = "", System.Boolean repeat = false, System.Boolean smoothing = false)
    : AssetLoader<Texture>(AvailableFormats, assetRoot)
{
    /// <summary>
    /// List of supported file endings for this TextureLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> AvailableFormats =
    [
        ".bmp", ".png", ".tga", ".jpg",
        ".gif", ".psd", ".hdr", ".pic"
    ];

    /// <summary>
    /// Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension.
    /// </summary>
    public System.Boolean Repeat { get; set; } = repeat;

    /// <summary>
    /// Determines if a smoothing should be applied onto newly loaded Textures.
    /// </summary>
    public System.Boolean Smoothing { get; set; } = smoothing;

    /// <summary>
    /// Loads or retrieves an already loaded instance of a Texture from a File or Raw Data Source
    /// </summary>
    /// <param name="name">Name of the Texture</param>
    /// <param name="rawData">Optional byte array containing the raw data of the Texture</param>
    /// <returns>The managed Texture</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override Texture Load(System.String name, System.Byte[] rawData = null)
        => Load(name, Repeat, Smoothing, rawData);

    /// <summary>Loads or retrieves an already loaded instance of a Texture from a File or Raw Data Source</summary>
    /// <param name="name">Name of the Texture</param>
    /// <param name="repeat">Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension.</param>
    /// <param name="smoothing">Determines if a smoothing should be applied onto newly loaded Textures.</param>
    /// <param name="rawData">Optional byte array containing the raw data of the Texture</param>
    /// <returns>The managed Texture</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Texture Load(
        System.String name, System.Boolean? repeat = null,
        System.Boolean? smoothing = null, System.Byte[] rawData = null)
    {
        Texture tex = base.Load(name, rawData);
        if (tex != null)
        {
            tex.Repeated = repeat ?? Repeat;
            tex.Smooth = smoothing ?? Smoothing;
        }
        return tex;
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override Texture CreateInstanceFromRawData(System.Byte[] rawData)
    {
        using System.IO.MemoryStream ms = new(rawData);
        Texture texture = new(ms); // Pass the MemoryStream to the constructor
        return texture;
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override Texture CreateInstanceFromPath(System.String path)
    {
        using var fs = System.IO.File.OpenRead(path);
        Texture texture = new(fs); // Pass the FileStream to the constructor
        return texture;
    }
}
