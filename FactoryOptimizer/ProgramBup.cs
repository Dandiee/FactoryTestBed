
//using System.Linq;
//using System.Reflection.Metadata;
//using System.Text.Json;

//public static class Program
//{
//    public static Dictionary<string, string> ClassNameMapping = new();

//    public static void Main(string[] args)
//    {

//        var game = JsonSerializer.Deserialize<Descriptor[]>(File.OpenRead(@"All.json"));

//        ClassNameMapping = game
//            .SelectMany(e => e.Classes)
//            .ToDictionary(e => e.ClassName, w => w.mDisplayName);

//        var validProducedIn = new[]
//        {
//            "Build_ConstructorMk1_C",
//            "Build_SmelterMk1_C",
//            "Build_OilRefinery_C",
//            "Build_AssemblerMk1_C",
//            "Build_FoundryMk1_C",
//            "Build_Blender_C",
//            "Build_ManufacturerMk1_C",
//            "Build_HadronCollider_C",
//        }.ToHashSet();

//        var notSupportedIngredients = new[]
//        {
//            "Desc_StingerParts_C", "Desc_Mycelia_C", "Desc_Leaves_C", "Desc_SpitterParts_C", "Desc_HogParts_C",
//            "Desc_HatcherParts_C", "Desc_AlienProtein_C",

//            "Desc_PetroleumCoke_C", "Desc_CompactedCoal_C",
//            "Desc_PackagedOil_C", "Desc_PackagedOilResidue_C", "Desc_PackagedWater_C", "Desc_PackagedBiofuel_C", "Desc_PackagedAlumina_C",
//            "Desc_PackagedNitrogenGas_C", "Desc_PackagedNitricAcid_C", "Desc_PackagedSulfuricAcid_C",

//            "Desc_FluidCanister_C"
//        }.ToHashSet();

//        var recipes = game
//            .SelectMany(e => e.Classes
//                .Where(w => w.ClassName.StartsWith("Recipe_") && 
//                            !w.ClassName.StartsWith("Recipe_Pattern_") &&
//                            !w.ClassName.StartsWith("Recipe_Swatch")))

//            .Select(e => new Recipe(e, game))
//            .Where(e => e.ProducedIn.Any(p => validProducedIn.Contains(p)))
//            .Where(e => e.Ingredients.All(ingr => !notSupportedIngredients.Contains(ingr.Key)) &&
//                        e.Products.All(ingr => !notSupportedIngredients.Contains(ingr.Key)))
//            .ToDictionary(e => e.ClassName);

//        var allTypes = recipes.SelectMany(e => e.Value.ProducedIn).ToHashSet();

//        //var asd = recipes.Single(e => e.ClassName == "Recipe_Protein_Stinger_C");

//        var recipesByProducts = recipes.SelectMany(e => e.Value.Products.Select(w => new
//        {
//            Product = w.Key,
//            Recepie = e
//        })).GroupBy(e => e.Product)
//            .ToDictionary(e => e.Key, e => e.ToList());


//        var trueResources = new[]
//        {
//            "Desc_Water_C", "Desc_OreCopper_C", "Desc_OreIron_C", "Desc_Wood_C", "Desc_Coal_C", "Desc_OreUranium_C",
//            "Desc_OreBauxite_C", "Desc_OreGold_C", "Desc_Stone_C", "Desc_Sulfur_C", "Desc_LiquidOil_C",
//            "Desc_RawQuartz_C", "Desc_NitrogenGas_C",
//        }.ToHashSet();

//        var resources = new[]
//        {
            
//            "Desc_PetroleumCoke_C", "Desc_CompactedCoal_C",
//            "Desc_PackagedOil_C", "Desc_PackagedOilResidue_C", "Desc_PackagedWater_C", "Desc_PackagedBiofuel_C", "Desc_PackagedAlumina_C", 
//            "Desc_PackagedNitrogenGas_C", "Desc_PackagedNitricAcid_C", "Desc_PackagedSulfuricAcid_C"

//        }.Concat(trueResources).ToHashSet();

//        var what = "Desc_Computer_C";
//        //var what = "Desc_IronScrew_C";
//        //var what = "Desc_Rotor_C";
//        var howMuch = 1;

//        var pendingLofaszok = new Queue<Lofasz>(new Lofasz[]
//        {
//            new Lofasz()
//            {
//                //pendingRecipes = new Queue<string>(new [] {"Desc_ComputerSuper_C"})
//                //pendingRecipes = new Queue<string>(new [] {"Desc_Rotor_C"})
//                pendingRecipes = new Queue<string>(new [] {what})
//            }
//        });

