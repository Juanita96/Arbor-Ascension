using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Arbor/Tower Config", fileName = "TowerConfig")]
public class TowerConfig : ScriptableObject
{
    // variables
    [Serializable]
    public class TowerLevel
    {
        public float MaxHealth = 300f;
        [Tooltip("cost to reach this level from the previous one, ignored on level 1")]
        public ResourceCost UpgradeCost;
        public GameObject VisualPrefab;

        [Header("Bonuses of this level")]
        [Tooltip("extra damage and healing for the alchemists, 1 means +100%")]
        public float AlchemistDamageBonus = 0f;
        [Tooltip("the max health of the alchemists is multiplied by this")]
        public float AlchemistHealthMultiplier = 1f;
        [Tooltip("size of the tree visual on this level")]
        public float VisualScale = 1f;
    }

    public TowerLevel[] Levels =
    {
        new TowerLevel { MaxHealth = 300f },
        new TowerLevel
        {
            MaxHealth = 600f,
            UpgradeCost = new ResourceCost { Sulfur = 5, Mercurius = 5, Sal = 5 },
            AlchemistDamageBonus = 1f,
            AlchemistHealthMultiplier = 2f,
            VisualScale = 1.4f
        }
    };

    public int MaxLevel => Levels.Length;

    // functions
    public TowerLevel Get(int Level)
    {
        // levels start at 1, the array starts at 0
        return Levels[Mathf.Clamp(Level - 1, 0, Levels.Length - 1)];
    }
}
