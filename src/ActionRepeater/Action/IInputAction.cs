namespace ActionRepeater.Action;

public interface IInputAction : System.ComponentModel.INotifyPropertyChanged
{
    public string Name { get; }
    public string Description { get; }

    public void Play();
    public IInputAction Clone();
}
