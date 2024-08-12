using System.Collections;
using System.Diagnostics;
using System.Text.Json;

namespace FactoryOptimizer;

public static class IdProvider
{
    private static int _current;

    public static int Next()
    {
        Interlocked.Increment(ref _current);
        return _current;
    }

}

[DebuggerDisplay("{DisplayName}")]
public class Item
{
    public int Id { get; }
    public string ClassName { get; set; }
    public string DisplayName { get; set; }
    public IReadOnlyList<SingleRecipe> Recipes { get; set; }
    public IReadOnlyList<SingleRecipe> AlternateRecipes { get; set; }
    public SingleRecipe MainRecipe { get; set; }

    public Item(string className, string displayName)
    {
        Id = IdProvider.Next();
        ClassName = className;
        DisplayName = displayName;
    }

    public static Item Water;
    public static Item Copper;
    public static Item Iron;
    public static Item Wood;
    public static Item Coal;
    public static Item Uranium;
    public static Item Bauxite;
    public static Item Gold;
    public static Item Stone;
    public static Item Sulfur;
    public static Item Oil;
    public static Item Quartz;
    public static Item Nitrogen;
}


[DebuggerDisplay("{DisplayName}")]
public class Recipe
{
    public string ClassName { get; }
    public string DisplayName { get; }
    public ItemRateCollection Ingredients { get; }
    public ItemRateCollection Products { get; }
    public HashSet<string> ProducedIn { get; }
    public float Duration { get; }
    public Cost Cost { get; private set; }
    public bool IsAlternate { get; }

    public Recipe(ClassDescriptor descriptor, DataProvider dataProvider)
    {
        if (string.IsNullOrEmpty(descriptor.mDisplayName)) throw new Exception();

        ClassName = descriptor.ClassName;
        DisplayName = descriptor.mDisplayName.Split('.')[^1];
        Duration = float.Parse(descriptor.mManufactoringDuration);
        Ingredients = ItemRate.Create(this, descriptor.mIngredients, dataProvider);
        Products = ItemRate.Create(this, descriptor.mProduct, dataProvider);
        
        if (!string.IsNullOrEmpty(descriptor.mProducedIn))
        {
            ProducedIn = JsonSerializer
                .Deserialize<string[]>(descriptor.mProducedIn.Replace('(', '[').Replace(')', ']'))!
                .Select(e => e.Split('.')[1])
                .ToHashSet();
        }

        IsAlternate = ClassName.StartsWith("Recipe_Alternate_") || DisplayName.StartsWith("Alternate: ");
        //Cost = new Cost(this);
    }
}
[DebuggerDisplay("{Recipe.DisplayName}")]
public sealed class SingleRecipe
{
    public Recipe Recipe { get; }
    public Item Product { get; }
    public Cost CostPerOneProduct { get; }
    public ItemRateCollection IngredientsPerOneProduct { get; }
    public ItemRateCollection NonResourceIngredientsPerOneProduct { get; }
    public ItemRate? TrashRate { get; }

    public SingleRecipe(Recipe recipe, Item product)
    {
        Recipe = recipe;
        Product = product;

        var productRate = recipe.Products[product].Amount;

        IngredientsPerOneProduct = new ItemRateCollection(recipe, recipe.Ingredients.Select(e => new ItemRate(e.Item, e.Amount / productRate)));
        CostPerOneProduct = new Cost(IngredientsPerOneProduct);
        NonResourceIngredientsPerOneProduct = new ItemRateCollection(recipe, IngredientsPerOneProduct.Where(e => !DataProvider.RawResourceClassNames.Contains(e.Item.ClassName)));

        var trash = recipe.Products.SingleOrDefault(e => e.Item != product);
        if (trash != null)
        {
            TrashRate = new ItemRate(trash.Item, trash.Amount / productRate);
        }

    }
}

public class ItemRateCollection : IEnumerable<ItemRate>
{
    public Recipe Recipe { get; }

    private readonly IDictionary<Item, ItemRate> _items;

    public ItemRate this[Item item] => _items[item];

    public ItemRateCollection(Recipe? recipe, IEnumerable<ItemRate> itemRates)
    {
        Recipe = recipe;
        _items = itemRates.ToDictionary(e => e.Item);
    }


    public IEnumerator<ItemRate> GetEnumerator() => _items.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGet(Item item, out ItemRate? itemRate) => _items.TryGetValue(item, out itemRate);
}

[DebuggerDisplay("{Item.DisplayName} {Amount}")]
public class ItemRate
{
    public Item Item { get; set; }
    public float Amount { get; set; }
    public float UnitAmount { get; set; }
    public Recipe? Recipe { get; set; }

    public ItemRate(Recipe recipe, Item item, float amount)
    {
        

        Recipe = recipe;
        UnitAmount = amount / recipe.Duration * 60;
        Item = item;
        Amount = amount;
    }

    public ItemRate(Item item, float amount)
    {
        Item = item;
        Amount = amount;
        UnitAmount = amount;
    }

    public static ItemRateCollection Create(Recipe recipe, string input, DataProvider dataProvider)
    {
        if (input[0] != '(') throw new NotSupportedException();
        if (input[^1] != ')') throw new NotSupportedException();

        input = $"[{input.Substring(1, input.Length - 2)}]";
        input = input.Replace("(", "{");
        input = input.Replace(")", "}");
        input = input.Replace("=", ":");
        input = input.Replace("\"", "");
        input = input.Replace("'", "");

        input = input.Replace("ItemClass:", "\"ItemClass\":\"");
        input = input.Replace("Amount", "\"Amount\"");
        input = input.Replace(",", "\",");
        input = input.Replace("}\",", "},");

        var itemRates = JsonSerializer
            .Deserialize<ItemRateJson[]>(input)!
            .Select(e => new ItemRate(recipe, dataProvider.GetItemByName(e.ItemClass.Split('.')[^1]), e.Amount));

        return new ItemRateCollection(recipe, itemRates);
    }
}

public record ItemRateJson(string ItemClass, float Amount);