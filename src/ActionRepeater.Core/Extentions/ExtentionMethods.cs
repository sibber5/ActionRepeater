using System.Collections.Generic;
using System.Linq;
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

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable is null || !enumerable.Any();
    }

    public static int RefIndexOfReverse<T>(this IReadOnlyList<T> list, T item)
    {
        for (int i = list.Count - 1; i > -1; i--)
        {
            if (ReferenceEquals(list[i], item))
            {
                return i;
            }
        }

        return -1;
    }
}
