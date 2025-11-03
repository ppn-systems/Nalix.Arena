using Nalix.Rendering.Attributes;
using System;

namespace Nalix.Launcher.Scenes.Menu.Login.Model;

[IgnoredLoad("RenderObject")]
internal sealed class LoginModel
{
    public Boolean IsBusy { get; set; }
    public DateTime LastSubmitAtUtc { get; set; } = DateTime.MinValue;

    // rate-limit: 2 yêu cầu / 3s
    private readonly System.Collections.Generic.Queue<DateTime> _sendTimes = new();

    public Boolean AllowSend()
    {
        var now = DateTime.UtcNow;
        while (_sendTimes.Count > 0 && (now - _sendTimes.Peek()).TotalSeconds > 3)
        {
            _sendTimes.Dequeue();
        }

        if (_sendTimes.Count >= 2)
        {
            return false;
        }

        _sendTimes.Enqueue(now);
        return true;
    }
}
