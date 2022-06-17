using System;
using ActionRepeater.Core.Action;

namespace ActionRepeater.UI.Utilities;

/// <summary>
/// Holds an <see cref="InputAction"/>.
/// </summary>
public class ActionHolder
{
    private InputAction? _action;
    public InputAction? Action
    {
        get => _action;
        set
        {
            _action = value;
            ActionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? ActionChanged;
}