//        var availableResources = new Dictionary<string, float>
//        {
//            ["Desc_Stone_C"] = 480,
//            ["Desc_OreIron_C"] = 1440,
//            ["Desc_OreCopper_C"] = 360,
//            ["Desc_LiquidOil_C"] = 1020 * 1000,
//            ["Desc_Water_C"] = 10000000000,
//            ["Desc_Coal_C"] = 960,
//            ["Desc_OreGold_C"] = 240,
//            ["Desc_RawQuartz_C"] = 480,
//            ["Desc_OreBauxite_C"] = 240
//        };

//        var finishedLofaszok = new List<Lofasz>();

//        var newBest = 0f;
//        var circles = 0;
//        var variations = 0;

//        while (pendingLofaszok.Count > 0)
//        {
//            var currentLofasz = pendingLofaszok.Dequeue();
//            variations++;
//            while (currentLofasz.pendingRecipes.Count > 0)
//            {
//                var currentIngredient = currentLofasz.pendingRecipes.Dequeue();
//                if (resources.Contains(currentIngredient)) continue;
//                if (currentLofasz.ingredientsWithProducts.ContainsKey(currentIngredient)) continue;

//                var recipeOptions = recipesByProducts[currentIngredient];
//                var lofaszCopy = new Lofasz(currentLofasz);

//                foreach (var recipe in recipeOptions)
//                {
//                    var lofasz = recipe == recipeOptions[0]
//                        ? currentLofasz
//                        : new Lofasz(lofaszCopy);

//                    if (recipe != recipeOptions[0]) pendingLofaszok.Enqueue(lofasz);

//                    lofasz.ingredientsWithProducts.Add(currentIngredient, recipe.Recepie.Value.ClassName);

//                    foreach (var ingredient in recipe.Recepie.Value.Ingredients)
//                    {
//                        lofasz.pendingRecipes.Enqueue(ingredient.Key);
//                    }
//                }
//            }
//            currentLofasz.Finish();
//            var cost = currentLofasz.CalculateCost(what, howMuch, recipes, trueResources);
//            if (cost == null)
//            {
//                circles++;
//                continue;
//            }

//            var max = MaxAmount(cost, availableResources) * howMuch;
//            var names = currentLofasz.GetRecipeNames(recipes);
//            var totalConsumption = cost.ToDictionary(e => e.Key, e => e.Value * max);

//            finishedLofaszok.Add(currentLofasz);

//            if (variations % 1000 == 0)
//            {
//                Console.WriteLine($"{variations} / {circles}");
//            }

//            if (max <= newBest)
//            {
//                continue;
                

//            }

            

//            newBest = max;
//            Console.WriteLine(max);



//            if (false && finishedLofaszok.Any(e => e.AreSame(currentLofasz)))
//            {
//               // throw new Exception("asd");
//            }
//            finishedLofaszok.Add(currentLofasz);
//        }

//        var grp = finishedLofaszok
//            .GroupBy(e => MaxAmount(e.CalculateCost(what, howMuch, recipes, resources), availableResources))
//            .Select(e => new
//            {
//                CostCategory = e.Key,
//                Items = e.ToList()
//            })
//            .OrderByDescending(e => e.Items.Count).ToList();
            

//        Console.WriteLine($"Bestest: {newBest}");
//    }


//    public static float MaxAmount(Dictionary<string, float> required, Dictionary<string, float> available)
//    {
//        var min = float.MaxValue;

//        foreach (var res in required)
//        {
//            if (!available.TryGetValue(res.Key, out var resAvailable))
//            {
//                return 0;
//            }

//            var current = resAvailable / res.Value;
//            if (min > current)
//            {
//                min = current;
//            }
//        }

//        return min;
//    }

//    public record ItemNeed(string Name, float Amount);

//    public class Lofasz
//    {
//        public static int MaxId = 0;
//        public readonly int Id;

//        public Queue<string> pendingRecipes { get; set; } = new();
//        public Dictionary<string, string> ingredientsWithProducts { get; set; } = new();
//        public HashSet<string> finalRecipes { get; set; } = new();
//        public Dictionary<string, float> trash { get; set; } = new();
//        public Dictionary<string, float> cost { get; set; } = new();

//        public HashSet<string> GetRecipeNames(Dictionary<string, Recipe> recipes)
//        {
//            return ingredientsWithProducts.Select(e => recipes[e.Value].Name).ToHashSet();
//        }


//        public Dictionary<string, float> CalculateCost(string what, float amount, Dictionary<string, Recipe> recipes, HashSet<string> resources)
//        {
//            var cost = new Dictionary<string, float>();

//            var queue = new Queue<ItemNeed>(new[] { new ItemNeed(what, amount) });
//            var cycles = 0;

//            while (queue.Count > 0)
//            {
//                cycles++;
//                if (cycles > 1000)
//                {
//                    return null;
//                }
//                var item = queue.Dequeue();
//                var recipeName = ingredientsWithProducts[item.Name];
//                var recipe = recipes[recipeName];

