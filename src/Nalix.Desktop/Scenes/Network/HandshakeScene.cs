// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Logging.Abstractions;
using Nalix.Common.Packets.Abstractions;
using Nalix.Communication.Enums;
using Nalix.Communication.Extensions; // CommandExtensions
using Nalix.Desktop.Enums;
using Nalix.Desktop.Objects.Indicators;
using Nalix.Desktop.Objects.Notifications;
using Nalix.Rendering.Attributes;
using Nalix.Rendering.Objects;
using Nalix.Rendering.Scenes;
using Nalix.SDK.Remote;
using Nalix.SDK.Remote.Extensions;       // HandshakeExtensions
using Nalix.Shared.Injection;
using SFML.Graphics;

namespace Nalix.Desktop.Scenes.Network;

/// <summary>
/// Scene responsible for performing a secure cryptographic handshake after TCP connect,
/// then transitioning to the next scene upon success.
/// </summary>
public sealed class HandshakeScene : Scene
{
    /// <summary>
    /// Initializes a new instance of <see cref="HandshakeScene"/> with name <see cref="SceneNames.Handshake"/>.
    /// </summary>
    public HandshakeScene() : base(SceneNames.Handshake) { }

    /// <summary>
    /// Load visual indicator(s) and the internal handler that performs the handshake.
    /// </summary>
    protected override void LoadObjects()
    {
        AddObject(new LoadingSpinner());
        AddObject(new HandshakeHandler());
        AddObject(new Notification(Text.Start, Side.Top));
    }

    /// <summary>
    /// Text resources for user-facing messages (ready for localization).
    /// </summary>
    private static class Text
    {
        public const System.String Start = "Securing connection…";
        public const System.String Initiating = "Handshake: sending client public key";
        public const System.String Waiting = "Handshake: awaiting server response";
        public const System.String Installing = "Handshake: installing encryption key";
        public const System.String Success = "Handshake successful";
        public const System.String Failed = "Handshake failed";
        public const System.String Hint = "Please try again or check the server status";
        public const System.String Retrying = "Retrying handshake in {0} s (attempt {1}/{2})";
    }

    [IgnoredLoad("RenderObject")]
    private sealed class HandshakeHandler : RenderObject
    {
        // ---- Configuration ---------------------------------------------------
        private const System.Single RetryDelaySec = 3f;
        private const System.Int32 MaxAttempts = 3;
        private const System.String NextScene = SceneNames.Login;
        // ---------------------------------------------------------------------

        private enum State { Idle, Running, Success, Failed, WaitingRetry, ShowFail, Done }

        private System.Int32 _attempt;
        private System.Single _timer;
        private State _state;

        private System.Threading.CancellationTokenSource _cts;
        private System.Threading.Tasks.Task _task;

        public HandshakeHandler()
        {
            _attempt = 0;
            _timer = 0f;
            _state = State.Idle;
        }

        /// <summary>
        /// Force immediate retry from outside (optional UX hook).
        /// </summary>
        public void ForceRetryNow()
        {
            if (_state == State.WaitingRetry)
            {
                _timer = RetryDelaySec;
            }
        }

        public override void Update(System.Single dt)
        {
            _timer += dt;

            switch (_state)
            {
                case State.Idle:
                    {
                        // Start first attempt immediately
                        _attempt = 1;
                        SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Initiating);
                        StartHandshake();
                        _state = State.Running;
                        _timer = 0f;
                        break;
                    }

                case State.Running:
                    {
                        if (_task is null)
                        {
                            break;
                        }

                        if (_task.IsFaulted)
                        {
                            CleanupTask();

                            if (_attempt >= MaxAttempts)
                            {
                                // Final failure
                                SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Failed);
                                _state = State.Failed;
                                _timer = 0f;
                            }
                            else
                            {
                                // Prepare a retry cycle
                                _state = State.WaitingRetry;
                                _timer = 0f;
                                SceneManager.FindByType<Notification>()
                                    ?.UpdateMessage(System.String.Format(Text.Retrying, (System.Int32)RetryDelaySec, _attempt, MaxAttempts));
                            }
                        }
                        else if (_task.IsCompletedSuccessfully)
                        {
                            CleanupTask();
                            SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Success);
                            _state = State.Success;
                        }
                        break;
                    }

