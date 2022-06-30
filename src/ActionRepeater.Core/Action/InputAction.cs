using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActionRepeater.Core.Action;

public abstract class InputAction : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual InputAction Clone() => (InputAction)MemberwiseClone();

    public abstract void Play();
}
