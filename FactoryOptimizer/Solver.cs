namespace FactoryOptimizer;

public static class Solver
{
    private const int HorizontalLimit = 1024 * 8;

    private static readonly ParallelOptions ParallelOptions = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    private static volatile float _veryNotThreadSafeMax = 0;

    public static Factory Optimize(Item item, Cost availableCost)
    {

        var globalResult = new Factory();

        var threads = new MyQueue<Factory>(Factory.Create(item));
        while (threads.Count is > 0 and < HorizontalLimit)
        {
            var factory = Traversal(threads, availableCost);
            if (factory.Ingredients.Count == 0 && factory.MaxAmount > _veryNotThreadSafeMax)
            {
                globalResult = Factory.Create(factory);
                _veryNotThreadSafeMax = factory.MaxAmount;
                Console.WriteLine($"New best (single): {factory.MaxAmount:N2}");
            }
        }


        if (threads.Count > 0)
        {
            Parallel.ForEach(threads, ParallelOptions, () => Factory.Create(item), (rootFactory, _, localBest) =>
                {
                    var stack = new MyStack<Factory>(rootFactory);
                    while (stack.Count > 0)
                    {
                        var factory = Traversal(stack, availableCost);
                        if (factory.Ingredients.Count == 0 && factory.MaxAmount > _veryNotThreadSafeMax)
                        {
                            localBest = Factory.Create(factory);
                            _veryNotThreadSafeMax = factory.MaxAmount;
                            Console.WriteLine($"New best (multi): {localBest.MaxAmount:N2}");
                        }
                        factory.Kill();
                    }

                    return localBest;
                }, localResult =>
                {
                    if (localResult.MaxAmount > globalResult.MaxAmount)
                    {
                        globalResult = localResult;
                    }
                });
        }

        return globalResult;
    }

    private static Factory Traversal(IDataStructure<Factory> structure, Cost availableCost)
    {
        var factory = structure.Get();
        if (Cost.MaxAmount(factory.Cost, availableCost) < _veryNotThreadSafeMax)
        {
            return factory;
        }

        while (factory.Ingredients.Count > 0)
        {
            var demand = factory.Ingredients.Dequeue();
            if (!factory.IngredientsWithRecipes.TryGetValue(demand.Item, out var recipe))
            {
                // first create all the alternate factories
                foreach (var alternativeRecipe in demand.Item.AlternateRecipes)
                {
                    var alternateFactory = Factory.Create(factory);
                    alternateFactory.Ingredients.Enqueue(demand);
                    alternateFactory.IngredientsWithRecipes.Add(demand.Item, alternativeRecipe);
                    structure.Add(alternateFactory);
                }

                // carry on with the original factory with the main recipe
                recipe = demand.Item.MainRecipe;
                factory.IngredientsWithRecipes.Add(demand.Item, recipe);
            }

            factory.Cost += recipe.CostPerOneProduct * demand.Amount;
            factory.MaxAmount = Cost.MaxAmount(factory.Cost, availableCost);

            if (_veryNotThreadSafeMax > factory.MaxAmount)
            {
                break;
            }

            foreach (var ingredient in recipe.NonResourceIngredientsPerOneProduct)
            {
                factory.Ingredients.Enqueue(new ItemRate(ingredient.Item, ingredient.Amount * demand.Amount));
            }

            if (recipe.TrashRate != null)
            {
                factory.Trash.Add(new ItemRate(recipe.TrashRate.Item, recipe.TrashRate.Amount * demand.Amount));
            }
        }

        return factory;
    }
}