using System.Runtime.InteropServices;

namespace FactoryOptimizer;

public struct Cost
{
    public float Water;
    public float Copper;
    public float Iron;
    public float Wood;
    public float Coal;
    public float Uranium;
    public float Bauxite;
    public float Gold;
    public float Stone;
    public float Sulfur;
    public float Oil;
    public float Quartz;
    public float Nitrogen;

    public Cost(Cost cost)
    {
        Water = cost.Water;
        Copper = cost.Copper;
        Iron = cost.Iron;
        Wood = cost.Wood;
        Coal = cost.Coal;
        Uranium = cost.Uranium;
        Bauxite = cost.Bauxite;
        Gold = cost.Gold;
        Stone = cost.Stone;
        Sulfur = cost.Sulfur;
        Oil = cost.Oil;
        Quartz = cost.Quartz;
        Nitrogen = cost.Nitrogen;
    }

    public Cost(ItemRateCollection ingredientsPerOneItem)
    {
        Water = ingredientsPerOneItem.TryGet(Item.Water, out var water) ? water.Amount : 0;
        Copper = ingredientsPerOneItem.TryGet(Item.Copper, out var copper) ? copper.Amount : 0;
        Iron = ingredientsPerOneItem.TryGet(Item.Iron, out var iron) ? iron.Amount : 0;
        Wood = ingredientsPerOneItem.TryGet(Item.Wood, out var wood) ? wood.Amount : 0;
        Coal = ingredientsPerOneItem.TryGet(Item.Coal, out var coal) ? coal.Amount : 0;
        Uranium = ingredientsPerOneItem.TryGet(Item.Uranium, out var uranium) ? uranium.Amount : 0;
        Bauxite = ingredientsPerOneItem.TryGet(Item.Bauxite, out var bauxite) ? bauxite.Amount : 0;
        Gold = ingredientsPerOneItem.TryGet(Item.Gold, out var gold) ? gold.Amount : 0;
        Stone = ingredientsPerOneItem.TryGet(Item.Stone, out var stone) ? stone.Amount : 0;
        Sulfur = ingredientsPerOneItem.TryGet(Item.Sulfur, out var sulfur) ? sulfur.Amount : 0;
        Oil = ingredientsPerOneItem.TryGet(Item.Oil, out var oil) ? oil.Amount : 0;
        Quartz = ingredientsPerOneItem.TryGet(Item.Quartz, out var quartz) ? quartz.Amount : 0;
        Nitrogen = ingredientsPerOneItem.TryGet(Item.Nitrogen, out var nitrogen) ? nitrogen.Amount : 0;
    }

    public static Cost operator +(Cost lhs, Cost rhs)
    {
        return new Cost
        {
            Water = lhs.Water + rhs.Water,
            Copper = lhs.Copper + rhs.Copper,
            Iron = lhs.Iron + rhs.Iron,
            Wood = lhs.Wood + rhs.Wood,
            Coal = lhs.Coal + rhs.Coal,
            Uranium = lhs.Uranium + rhs.Uranium,
            Bauxite = lhs.Bauxite + rhs.Bauxite,
            Gold = lhs.Gold + rhs.Gold,
            Stone = lhs.Stone + rhs.Stone,
            Sulfur = lhs.Sulfur + rhs.Sulfur,
            Oil = lhs.Oil + rhs.Oil,
            Quartz = lhs.Quartz + rhs.Quartz,
            Nitrogen = lhs.Nitrogen + rhs.Nitrogen,
        };
    }

    public static Cost operator*(Cost lhs, float rhs)
    {
        return new Cost
        {
            Water = lhs.Water * rhs,
            Copper = lhs.Copper * rhs,
            Iron = lhs.Iron * rhs,
            Wood = lhs.Wood * rhs,
            Coal = lhs.Coal * rhs,
            Uranium = lhs.Uranium * rhs,
            Bauxite = lhs.Bauxite * rhs,
            Gold = lhs.Gold * rhs,
            Stone = lhs.Stone * rhs,
            Sulfur = lhs.Sulfur * rhs,
            Oil = lhs.Oil * rhs,
            Quartz = lhs.Quartz * rhs,
            Nitrogen = lhs.Nitrogen * rhs,
        };
    }

    public static float MaxAmount(Cost required, Cost available)
    {
        var worst = float.MaxValue;

        if (required.Copper > 0)
        {
            var min = available.Copper / required.Copper;
            if (min < worst) worst = min;
        }

        if (required.Iron > 0)
        {
            var min = available.Iron / required.Iron;
            if (min < worst) worst = min;
        }

        if (required.Wood > 0)
        {
            var min = available.Wood / required.Wood;
            if (min < worst) worst = min;
        }

        if (required.Coal > 0)
        {
            var min = available.Coal / required.Coal;
            if (min < worst) worst = min;
        }

        if (required.Uranium > 0)
        {
            var min = available.Uranium / required.Uranium;
            if (min < worst) worst = min;
        }

        if (required.Bauxite > 0)
        {
            var min = available.Bauxite / required.Bauxite;
            if (min < worst) worst = min;
        }

        if (required.Gold > 0)
        {
            var min = available.Gold / required.Gold;
            if (min < worst) worst = min;
        }

        if (required.Stone > 0)
        {
            var min = available.Stone / required.Stone;
            if (min < worst) worst = min;
        }

        if (required.Sulfur > 0)
        {
            var min = available.Sulfur / required.Sulfur;
            if (min < worst) worst = min;
        }

        if (required.Oil > 0)
        {
            var min = available.Oil / required.Oil;
            if (min < worst) worst = min;
        }

        if (required.Quartz > 0)
        {
            var min = available.Quartz / required.Quartz;
            if (min < worst) worst = min;
        }

        if (required.Nitrogen > 0)
        {
            var min = available.Nitrogen / required.Nitrogen;
            if (min < worst) worst = min;
        }

        return worst;

    }
}