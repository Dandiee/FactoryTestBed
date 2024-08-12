using System.Collections.Concurrent;
using System.Dynamic;

namespace FactoryOptimizer;

public class Factory
{
    public string GetLink(Item item, float amount) => $"{{\"{item.ClassName}\":\"{amount}\",\"altRecipes\":[{string.Join(",", IngredientsWithRecipes.Values.Select(e => $"\"{e.Recipe.ClassName}\""))}]}}";

    public Cost Cost { get; set; }
    public float MaxAmount { get; set; }
    public Queue<ItemRate> Ingredients { get; } = new();
    public List<ItemRate> Trash { get; } = new();
    public Dictionary<Item, SingleRecipe> IngredientsWithRecipes { get; } = new(ItemComparer.Instance);

    private static readonly ConcurrentBag<Factory> Graveyard = new();

    private static Factory Create()
    {
        if (Graveyard.TryTake(out var factory))
        {
            return factory;
        }

        return new Factory();
    }

    public void Kill()
    {
        Ingredients.Clear();
        IngredientsWithRecipes.Clear();
        Trash.Clear();
        Cost = new Cost();

        Graveyard.Add(this);
    }

    public static Factory Create(Factory factoryCopy)
    {
        var instance = Create();

        foreach (var item in factoryCopy.Ingredients)
        {
            instance.Ingredients.Enqueue(item);
        }
        
        foreach (var kvp in factoryCopy.IngredientsWithRecipes)
        {
            instance.IngredientsWithRecipes[kvp.Key] = kvp.Value;
        }

        foreach (var item in factoryCopy.Trash)
        {
            instance.Trash.Add(item);
        }

        instance.MaxAmount = factoryCopy.MaxAmount;
        instance.Cost = new Cost(factoryCopy.Cost);
        return instance;
    }

    public static Factory Create(Item demand)
    {
        var instance = Create();
        instance.Ingredients.Enqueue(new ItemRate(demand, 1));
        return instance;
    }
}