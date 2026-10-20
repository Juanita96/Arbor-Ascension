using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // variables
    public const int MaxAttackTeam = 3;
    public const int MaxReserveTeam = 3;

    private static PlayerInventory CachedInstance;

    public int GoldDust { get; private set; }
    public int TowerLevel { get; private set; } = 1;

    private readonly Dictionary<ResourceType, int> Resources = new Dictionary<ResourceType, int>();
    private readonly List<AlchemistInfo> OwnedCharacters = new List<AlchemistInfo>();
    private readonly List<AlchemistInfo> AttackTeamList = new List<AlchemistInfo>();
    private readonly List<AlchemistInfo> ReserveTeamList = new List<AlchemistInfo>();

    // references
    public IReadOnlyList<AlchemistInfo> Owned => OwnedCharacters;
    public IReadOnlyList<AlchemistInfo> AttackTeam => AttackTeamList;
    public IReadOnlyList<AlchemistInfo> ReserveTeam => ReserveTeamList;
    public AlchemistInfo AttackLeader { get; private set; }
    public AlchemistInfo ReserveLeader { get; private set; }

    // functions
    public static PlayerInventory Instance
    {
        get
        {
            // creating the inventory the first time someone needs it
            if (CachedInstance == null)
            {
                new GameObject("PlayerInventory").AddComponent<PlayerInventory>();
            }

            return CachedInstance;
        }
    }

    private void Awake()
    {
        // only one inventory can exist
        if (CachedInstance != null && CachedInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        // keeping it alive between scenes
        CachedInstance = this;
        DontDestroyOnLoad(gameObject);

        // nothing is saved, so every time the game opens it's a new game
        StartNewGame(GameConfig.Instance);
    }

    private void StartNewGame(GameConfig GameConfiguration)
    {
        GoldDust = GameConfiguration.StartingGoldDust;

        // giving the starter alchemists
        foreach (AlchemistInfo Character in GameConfiguration.StarterAlchemists)
        {
            if (Character != null && !OwnedCharacters.Contains(Character))
            {
                OwnedCharacters.Add(Character);
            }
        }

        // putting them in the attack team so the player can fight right away
        FillTeam(AttackTeamList, OwnedCharacters, MaxAttackTeam, true);
        AttackLeader = FirstOrNull(AttackTeamList);
    }

    public int GetResource(ResourceType Type)
    {
        // returning 0 if the player never got this resource
        int Amount;
        if (Resources.TryGetValue(Type, out Amount))
        {
            return Amount;
        }

        return 0;
    }

    public void AddResource(ResourceType Type, int Amount)
    {
        Resources[Type] = GetResource(Type) + Amount;
        Changed();
    }

    public bool CanAfford(ResourceCost Cost)
    {
        // the player needs enough of every resource
        return GetResource(ResourceType.Sulfur) >= Cost.Sulfur
            && GetResource(ResourceType.Mercurius) >= Cost.Mercurius
            && GetResource(ResourceType.Sal) >= Cost.Sal;
    }

    public bool TrySpend(ResourceCost Cost)
    {
        // making sure player can afford it
        if (!CanAfford(Cost))
        {
            return false;
        }

        // subtracting every resource
        Resources[ResourceType.Sulfur] = GetResource(ResourceType.Sulfur) - Cost.Sulfur;
        Resources[ResourceType.Mercurius] = GetResource(ResourceType.Mercurius) - Cost.Mercurius;
        Resources[ResourceType.Sal] = GetResource(ResourceType.Sal) - Cost.Sal;
        Changed();

        return true;
    }

    public void AddGoldDust(int Amount)
    {
        GoldDust += Amount;
        Changed();
    }

    public bool TrySpendGoldDust(int Amount)
    {
        // making sure player has enough
        if (GoldDust < Amount)
        {
            return false;
        }

        // subtracting
        GoldDust -= Amount;
        Changed();

        return true;
    }

    // returns true if the alchemist is new, if it's a duplicate the player gets gold dust instead
    public bool AddCharacter(AlchemistInfo Character)
    {
        // nothing passed
        if (Character == null)
        {
            return false;
        }

        // adding if player doesn't already own it
        if (!OwnedCharacters.Contains(Character))
        {
            OwnedCharacters.Add(Character);
            Changed();

            return true;
        }

        // compensating the player for the duplicated result
        AddGoldDust(GameConfig.Instance.DuplicateGoldDustReward);

        return false;
    }

    public bool HasCharacter(AlchemistInfo Character)
    {
        return OwnedCharacters.Contains(Character);
    }

    // replaces both teams, anything invalid is ignored
    public void SetTeams(IList<AlchemistInfo> Attack, IList<AlchemistInfo> Reserve, AlchemistInfo NewAttackLeader, AlchemistInfo NewReserveLeader)
    {
        // clearing
        AttackTeamList.Clear();
        ReserveTeamList.Clear();

        // filling
        FillTeam(AttackTeamList, Attack, MaxAttackTeam, true);
        FillTeam(ReserveTeamList, Reserve, MaxReserveTeam, false);

        // keeping the chosen leaders, or the oldest member if the leader left the team
        if (AttackTeamList.Contains(NewAttackLeader))
        {
            AttackLeader = NewAttackLeader;
        }
        else
        {
            AttackLeader = FirstOrNull(AttackTeamList);
        }

        if (ReserveTeamList.Contains(NewReserveLeader))
        {
            ReserveLeader = NewReserveLeader;
        }
        else
        {
            ReserveLeader = FirstOrNull(ReserveTeamList);
        }

        Changed();
    }

    private void FillTeam(List<AlchemistInfo> Target, IList<AlchemistInfo> Source, int Max, bool IsAttack)
    {
        // no source passed
        if (Source == null)
        {
            return;
        }

        // going through every member until the team is full
        for (int i = 0; i < Source.Count && Target.Count < Max; i++)
        {
            AlchemistInfo Character = Source[i];

            // skipping alchemists the player doesn't own
            if (Character == null || !HasCharacter(Character))
            {
                continue;
            }

            // the reserve alchemist can't be in the attack team
            if (IsAttack && Character.IsReserveOnly)
            {
                continue;
            }

            // an alchemist can't be in both teams
            if (AttackTeamList.Contains(Character) || ReserveTeamList.Contains(Character))
            {
                continue;
            }

            Target.Add(Character);
        }
    }

    private static AlchemistInfo FirstOrNull(List<AlchemistInfo> List)
    {
        if (List.Count > 0)
        {
            return List[0];
        }

        return null;
    }

    public bool CanUpgradeTower(TowerConfig TowerConfiguration)
    {
        // the tree can be upgraded if it's not at max level and the player can pay
        return TowerConfiguration != null && TowerLevel < TowerConfiguration.MaxLevel && CanAfford(TowerConfiguration.Get(TowerLevel + 1).UpgradeCost);
    }

    public bool TryUpgradeTower(TowerConfig TowerConfiguration)
    {
        // already at max level
        if (TowerConfiguration == null || TowerLevel >= TowerConfiguration.MaxLevel)
        {
            return false;
        }

        // paying the upgrade
        if (!TrySpend(TowerConfiguration.Get(TowerLevel + 1).UpgradeCost))
        {
            return false;
        }

        TowerLevel++;
        UnlockByLevel(GameConfig.Instance);
        Changed();

        return true;
    }

    private void UnlockByLevel(GameConfig GameConfiguration)
    {
        // the reserve alchemist is unlocked when the tree reaches its level
        AlchemistInfo Reserve = GameConfiguration.ReserveAlchemist;

        if (Reserve != null && TowerLevel >= GameConfiguration.ReserveUnlockLevel && !OwnedCharacters.Contains(Reserve))
        {
            OwnedCharacters.Add(Reserve);
        }
    }

    private void Changed()
    {
        // telling the ui that something changed
        if (OnChanged != null)
        {
            OnChanged.Invoke();
        }
    }

    // events
    public event Action OnChanged;
}
