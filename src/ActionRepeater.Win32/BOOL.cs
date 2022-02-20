namespace ActionRepeater.Win32;

public readonly struct BOOL
{
	private readonly int value;

	public int Value => this.value;
	public unsafe BOOL(bool value) => this.value = *(sbyte*)&value;
	public BOOL(int value) => this.value = value;
	public static unsafe implicit operator bool(BOOL value)
	{
		sbyte v = checked((sbyte)value.value);
		return *(bool*)&v;
	}
	public static implicit operator BOOL(bool value) => new BOOL(value);
	public static explicit operator BOOL(int value) => new BOOL(value);
}
