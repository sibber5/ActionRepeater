using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.CompilerServices;

namespace ActionRepeater.Action;

public abstract class InputAction : INotifyPropertyChanged, System.Xml.Serialization.IXmlSerializable
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract InputAction Clone();

    public abstract void Play();

    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader) => throw new System.NotImplementedException("This shouldn't be used. Use the custom parser.");

    public abstract void WriteXml(XmlWriter writer);

    //public abstract System.Threading.Tasks.Task<InputAction> CreateActionFromXmlAsync(XmlReader reader);

    /// <summary>This is intended for serialization only.</summary>
    protected InputAction() { }
}
