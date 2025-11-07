// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Portal.Scenes.Shared.View;

/// <summary>
/// Minimal contract a credentials view must provide (Login/Register).
/// </summary>
internal interface ICredentialsView
{
    // Events
    event System.Action SubmitRequested;
    event System.Action BackRequested;
    event System.Action<System.Boolean> TabToggled;

    // Data
    System.String Username { get; }
    System.String Password { get; }

    // UI actions
    void FocusPass();
    void LockUi(System.Boolean on);
    void ShowWarning(System.String message);

    // Keyboard hooks
    void OnEnter();
    void OnEscape();
    void OnTab();
    void OnTogglePassword();
}
