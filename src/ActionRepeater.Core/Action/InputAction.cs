using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ActionRepeater.Core.Action;

[XmlInclude(typeof(KeyAction))]
[XmlInclude(typeof(MouseButtonAction))]
[XmlInclude(typeof(MouseWheelAction))]
[XmlInclude(typeof(WaitAction))]
public abstract class InputAction : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual InputAction Clone() => (InputAction)this.MemberwiseClone();

    public abstract void Play();
}
