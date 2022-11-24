using System;
using System.Text.Json.Serialization;

namespace ActionRepeater.Core.Action;

[JsonDerivedType(typeof(KeyAction), nameof(KeyAction))]
[JsonDerivedType(typeof(MouseButtonAction), nameof(MouseButtonAction))]
[JsonDerivedType(typeof(MouseWheelAction), nameof(MouseWheelAction))]
[JsonDerivedType(typeof(WaitAction), nameof(WaitAction))]
public abstract class InputAction
{
    public event EventHandler<string>? DescriptionChanged;
    protected void OnDescriptionChanged() => DescriptionChanged?.Invoke(this, Description);

    public event EventHandler<string>? NameChanged;
    protected void OnNameChanged() => NameChanged?.Invoke(this, Name);

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual InputAction Clone() => (InputAction)MemberwiseClone();

    public abstract void Play();
}
