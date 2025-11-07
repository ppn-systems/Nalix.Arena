using Nalix.Rendering.Attributes;

namespace Nalix.Launcher.Scenes.Menu.Register.Model;

[IgnoredLoad("RenderObject")]
internal sealed class RegisterModel
{
    public System.Boolean IsBusy { get; set; }
    public System.DateTime LastSubmitAtUtc { get; set; } = System.DateTime.MinValue;

    // rate-limit: 2 yêu cầu / 3s
    private readonly System.Collections.Generic.Queue<System.DateTime> _sendTimes = new();

    public System.Boolean AllowSend()
    {
        System.DateTime now = System.DateTime.UtcNow;

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
