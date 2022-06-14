using System.Text;

namespace ActionRepeater.Core.Extentions;

public static class ExtentionMethods
{
    public static string AddSpacesBetweenWords(this string str)
    {
        StringBuilder sb = new();
        for (int i = 1; i < str.Length; ++i)
        {
            sb.Append(str[i - 1]);
            if (char.IsUpper(str[i]))
            {
                sb.Append(' ');
            }
        }
        sb.Append(str[^1]);
        return sb.ToString();
    }

    public static bool ContainsInclusive(this System.Drawing.Rectangle rect, int x, int y)
    {
        return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
    }
}
