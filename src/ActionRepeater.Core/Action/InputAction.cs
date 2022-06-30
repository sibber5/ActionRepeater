using System;

namespace ActionRepeater.Core.Action;

public abstract class InputAction
{
    public event EventHandler<string>? DescriptionChanged;
    protected void OnDescriptionChanged() => DescriptionChanged?.Invoke(this, Description);

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual InputAction Clone() => (InputAction)MemberwiseClone();

    public abstract void Play();
}
