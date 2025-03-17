namespace ActionRepeater.Win32;

public struct MARGINS
{
    public int cxLeftWidth;

    public int cxRightWidth;

    public int cyTopHeight;

    public int cyBottomHeight;

    public MARGINS(int cxLeftWidth, int cxRightWidth, int cyTopHeight, int cyBottomHeight)
    {
        this.cxLeftWidth = cxLeftWidth;
        this.cxRightWidth = cxRightWidth;
        this.cyTopHeight = cyTopHeight;
        this.cyBottomHeight = cyBottomHeight;
    }
}
