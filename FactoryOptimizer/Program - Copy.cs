using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

public static class Program
{
    public static Dictionary<string, string> ClassNameMapping = new();

    public static void Main(string[] args)
    {
        var game = JsonSerializer.Deserialize<Descriptor[]>(File.OpenRead(@"All.json"));

        var itemsByName = game
            .SelectMany(e => e.Classes)
            .ToDictionary(e => e.ClassName, w => new Item
            {
                ClassName = w.ClassName,
                DisplayName = w.mDisplayName
            });

        var validProducedIn = new[]
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

        var notSupportedIngredients = new[]
        {
            "Desc_StingerParts_C", "Desc_Mycelia_C", "Desc_Leaves_C", "Desc_SpitterParts_C", "Desc_HogParts_C",
            "Desc_HatcherParts_C", "Desc_AlienProtein_C",

            "Desc_PetroleumCoke_C", "Desc_CompactedCoal_C",
            "Desc_PackagedOil_C", "Desc_PackagedOilResidue_C", "Desc_PackagedWater_C", "Desc_PackagedBiofuel_C", "Desc_PackagedAlumina_C",
            "Desc_PackagedNitrogenGas_C", "Desc_PackagedNitricAcid_C", "Desc_PackagedSulfuricAcid_C",

            "Desc_FluidCanister_C"
        }.ToHashSet();

        var recipes = game
            .SelectMany(e => e.Classes
                .Where(w => w.ClassName.StartsWith("Recipe_") &&
                            !w.ClassName.StartsWith("Recipe_Pattern_") &&
                            !w.ClassName.StartsWith("Recipe_Swatch"))
                .Where(e => !string.IsNullOrEmpty(e.mDisplayName))
                .Where(e => !string.IsNullOrEmpty(e.ClassName))
                .Where(e => !string.IsNullOrEmpty(e.mProduct))
                .Where(e => !string.IsNullOrEmpty(e.mProducedIn))
                .Where(e => !string.IsNullOrEmpty(e.mIngredients))
            )


            .Select(e => new Recipe(e, game, itemsByName))
            .Where(e => e.ProducedIn.Any(p => validProducedIn.Contains(p)))
            .Where(e => e.Ingredients.All(ingr => !notSupportedIngredients.Contains(ingr.Item.ClassName)))
            .Where(e => e.Products.All(ingr => !notSupportedIngredients.Contains(ingr.Item.ClassName)))
            .ToDictionary(e => e.ClassName);

        Dictionary<Item, List<Recipe>> recipesByProducts = recipes
            .SelectMany(e => e.Value.Products.Select(w => new
            {
                Product = w.Item,
                Recepie = e
            }))
            .GroupBy(e => e.Product)
            .ToDictionary(e => e.Key, e => e.Select(e => e.Recepie.Value).ToList());


        var trueResources = new[]
        {
            "Desc_Water_C", "Desc_OreCopper_C", "Desc_OreIron_C", "Desc_Wood_C", "Desc_Coal_C", "Desc_OreUranium_C",
            "Desc_OreBauxite_C", "Desc_OreGold_C", "Desc_Stone_C", "Desc_Sulfur_C", "Desc_LiquidOil_C",
            "Desc_RawQuartz_C", "Desc_NitrogenGas_C",
        }.Select(e => itemsByName[e]).ToHashSet();

        var resources = new[]
            {

                "Desc_PetroleumCoke_C", "Desc_CompactedCoal_C",
                "Desc_PackagedOil_C", "Desc_PackagedOilResidue_C", "Desc_PackagedWater_C", "Desc_PackagedBiofuel_C",
                "Desc_PackagedAlumina_C",
                "Desc_PackagedNitrogenGas_C", "Desc_PackagedNitricAcid_C", "Desc_PackagedSulfuricAcid_C"

            }.Select(e => itemsByName[e])
            .Concat(trueResources)
            .ToHashSet();

        var what = itemsByName["Desc_Computer_C"];
        //var what = itemsByName["Desc_IronScrew_C"];
        //var what = itemsByName["Desc_Rotor_C"];
        var howMuch = 1f;

        var pendingFactories = new Queue<Factory>(new Factory[]
        {
            new()
            {
                Ingredients = new Queue<ItemRate>(new []{new ItemRate(what, 1)})
            }
        });

        var availableResources = new ItemRateCollection(new Dictionary<string, float>
        {
            ["Desc_Stone_C"] = 480,
            ["Desc_OreIron_C"] = 1440,
            ["Desc_OreCopper_C"] = 360,
            ["Desc_LiquidOil_C"] = 1020 * 1000,
            ["Desc_Water_C"] = 10000000000,
            ["Desc_Coal_C"] = 960,
            ["Desc_OreGold_C"] = 240,
            ["Desc_RawQuartz_C"] = 480,
            ["Desc_OreBauxite_C"] = 240
        }.Select(e => new ItemRate(itemsByName[e.Key], e.Value)));

        var finishedLofaszok = new List<Factory>();

        var newBest = 0f;
        var circles = 0;
        var variations = 0;

        while (pendingFactories.Count > 0)
        {
            var currentLofasz = pendingFactories.Dequeue();
            variations++;
            while (currentLofasz.Ingredients.Count > 0)
            {
                var demand = currentLofasz.Ingredients.Dequeue();
                //if (resources.Contains(demand.Item)) continue;
                //if (currentLofasz.IngredientsWithRecipes.ContainsKey(demand.Item)) continue;

                var recipeOptions = recipesByProducts[demand.Item];
                var isExistingRecipe = currentLofasz.IngredientsWithRecipes.TryGetValue(demand.Item, out var existingRecipe);
                if (isExistingRecipe)
                {
                    recipeOptions = new[] { existingRecipe }.ToList();
                }

                var newFactory = new Factory(currentLofasz);

                foreach (var recipe in recipeOptions)
                {
                    var factory = recipe == recipeOptions[0] 
                        ? currentLofasz 
                        : new Factory(newFactory);

                    if (isExistingRecipe) factory = currentLofasz;

                    if (!isExistingRecipe && recipe != recipeOptions[0]) pendingFactories.Enqueue(factory);

                    var productionAmount = recipe.Products[demand.Item];
                    var productionDuration = recipe.Duration;
                    var productionPerSec = productionAmount.Amount / productionDuration;
                    var productionPerMinutePerFactory = productionPerSec * 60f;
                    var factor = demand.Amount / productionPerMinutePerFactory;

                    foreach (var product in recipe.Products)
                    {
                        if (product.Item != demand.Item)
                        {
                            factory.Trash.TryGetValue(product.Item, out var trashAmount);
                            factory.Trash[product.Item] = product.Amount * factor + trashAmount;
                        }
                    }

                    if (factory.Id == 73)
                    {

                    }

                    factory.IngredientsWithRecipes.TryAdd(demand.Item, recipe);

                    foreach (var ingredient in recipe.Ingredients)
                    {
                        var factoredAmount = (demand.Amount / productionAmount.Amount) * ingredient.Amount;

                        if (resources.Contains(ingredient.Item))
                        {
                            if (factory.Id == 73)
                            {

                            }

                            if (factory.Cost.TryGet(ingredient.Item, out var itemRate))
                            {
                                factory.Cost[ingredient.Item].Amount += factoredAmount;
                            }
                            else
                            {
                                factory.Cost[ingredient.Item] = new ItemRate(ingredient.Item, factoredAmount);
                            }
                        }
                        else
                        {
                            if (ingredient.Item.ClassName == "Iron Ingot" && factory.Id == 73)
                            {

                            }

                            factory.Ingredients.Enqueue(new ItemRate(ingredient.Item, factoredAmount));
                        }

                        
                    }
                }
            }
            currentLofasz.Finish();
            if (currentLofasz.Id == 4)
            {

            }
            var expectedCost = currentLofasz.CalculateCost(what, trueResources);
            var actualCost = currentLofasz.Cost;

            foreach (var expectedCostItem in expectedCost)
            {
                if (actualCost[expectedCostItem.Item].Amount != expectedCostItem.Amount)
                {

                }
            }

            //if (cost == null)
            //{
            //    circles++;
            //    continue;
            //}

            var max = MaxAmount(currentLofasz.Cost, availableResources) * howMuch;
            var names = currentLofasz.GetRecipeNames(recipes);
            //var totalConsumption = Cost.ToDictionary(e => e.Key, e => e.Value * max);

            finishedLofaszok.Add(currentLofasz);

            if (variations % 1000 == 0)
            {
                Console.WriteLine($"{variations} / {circles}");
            }

            if (max <= newBest)
            {
                continue;
            }



            newBest = max;
            Console.WriteLine(max);



            if (false && finishedLofaszok.Any(e => e.AreSame(currentLofasz)))
            {
                // throw new Exception("asd");
            }
            finishedLofaszok.Add(currentLofasz);
        }

        var grp = finishedLofaszok
            .GroupBy(e => MaxAmount(e.CalculateCost(what, trueResources), availableResources))
            .Select(e => new
            {
                CostCategory = e.Key,
                Items = e.ToList()
            })
            .OrderByDescending(e => e.Items.Count).ToList();


        Console.WriteLine($"Bestest: {newBest}");
    }


