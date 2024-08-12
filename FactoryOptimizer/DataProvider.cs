using System.Diagnostics;
using System.Text.Json;

namespace FactoryOptimizer;

public class DataProvider
{
    public IReadOnlyDictionary<Item, List<SingleRecipe>> RecipesByProducts { get; }
    public IReadOnlyDictionary<string, Item> ItemsByName { get; }

    public static readonly HashSet<string> UncomfortableRecipes = new[]
    {
        "Recipe_Alternate_Plastic_1_C",
        //"Recipe_Alternate_RecycledRubber_C"
    }.ToHashSet();

    public static readonly HashSet<string> RawResourceClassNames = new[]
    {
        "Desc_Water_C", "Desc_OreCopper_C", "Desc_OreIron_C", "Desc_Wood_C", "Desc_Coal_C", "Desc_OreUranium_C",
        "Desc_OreBauxite_C", "Desc_OreGold_C", "Desc_Stone_C", "Desc_Sulfur_C", "Desc_LiquidOil_C",
        "Desc_RawQuartz_C", "Desc_NitrogenGas_C"
    }.ToHashSet();

    private static readonly HashSet<string> ValidBuildings = new[]
    {
        "Build_ConstructorMk1_C",
        "Build_SmelterMk1_C",
        "Build_OilRefinery_C",
        "Build_AssemblerMk1_C",
        "Build_FoundryMk1_C",
        "Build_Blender_C",
        "Build_ManufacturerMk1_C",
        "Build_HadronCollider_C",
    }.ToHashSet();

    private static readonly HashSet<string> NotSupportedIngredients = new[]
    {
        "Desc_StingerParts_C", "Desc_Mycelia_C", "Desc_Leaves_C", "Desc_SpitterParts_C", "Desc_HogParts_C",
        "Desc_HatcherParts_C", "Desc_AlienProtein_C",

        "Desc_PetroleumCoke_C", "Desc_CompactedCoal_C",
        "Desc_PackagedOil_C", "Desc_PackagedOilResidue_C", "Desc_PackagedWater_C", "Desc_PackagedBiofuel_C", "Desc_PackagedAlumina_C",
        "Desc_PackagedNitrogenGas_C", "Desc_PackagedNitricAcid_C", "Desc_PackagedSulfuricAcid_C",

        "Desc_FluidCanister_C"
    }.ToHashSet();

    private static readonly Dictionary<string, HashSet<string>> NotAllowedRecipesForItems = new()
    {
        ["Desc_SulfuricAcid_C"] = new[] { "Recipe_UraniumCell_C" }.ToHashSet()
    };

    public Item GetItemByName(string className) => ItemsByName[className];
    public bool IsAllowed(Item item, Recipe recipe)
    {
        if (!NotAllowedRecipesForItems.TryGetValue(item.ClassName, out var blackList))
        {
            return true;
        }

        return !blackList.Contains(recipe.ClassName);
    }

    public record ItemO(string Name, string DisplayName);

    public record RecipeO(string Name, string DisplayName, float Duration, IReadOnlyCollection<Product> Ingredients, IReadOnlyCollection<Product> Products);

    public record Product(string Name, float Amount);

    public DataProvider(string path)
    {
        var descriptors = JsonSerializer.Deserialize<Descriptor[]>(File.OpenRead(path));

        ItemsByName = descriptors!
            .SelectMany(e => e.Classes)
            .ToDictionary(e => e.ClassName, w => new Item(w.ClassName, w.mDisplayName));

        var items = ItemsByName.Select(e => new ItemO(e.Value.ClassName, e.Value.DisplayName)).ToList();
        

        Item.Water = ItemsByName["Desc_Water_C"];
        Item.Copper = ItemsByName["Desc_OreCopper_C"];
        Item.Iron = ItemsByName["Desc_OreIron_C"];
        Item.Wood = ItemsByName["Desc_Wood_C"];
        Item.Coal = ItemsByName["Desc_Coal_C"];
        Item.Uranium = ItemsByName["Desc_OreUranium_C"];
        Item.Bauxite = ItemsByName["Desc_OreBauxite_C"];
        Item.Gold = ItemsByName["Desc_OreGold_C"];
        Item.Stone = ItemsByName["Desc_Stone_C"];
        Item.Sulfur = ItemsByName["Desc_Sulfur_C"];
        Item.Oil = ItemsByName["Desc_LiquidOil_C"];
        Item.Quartz = ItemsByName["Desc_RawQuartz_C"];
        Item.Nitrogen = ItemsByName["Desc_NitrogenGas_C"];

        var rec = GetRecipes(descriptors!);

        var recipes = rec.Select(e => new RecipeO(e.Recipe.ClassName, e.Recipe.DisplayName, e.Recipe.Duration,

            e.Recipe.Ingredients.Select(w => new Product(w.Item.ClassName, w.Amount)).ToList(),
            e.Recipe.Products.Select(w => new Product(w.Item.ClassName, w.Amount)).ToList())).ToList();

        RecipesByProducts = GetRecipes(descriptors!)
            .GroupBy(e => e.Product)
            .ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Select(e => e).ToList());

        BuildRelationships();
    }

    private void BuildRelationships()
    {
        foreach (var item in ItemsByName.Values)
        {
            if (RecipesByProducts.TryGetValue(item, out var recipes))
            {
                item.Recipes = recipes.Where(e => IsAllowed(item, e.Recipe)).ToList();
                item.MainRecipe = recipes.FirstOrDefault(e => !e.Recipe.IsAlternate) ?? recipes.First();
                item.AlternateRecipes = recipes.Where(e => e != item.MainRecipe).ToList();
            }
            else
            {
                Debug.WriteLine(item.ClassName);
            }
            
        }
    }

    private IEnumerable<SingleRecipe> GetRecipes(Descriptor[] descriptors)
    {
        return descriptors
            .SelectMany(e => e.Classes
                .Where(w => !UncomfortableRecipes.Contains(w.ClassName))
                .Where(w => w.ClassName.StartsWith("Recipe_") &&
                            !w.ClassName.StartsWith("Recipe_Pattern_") &&
                            !w.ClassName.StartsWith("Recipe_Swatch"))
                .Where(descriptor => !string.IsNullOrEmpty(descriptor.mDisplayName))
                .Where(descriptor => !string.IsNullOrEmpty(descriptor.ClassName))
                .Where(descriptor => !string.IsNullOrEmpty(descriptor.mProduct))
                .Where(descriptor => !string.IsNullOrEmpty(descriptor.mProducedIn))
                .Where(descriptor => !string.IsNullOrEmpty(descriptor.mIngredients))
                .Where(descriptor => float.TryParse(descriptor.mManufactoringDuration, out var duration) && duration > 0)
                )
            .Select(descriptor => new Recipe(descriptor, this))
            .Where(recipe => recipe.ProducedIn.Any(building => ValidBuildings.Contains(building)))
            .Where(recipe => recipe.Ingredients.All(ingredient => !NotSupportedIngredients.Contains(ingredient.Item.ClassName)))
            .Where(recipe => recipe.Products.All(product => !NotSupportedIngredients.Contains(product.Item.ClassName)))
            .SelectMany(e => e.Products.Select(w => new SingleRecipe(e, w.Item)));
    }

    
}