//                var productionAmount = recipe.Products[item.Name];
//                var productionDuration = recipe.Duration;
//                var productionPerSec = productionAmount/productionDuration;
//                var productionPerMinutePerFactory = productionPerSec * 60f;
//                var factor = item.Amount / productionPerMinutePerFactory;

//                foreach (var product in recipe.Products)
//                {
//                    if (product.Key != item.Name)
//                    {
//                        trash.TryGetValue(product.Key, out var trashAmount);
//                        trash[product.Key] = product.Value * factor + trashAmount;
//                    }
//                }

//                foreach (var ingredient in recipe.Ingredients)
//                {
//                    var ingredientName = ingredient.Key;
//                    var factoredAmount = (item.Amount / productionAmount) * ingredient.Value;

//                    if (resources.Contains(ingredientName))
//                    {
//                        cost.TryGetValue(ingredientName, out var resourceCost);
//                        cost[ingredientName] = resourceCost + factoredAmount;
//                    }
//                    else
//                    {
//                        queue.Enqueue(new ItemNeed(ingredientName, factoredAmount));
//                    }
//                }
//            }

//            return cost;
//        }

//        public Lofasz()
//        {
//            MaxId++;
//            Id = MaxId;
//        }

//        public Lofasz(Lofasz lofaszCopy) : this()
//        {
//            pendingRecipes = new Queue<string>(lofaszCopy.pendingRecipes.ToList());
//            ingredientsWithProducts = lofaszCopy.ingredientsWithProducts.ToDictionary(e => e.Key, e => e.Value);
            
//        }

//        public void Finish()
//        {
//            finalRecipes = ingredientsWithProducts.Values.ToHashSet();
//            if (finalRecipes.Count != ingredientsWithProducts.Count)
//            {
//                //throw new Exception("o");
//            }
//        }

//        public bool AreSame(Lofasz other)
//        {
//            if (other.ingredientsWithProducts.Count != other.ingredientsWithProducts.Count) return false;

//            foreach (var item in other.ingredientsWithProducts)
//            {
//                if (!ingredientsWithProducts.TryGetValue(item.Key, out var result))
//                {
//                    return false;
//                }

//                if (result != item.Value) return false;
//            }

//            return true;
//        }
//    }

//    public static void MainWorkin(string[] args)
//    {

//        var game = JsonSerializer.Deserialize<Descriptor[]>(File.OpenRead(@"All.json"));

//        ClassNameMapping = game
//            .SelectMany(e => e.Classes)
//            .ToDictionary(e => e.ClassName, w => w.mDisplayName);



//        var recipes = game
//            .SelectMany(e => e.Classes
//                .Where(w => w.ClassName.StartsWith("Recipe_") && !w.ClassName.StartsWith("Recipe_Pattern_")))
//            .Select(e => new Recipe(e, game));

//        var recipesByNames = recipes.ToDictionary(e => e.ClassName);

//        var recipesByProducts = recipes.SelectMany(e => e.Products.Select(w => new
//        {
//            Product = w.Key,
//            Recepie = e
//        })).GroupBy(e => e.Product)
//            .ToDictionary(e => e.Key, e => e.ToList());

//        const string target = "Rotor";
//        const float targetAmountPerSecond = 10f / 60;

//        var rootRecipeName = recipesByProducts["Desc_Rotor_C"][0].Recepie.ClassName;
//        var queue = new Queue<string>(new[] { rootRecipeName });
//        var knownIngredients = new Dictionary<string, float>
//        {
//            [rootRecipeName] = targetAmountPerSecond
//        };

//        var pendingRecipes = new Queue<string>(new[] { "Desc_ComputerSuper_C" });
//        var ingredientsWithProducts = new Dictionary<string, string>();

//        var resources = new[]
//        {
//            "Desc_Water_C", "Desc_OreCopper_C", "Desc_OreIron_C", "Desc_Wood_C", "Desc_Coal_C", "Desc_OreUranium_C",
//            "Desc_OreBauxite_C", "Desc_OreGold_C", "Desc_Stone_C", "Desc_Sulfur_C", "Desc_LiquidOil_C"
//        }.ToHashSet();

//        while (pendingRecipes.Count > 0)
//        {
//            var currentIngredientName = pendingRecipes.Dequeue();
//            if (resources.Contains(currentIngredientName)) continue;
//            if (ingredientsWithProducts.ContainsKey(currentIngredientName)) continue;

//            var selectedRecipe = recipesByProducts[currentIngredientName][0];
//            ingredientsWithProducts.Add(currentIngredientName, selectedRecipe.Recepie.ClassName);

//            foreach (var ingredient in selectedRecipe.Recepie.Ingredients)
//            {
//                pendingRecipes.Enqueue(ingredient.Key);
//            }
//        }
//    }
//}
