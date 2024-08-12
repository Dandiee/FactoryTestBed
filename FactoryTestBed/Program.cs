using System.Diagnostics;
using FactoryTestBed;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class Program
{
    public static void Main(string[] args)
    {
        var data = DataProvider.Get();
        var availableResourcesPerMinute = new ItemRate[]
        {
            new (data.GetItem("Desc_Stone_C"), 480),
            new (data.GetItem("Desc_OreIron_C"), 1440),
            new (data.GetItem("Desc_OreCopper_C"), 360),
            new (data.GetItem("Desc_LiquidOil_C"), 1020 * 1000),
            new (data.GetItem("Desc_Water_C"), 10000000000),
            new (data.GetItem("Desc_Coal_C"), 960),
            new (data.GetItem("Desc_OreGold_C"), 240),
            new (data.GetItem("Desc_RawQuartz_C"), 480),
            new (data.GetItem("Desc_OreBauxite_C"), 240),
            // these make things harder
            // new (data.GetItem("Desc_NitrogenGas_C"), 100000),
            // new (data.GetItem("Desc_OreUranium_C"), 1000),
            // new (data.GetItem("Desc_Sulfur_C"), 1000),
        };

        Test(data, availableResourcesPerMinute, "Desc_IronScrew_C", 23040);
        Test(data, availableResourcesPerMinute, "Desc_Rotor_C", 511.99997f);
        Test(data, availableResourcesPerMinute, "Desc_Computer_C", 37.333336f);
        Test(data, availableResourcesPerMinute, "Desc_ComputerSuper_C", 8.372093f);
    }

    private static float GetBest(Data data, IReadOnlyCollection<ItemRate> availableResourcesPerMinute, string itemName, float expectedAmountPerMinute)
    {
        // TODO: improve
        return 42;
    }

    private static void Test(Data data, IReadOnlyCollection<ItemRate> availableResourcesPerMinute, string itemName, float expectedAmountPerMinute)
    {
        var item = data.GetItem(itemName);
        var sw = Stopwatch.StartNew();
        var best = GetBest(data, availableResourcesPerMinute, itemName, expectedAmountPerMinute);
        Console.WriteLine($"{item.DisplayName} took {sw.ElapsedMilliseconds:N0} ms");
        Debug.Assert(Math.Abs(best - expectedAmountPerMinute) < 0.001);
    }
}