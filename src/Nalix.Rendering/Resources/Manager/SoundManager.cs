using SFML.Audio;
using System;

namespace Nalix.Rendering.Resources.Manager;

/// <summary>
/// Represents a single Sound Effect
/// </summary>
public class SoundManager : IDisposable
{
    private SoundBuffer _Buffer;
    private Sound[] _Sounds;

    /// <summary>
    /// Gets the name of this <see cref="SoundManager"/>.
    /// </summary>
    public String Name { get; }

    /// <summary>
    /// Determines whether this <see cref="SoundManager"/> has been disposed.
    /// </summary>
    public Boolean Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundManager" /> class.
    /// </summary>
    /// <param name="name">The sounds name</param>
    /// <param name="soundBuffer">Sound buffer containing the audio data to play with the sound instance</param>
    /// <param name="parallelSounds">The maximum number of parallel playing sounds.</param>
    public SoundManager(String name, SoundBuffer soundBuffer, Int32 parallelSounds)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Invalid {nameof(name)}:{name}");
        }

        Name = name;
        _Buffer = soundBuffer ?? throw new ArgumentNullException(nameof(soundBuffer));
        _Sounds = new Sound[Math.Clamp(parallelSounds, 1, 25)];
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SoundManager" /> class.
    /// </summary>
    ~SoundManager()
    {
        Dispose(false);
    }

    /// <summary>
    /// Retrieves a sound when available. The amount of sounds per frame is limited.
    /// </summary>
    /// <returns>The sound instance or null when too many instances of the same sound are already active</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Sound GetSound()
    {
        ObjectDisposedException.ThrowIf(Disposed, Name);

        for (Int32 i = 0; i < _Sounds.Length; i++)
        {
            var sound = _Sounds[i];
            if (sound == null)
            {
                _Sounds[i] = sound = new Sound(_Buffer);
            }

            if (sound.Status != SoundStatus.Stopped)
            {
                continue;
            }

            return sound;
        }
        return null; // when all sounds are busy none shall be added
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual void Dispose(Boolean disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                for (Int32 i = 0; i < _Sounds.Length; i++)
                {
                    if (_Sounds[i] != null)
                    {
                        _Sounds[i].Dispose();
                        _Sounds[i] = null;
                    }
                }
            }
            _Sounds = null;
            _Buffer = null;
            Disposed = true;
        }
    }
}
