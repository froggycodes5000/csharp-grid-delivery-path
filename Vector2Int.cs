using System.Numerics;

public class Vector2Int
{
    private Vector2 v;
    public int x
    {
        get => (int)(v.X);
    }
    public int y
    {
        get => (int)(v.Y);
    }

    public Vector2Int(int x, int y)
    {
        this.v = new Vector2(x, y);
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }

    public override bool Equals(object? obj)
    {
        Vector2Int? other = obj as Vector2Int;
        if (other == null)
        {
            return false;
        }
        return v.Equals(other.v);
    }

    public override int GetHashCode()
    {
        return v.GetHashCode();
    }

    public static float Distance(Vector2Int a, Vector2Int b) {
        return Vector2.Distance(a.v, b.v);
    }
}
