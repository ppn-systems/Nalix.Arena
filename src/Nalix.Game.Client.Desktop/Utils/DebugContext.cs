using System;

namespace Nalix.Game.Client.Desktop.Utils;

public class DebugContext
{
    private const string FpsTextFormat = "FPS: {0:0}";
    private const string ElapsedTimeFormat = "Time: {0:0.00} s";
    private const string MemoryUsageFormat = "Memory: {0:0.00} MB";

    private int _frameCount = 0;
    private float _currentFps = 0f;
    private float _fpsUpdateTimer = 0f;
    private float _totalTime = 0f;

    private float _memoryUsage = 0f;
    private float _memoryUpdateTimer = 0f;
    private readonly float _memoryUpdateInterval = 5f; // Cập nhật mỗi 5 giây

    public void Update(float deltaTime)
    {
        _frameCount++;

        _totalTime += deltaTime;
        _fpsUpdateTimer += deltaTime;
        _memoryUpdateTimer += deltaTime;

        if (_fpsUpdateTimer >= 1.0f)
        {
            _currentFps = _frameCount / _fpsUpdateTimer;
            _frameCount = 0;
            _fpsUpdateTimer = 0f;
        }

        if (_memoryUpdateTimer >= _memoryUpdateInterval)
        {
            _memoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f);
            _memoryUpdateTimer = 0f;
        }
    }

    public string GetFpsText() => string.Format(FpsTextFormat, _currentFps);

    public string GetElapsedTimeText() => string.Format(ElapsedTimeFormat, _totalTime);

    public string GetMemoryUsageText() => string.Format(MemoryUsageFormat, _memoryUsage);
}