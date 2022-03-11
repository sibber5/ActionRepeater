using System.Text;

namespace ActionRepeater.Extentions;

public static class ExtentionMethods
{
    public static string WithSpacesBetweenWords(this string str)
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
}
