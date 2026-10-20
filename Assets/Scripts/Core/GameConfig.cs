using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Arbor/Game Config", fileName = "ArborGameConfig")]
public class GameConfig : ScriptableObject
{
    // variables
    public const string ResourcePath = "ArborGameConfig";

    private static GameConfig CachedInstance;

    [Header("Gacha")]
    [FormerlySerializedAs("StartingCrystals")] public int StartingGoldDust = 300;
    public int PullCost = 100;
    [FormerlySerializedAs("DuplicateCrystalReward")] public int DuplicateGoldDustReward = 50;

    [Header("Tree")]
    public int ReserveUnlockLevel = 2;

    [Header("Scenes")]
    public string CombatScene = "Combat";
    public string PlayMenuScene = "PlayMenu";
    public string TeamScene = "ArrangeTeam";
    public string MainMenuScene = "MainMenu";

    // references
    [Header("Alchemists")]
    [Tooltip("every alchemist in the game, used to show the total in the inventory")]
    public List<AlchemistInfo> AllAlchemists = new List<AlchemistInfo>();
    [Tooltip("alchemists the player owns when the game starts")]
    public List<AlchemistInfo> StarterAlchemists = new List<AlchemistInfo>();
    [Tooltip("unlocked when the tree reaches the reserve unlock level")]
    public AlchemistInfo ReserveAlchemist;
    public CharacterUnit DefaultCharacterPrefab;

    [Header("Tree")]
    public TowerConfig Tower;

    // functions
    public static GameConfig Instance
    {
        get
        {
            // loading the config from the resources folder the first time it's needed
            if (CachedInstance == null)
            {
                CachedInstance = Resources.Load<GameConfig>(ResourcePath);

                // creating an empty one so the game doesn't break if the asset is missing
                if (CachedInstance == null)
                {
                    Debug.LogWarning("[GameConfig] Resources/ArborGameConfig was not found");
                    CachedInstance = CreateInstance<GameConfig>();
                }
            }

            return CachedInstance;
        }
    }
}
