using System.Diagnostics;

namespace FactoryOptimizer;

public static class Program
{
    public static async Task Main(string[] args)
    {

        var dataProvider = new DataProvider(@"All.json");

        //var item = dataProvider.GetItemByName("Desc_Rotor_C");
        //var item = dataProvider.GetItemByName("Desc_IronScrew_C");
        //var item = dataProvider.GetItemByName("Desc_CoolingSystem_C");
        //var item = dataProvider.GetItemByName("Desc_ComputerSuper_C");
        var item = dataProvider.GetItemByName("Desc_SpaceElevatorPart_5_C");
        //var item = dataProvider.GetItemByName("Desc_ModularFrameLightweight_C");
        //var item = dataProvider.GetItemByName("Desc_SpaceElevatorPart_7_C");

        //var what = dataProvider.GetItemByName("Desc_Computer_C");

        var availableResources = new ItemRateCollection(null, new Dictionary<string, float>
        {
            ["Desc_Stone_C"] = 480,
            ["Desc_OreIron_C"] = 1440,
            ["Desc_OreCopper_C"] = 360,
            ["Desc_LiquidOil_C"] = 1020 * 1000,
            ["Desc_Water_C"] = 10000000000,
            ["Desc_Coal_C"] = 960,
            ["Desc_OreGold_C"] = 240,
            ["Desc_RawQuartz_C"] = 480,
            ["Desc_OreBauxite_C"] = 240,
            //["Desc_OreUranium_C"] = 1000,
            //["Desc_Sulfur_C"] = 1000,
            //["Desc_NitrogenGas_C"] = 1000000, 
        }.Select(e => new ItemRate(dataProvider.GetItemByName(e.Key), e.Value)));

        var availableCost = new Cost(availableResources);

        

        var sw = Stopwatch.StartNew();
        var bestFactory = Solver.Optimize(item, availableCost);
        Console.WriteLine($"Best possible combination for {item.DisplayName} is {bestFactory.MaxAmount:N2} / min [calculated in: {sw.ElapsedMilliseconds} ms]");
    }
}