                case State.WaitingRetry:
                    {
                        // Show countdown while waiting
                        var remain = System.Math.Max(0, (System.Int32)System.Math.Ceiling(RetryDelaySec - _timer));
                        SceneManager.FindByType<Notification>()
                            ?.UpdateMessage(System.String.Format(Text.Retrying, remain, _attempt, MaxAttempts));

                        if (_timer >= RetryDelaySec)
                        {
                            _attempt++;
                            SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Initiating);
                            StartHandshake();
                            _state = State.Running;
                            _timer = 0f;
                        }
                        break;
                    }

                case State.Success:
                    {
                        // Proceed to next scene
                        SceneManager.QueueDestroy(this);
                        SceneManager.ChangeScene(NextScene);
                        _state = State.Done;
                        break;
                    }

                case State.Failed:
                    {
                        // Replace banner with a blocking dialog
                        SceneManager.FindByType<Notification>()?.Destroy();
                        ShowFinalFailureBox();
                        _state = State.ShowFail;
                        break;
                    }

                case State.ShowFail:
                case State.Done:
                default:
                    break;
            }
        }

        public override void BeforeDestroy() => CleanupTask();

        public override void Render(RenderTarget target) { }
        protected override Drawable GetDrawable() => null;

        // ---------------------------------------------------------------------
        // Handshake flow
        // ---------------------------------------------------------------------
        private void StartHandshake()
        {
            CleanupTask();
            _cts = new System.Threading.CancellationTokenSource();

            _task = System.Threading.Tasks.Task.Run(async () =>
            {
                var logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

                try
                {
                    var client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();
                    if (!client.IsConnected)
                    {
                        throw new System.InvalidOperationException("Client is not connected.");
                    }

                    // 1) Initiate: send client public key
                    SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Initiating);
                    var kp = await client.InitiateHandshakeAsync(OpCommand.HANDSHAKE.AsUInt16(), _cts.Token).ConfigureAwait(false);

                    // 2) Wait for server's Handshake packet
                    SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Waiting);
                    IPacket serverPacket = await client.ReceiveAsync(_cts.Token).ConfigureAwait(false);

                    // 3) Install encryption key
                    SceneManager.FindByType<Notification>()?.UpdateMessage(Text.Installing);
                    System.Boolean ok = client.FinishHandshake(kp, serverPacket);
                    if (!ok)
                    {
                        throw new System.InvalidOperationException("Unexpected packet received during handshake.");
                    }

                    // Success
                    logger?.Info("Handshake succeeded. Session encryption key is installed.");
                }
                catch (System.OperationCanceledException ocex)
                {
                    // Treat as failure in this flow (we will retry if attempts left)
                    logger?.Warn("Handshake canceled or timed out.", ocex);
                    throw;
                }
                catch (System.Exception ex)
                {
                    logger?.Error("Handshake error.", ex);
                    throw;
                }
            }, _cts.Token);
        }

        private void CleanupTask()
        {
            try { _cts?.Cancel(); } catch { /* ignored */ }
            _cts?.Dispose();
            _cts = null;
            _task = null;
        }

        // ---------------------------------------------------------------------
        // Final failure dialog (non-recoverable UX)
        // ---------------------------------------------------------------------
        private static void ShowFinalFailureBox()
        {
            var box = new ActionNotification($"{Text.Failed}\n{Text.Hint}")
            {
                ButtonExtraOffsetY = 20f,
            };

            // Primary action: Retry immediately from a clean state (reload scene)
            box.RegisterAction(() =>
            {
                box.Destroy();
                SceneManager.ChangeScene(SceneNames.Handshake);
            });

            // Optional: add a secondary "Exit" if your ActionNotification supports multi-buttons.
            // box.RegisterSecondaryAction("Exit", () => System.Environment.Exit(0));

            box.Spawn();
        }
    }
}
