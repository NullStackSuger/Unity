namespace ET;

public class TypeArrayComparer : IEqualityComparer<Type[]>
{
    public bool Equals(Type[] x, Type[] y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null || x.Length != y.Length) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(Type[] obj)
    {
        var hash = new HashCode();
        foreach (var type in obj)
        {
            hash.Add(type);
        }
        return hash.ToHashCode();
    }
}