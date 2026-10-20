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
    }

    public TowerLevel[] Levels =
    {
        new TowerLevel { MaxHealth = 300f },
        new TowerLevel { MaxHealth = 300f, UpgradeCost = new ResourceCost { Sulfur = 10, Mercurius = 10, Sal = 10 } }
    };

    public int MaxLevel => Levels.Length;

    // functions
    public TowerLevel Get(int Level)
    {
        // levels start at 1, the array starts at 0
        return Levels[Mathf.Clamp(Level - 1, 0, Levels.Length - 1)];
    }
}
