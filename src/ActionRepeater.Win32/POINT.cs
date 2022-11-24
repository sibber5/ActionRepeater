using System;
using System.Diagnostics.CodeAnalysis;

namespace ActionRepeater.Win32;

/// <summary>The <typeparamref name="POINT"/> structure defines the <i>x</i>- and <i>y</i>-coordinates of a point.</summary>
/// <remarks>
/// <para>The <typeparamref name="POINT"/> structure is identical to the <a href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-pointl">POINTL</a> structure.</para>
/// <para><see href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-point">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
public struct POINT : IEquatable<POINT>
{
    /// <summary>Specifies the <i>x</i>-coordinate of the point.</summary>
    public int x;
    /// <summary>Specifies the <i>y</i>-coordinate of the point.</summary>
    public int y;

    public POINT(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator System.Drawing.Point(POINT p)
    {
        return new System.Drawing.Point(p.x, p.y);
    }

    public static implicit operator POINT(System.Drawing.Point p)
    {
        return new POINT(p.X, p.Y);
    }

    public static bool operator ==(POINT a, POINT b) => a.Equals(b);
    public static bool operator !=(POINT a, POINT b) => !a.Equals(b);

    public bool Equals(POINT other) => x == other.x && y == other.y;

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is POINT point)
        {
            return Equals(point);
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(x, y);

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}