    public static float MaxAmount(ItemRateCollection required, ItemRateCollection available)
    {
        var min = float.MaxValue;

        foreach (var res in required)
        {
            if (!available.TryGet(res.Item, out var resAvailable))
            {
                return 0;
            }

            var current = resAvailable.Amount / res.Amount;
            if (min > current)
            {
                min = current;
            }
        }

        return min;
    }

    public class Factory
    {
        public static int MaxId = 0;
        public readonly int Id;

        public Queue<ItemRate> Ingredients { get; set; } = new();
        public Dictionary<Item, Recipe> IngredientsWithRecipes { get; set; } = new();
        public HashSet<Recipe> Recipes { get; set; } = new();
        public Dictionary<Item, float> Trash { get; set; } = new();
        public Dictionary<Item, float> CostPerUnit { get; set; } = new();
        public ItemRateCollection Cost { get; set; } = new();

        public HashSet<string> GetRecipeNames(Dictionary<string, Recipe> recipes)
        {
            return IngredientsWithRecipes.Select(e => e.Value.ClassName).ToHashSet();
        }


        public ItemRateCollection CalculateCost(Item what, HashSet<Item> resources)
        {
            var cost = new ItemRateCollection();

            var queue = new Queue<ItemRate>(new[] { new ItemRate(what, 1f) });
            var cycles = 0;

            while (queue.Count > 0)
            {
                cycles++;
                if (cycles > 1000)
                {
                    return null;
                }
                var demand = queue.Dequeue();
                var recipe = IngredientsWithRecipes[demand.Item];

                var productionAmount = recipe.Products[demand.Item];
                var productionDuration = recipe.Duration;
                var productionPerSec = productionAmount.Amount / productionDuration;
                var productionPerMinutePerFactory = productionPerSec * 60f;
                var factor = demand.Amount / productionPerMinutePerFactory;

                foreach (var product in recipe.Products)
                {
                    if (product.Item != demand.Item)
                    {
                        Trash.TryGetValue(product.Item, out var trashAmount);
                        Trash[product.Item] = product.Amount * factor + trashAmount;
                    }
                }

                foreach (var ingredient in recipe.Ingredients)
                {
                    var factoredAmount = (demand.Amount / productionAmount.Amount) * ingredient.Amount;

                    if (resources.Contains(ingredient.Item))
                    {
                        if (cost.TryGet(ingredient.Item, out var itemRate))
                        {
                            cost[ingredient.Item].Amount += factoredAmount;
                        }
                        else
                        {
                            cost[ingredient.Item] = new ItemRate(ingredient.Item, factoredAmount);
                        }
                    }
                    else
                    {
                        queue.Enqueue(new ItemRate(ingredient.Item, factoredAmount));
                    }
                }
            }

            return cost;
        }

        public Factory()
        {
            MaxId++;
            Id = MaxId;
        }

        public Factory(Factory factoryCopy) : this()
        {
            Ingredients = new Queue<ItemRate>(factoryCopy.Ingredients.ToList());
            IngredientsWithRecipes = factoryCopy.IngredientsWithRecipes.ToDictionary(e => e.Key, e => e.Value);
            Trash = factoryCopy.Trash.ToDictionary(e => e.Key, e => e.Value);
            Cost = new ItemRateCollection(factoryCopy.Cost.Select(e => new ItemRate(e.Item, e.Amount)));
        }

        public void Finish()
        {
            Recipes = IngredientsWithRecipes.Values.ToHashSet();
            if (Recipes.Count != IngredientsWithRecipes.Count)
            {
                //throw new Exception("o");
            }
        }

        public bool AreSame(Factory other)
        {
            if (other.IngredientsWithRecipes.Count != other.IngredientsWithRecipes.Count) return false;

            foreach (var item in other.IngredientsWithRecipes)
            {
                if (!IngredientsWithRecipes.TryGetValue(item.Key, out var result))
                {
                    return false;
                }

                if (result != item.Value) return false;
            }

            return true;
        }
    }

}
