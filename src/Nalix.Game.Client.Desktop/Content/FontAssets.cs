using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace Nalix.Game.Client.Desktop.Content
{
    internal static class FontAssets
    {
        // Base path for fonts
        private static readonly string FontPath = "assets/fonts/{0}.ttf";

        // A dictionary to cache the loaded fonts, avoiding reloading them each time
        private static readonly Dictionary<string, Font> _fontCache = [];

        // Default font (JetBrainsMono)
        public static Font Default => GetFont("JetBrainsMono");

        // Pixel font (ThaleahFat)
        public static Font Pixel => GetFont("ThaleahFat");

        // Method to load the font from the path
        private static Font GetFont(string fontName)
        {
            if (!_fontCache.ContainsKey(fontName))
            {
                try
                {
                    // Load the font and add it to the cache
                    string fontPath = string.Format(FontPath, fontName);
                    Font font = new(fontPath);
                    _fontCache[fontName] = font;
                }
                catch (Exception ex)
                {
                    // Handle any font loading errors (e.g., file not found)
                    Console.WriteLine($"Error loading font '{fontName}': {ex.Message}");
                    return null; // Return null if the font fails to load
                }
            }
            return _fontCache[fontName];
        }
    }
}