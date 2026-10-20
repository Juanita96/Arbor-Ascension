using System.Collections.Generic;

// bonuses calculated once when the battle starts
public struct TeamBonuses
{
    // variables
    public float Synergy;
    public float ReserveBasic;
    public float CooldownMultiplier;

    // functions
    public static TeamBonuses None
    {
        get
        {
            // no bonuses at all
            return new TeamBonuses { Synergy = 0f, ReserveBasic = 0f, CooldownMultiplier = 1f };
        }
    }
}

public static class ElementSynergy
{
    // variables
    public const float TwoElementsBonus = 0.10f;
    public const float ThreeElementsBonus = 0.20f;

    // functions
    public static int CountDistinct(IList<AlchemistInfo> AttackTeam)
    {
        // a hash set only keeps each element once
        HashSet<Element> Elements = new HashSet<Element>();

        for (int i = 0; i < AttackTeam.Count; i++)
        {
            if (AttackTeam[i] != null)
            {
                Elements.Add(AttackTeam[i].Element);
            }
        }

        return Elements.Count;
    }

    public static float GetBonus(IList<AlchemistInfo> AttackTeam)
    {
        // more different elements means a bigger bonus
        int Distinct = CountDistinct(AttackTeam);

        if (Distinct >= 3)
        {
            return ThreeElementsBonus;
        }

        if (Distinct == 2)
        {
            return TwoElementsBonus;
        }

        return 0f;
    }
}
