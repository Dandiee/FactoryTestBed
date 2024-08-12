using System.Text.Json;
using FactoryTestBed;

public sealed class DataProvider
{
    public static Data Get()
    {
        using var itemsFile = File.OpenRead("Data/Items.json");
        var items = JsonSerializer.Deserialize<List<Item>>(itemsFile).ToDictionary(e => e.Name);

        using var recipesFile = File.OpenRead("Data/Recipes.json");
        var recipes = JsonSerializer.Deserialize<List<Recipe>>(recipesFile).DistinctBy(e => e.Name).ToList();

        foreach (var recipe in recipes)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                ingredient.Item = items[ingredient.Name];
            }

            foreach (var product in recipe.Products)
            {
                product.Item = items[product.Name];
            }
        }

        var recipesByProducts =recipes.SelectMany(r => r.Products.Select(p => new { r, p }))
            .GroupBy(grp => grp.p.Name)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Select(w => w.r));

        foreach (var item in items.Values)
        {
            item.Recipes = recipesByProducts.TryGetValue(item.Name, out var itemRecipes) ? itemRecipes.ToList() : Array.Empty<Recipe>();
        }

        return new Data
        {
            Items = items.Values,
            ItemsByName = items,
            Recipes = recipes
        };
    }
}