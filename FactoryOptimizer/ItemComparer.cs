namespace FactoryOptimizer;

public sealed class ItemComparer : IEqualityComparer<Item>
{
    public static readonly ItemComparer Instance = new ();

    public bool Equals(Item x, Item y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode(Item obj)
    {
        return obj.Id;
    }
}