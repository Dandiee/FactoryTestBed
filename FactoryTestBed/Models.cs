using System.Diagnostics;

namespace FactoryTestBed;

public sealed class Data
{
    public IReadOnlyCollection<Item> Items { get; set; }
    public IReadOnlyDictionary<string, Item> ItemsByName { get; set; }
    public IReadOnlyCollection<Recipe> Recipes { get; set; }

    public Item GetItem(string name) => ItemsByName[name];
}

[DebuggerDisplay("{DisplayName}")]
public sealed class Item
{
    public string Name { get; set; }
    public string DisplayName { get; set; }

    public IReadOnlyCollection<Recipe> Recipes { get; set; }
}

[DebuggerDisplay("{DisplayName}")]
public sealed class Recipe
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public float Duration { get; set; }
    public IReadOnlyCollection<ItemRate> Ingredients { get; set; }
    public IReadOnlyCollection<ItemRate> Products { get; set; }
}

[DebuggerDisplay("{Item.DisplayName}, Amount={Amount}")]
public sealed class ItemRate
{
    public string Name { get; set; }
    public float Amount { get; set; }
    public Item Item { get; set; }

    public ItemRate() { }

    public ItemRate(Item item, float amount)
    {
        Name = item.Name;
        Amount = amount;
        Item = item;
    }
}