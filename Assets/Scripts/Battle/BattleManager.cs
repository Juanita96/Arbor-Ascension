using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum BattleResult
{
    Victoria,
    Derrota,
    Retirada
}

// gold dust and tria prima earned in a battle
[Serializable]
public struct Loot
{
    // variables
    public int GoldDust;
    public int Sulfur;
    public int Mercurius;
    public int Sal;

    // functions
    public int Get(ResourceType Type)
    {
        switch (Type)
        {
            case ResourceType.Sulfur:
                return Sulfur;
            case ResourceType.Mercurius:
                return Mercurius;
            default:
                return Sal;
        }
    }

    public void Add(ResourceType Type, int Amount)
    {
        switch (Type)
        {
            case ResourceType.Sulfur:
                Sulfur += Amount;
                break;
            case ResourceType.Mercurius:
                Mercurius += Amount;
                break;
            default:
                Sal += Amount;
                break;
        }
    }

    public void Add(Loot Other)
    {
        GoldDust += Other.GoldDust;
        Sulfur += Other.Sulfur;
        Mercurius += Other.Mercurius;
        Sal += Other.Sal;
    }
}

public class BattleManager : MonoBehaviour
{
    // variables
    public static BattleManager Instance { get; private set; }

    private const int MaxTeamSize = 3;
    private const float WaveRegenPercent = 0.10f;
    private const float SupportWaveRegenBonus = 0.05f;
    private const float ReservePassiveBonus = 0.10f;
    private const float TreeDoubleDamage = 2f;

    [Tooltip("if nothing shows the result (no hud), it goes back to the menu after this time")]
    [SerializeField] private float AutoReturnDelay = 2f;

    public bool BattleOver { get; private set; }
    public bool Paused { get; private set; }
    public BattleResult Result { get; private set; }
    public TeamBonuses Bonuses { get; private set; }
    public int DistinctElements { get; private set; }
    public Loot SecuredLoot { get; private set; }
    public Loot PendingLoot { get; private set; }

    private bool[] RowLostAlchemist;
    private float AutoReturnTimer = -1f;
    private readonly List<CharacterUnit> Characters = new List<CharacterUnit>();

    // references
    [Tooltip("only used if the player didn't build a team, to test the scene directly")]
    [SerializeField] private List<AlchemistInfo> TestTeam = new List<AlchemistInfo>();
    [SerializeField] private Tower Tower;
    [SerializeField] private WaveManager WaveManager;
    [SerializeField] private EnemySpawner Spawner;

    public Tower BattleTower => Tower;
    public WaveManager Waves => WaveManager;
    public IReadOnlyList<CharacterUnit> Team => Characters;
    public CharacterUnit Selected { get; private set; }

    // functions
    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        PlayerInventory Inventory = PlayerInventory.Instance;

        // ending the battle right away if there is no attack team
        List<AlchemistInfo> AttackTeam = BuildTeam(Inventory);
        if (AttackTeam.Count == 0)
        {
            Debug.LogWarning("[BattleManager] The attack team is empty, build it in ArrangeTeam");
            EndBattle(BattleResult.Retirada);
            return;
        }

        // calculating the team bonuses once
        Bonuses = CalculateBonuses(AttackTeam, Inventory);
        DistinctElements = ElementSynergy.CountDistinct(AttackTeam);

        // using the chosen leader, or the first alchemist if it's not in the team
        AlchemistInfo Leader = AttackTeam[0];
        if (Inventory.AttackLeader != null && AttackTeam.Contains(Inventory.AttackLeader))
        {
            Leader = Inventory.AttackLeader;
        }

        CharacterUnit DefaultPrefab = GameConfig.Instance.DefaultCharacterPrefab;
        RowLostAlchemist = new bool[LaneManager.Instance.RowCount];

        // placing each alchemist in its row, in the order of the team
        List<int> ActiveRows = new List<int>();
        for (int i = 0; i < AttackTeam.Count; i++)
        {
            AlchemistInfo CurrentData = AttackTeam[i];

            CharacterUnit Prefab = CurrentData.Prefab;
            if (Prefab == null)
            {
                Prefab = DefaultPrefab;
            }

            if (Prefab == null)
            {
                Debug.LogWarning("[BattleManager] " + CurrentData.DisplayName + " has no prefab and there is no DefaultCharacterPrefab in the GameConfig");
                continue;
            }

            CharacterUnit Unit = Instantiate(Prefab, LaneManager.Instance.GetCharacterPosition(i), Quaternion.identity);
            Unit.Init(CurrentData, i, Bonuses, CurrentData == Leader);
            Unit.OnDied += HandleCharacterDied;

            LaneManager.Instance.SetCharacter(i, Unit);
            Characters.Add(Unit);
            ActiveRows.Add(i);
        }

        // connecting events and starting the waves
        Spawner.SetActiveRows(ActiveRows);
        Tower.Health.OnDeath += HandleTowerDied;
        WaveManager.OnWaveStarted += HandleWaveStarted;
        WaveManager.OnWaveCleared += HandleWaveCleared;
        WaveManager.OnAllWavesCleared += HandleAllWavesCleared;
        WaveManager.StartWaves();

        if (OnBattleStarted != null)
        {
            OnBattleStarted.Invoke();
        }
    }

    private List<AlchemistInfo> BuildTeam(PlayerInventory Inventory)
    {
        // using the player's team, or the test team if the player has none
        IReadOnlyList<AlchemistInfo> Source = TestTeam;
        if (Inventory.AttackTeam.Count > 0)
        {
            Source = Inventory.AttackTeam;
        }

        int Count = Mathf.Min(Source.Count, MaxTeamSize, LaneManager.Instance.RowCount);

        // the reserve alchemist never fights
        List<AlchemistInfo> AttackTeam = new List<AlchemistInfo>();
        for (int i = 0; i < Count; i++)
        {
            if (Source[i] != null && !Source[i].IsReserveOnly)
            {
                AttackTeam.Add(Source[i]);
            }
        }

        return AttackTeam;
    }

    private static TeamBonuses CalculateBonuses(List<AlchemistInfo> AttackTeam, PlayerInventory Inventory)
    {
        TeamBonuses NewBonuses = TeamBonuses.None;

        // synergy from the different elements of the attack team
        NewBonuses.Synergy = ElementSynergy.GetBonus(AttackTeam);

        // passive of every reserve alchemist
        foreach (AlchemistInfo Member in Inventory.ReserveTeam)
        {
            if (Member.Kit == Kit.Reserva)
            {
                NewBonuses.ReserveBasic += ReservePassiveBonus;
            }
        }

        // the reserve leader reduces the cooldown of the actives
        AlchemistInfo ReserveLeader = Inventory.ReserveLeader;
        if (ReserveLeader != null && ReserveLeader.Kit == Kit.Reserva)
        {
            NewBonuses.CooldownMultiplier = 1f - ReserveLeader.LeaderValue;
        }

        return NewBonuses;
    }

    private void Update()
    {
        // after the battle ends, only the auto return is checked
        if (BattleOver)
        {
            if (AutoReturnTimer < 0f)
            {
                return;
            }

            AutoReturnTimer -= Time.unscaledDeltaTime;
            if (AutoReturnTimer <= 0f)
            {
                ReturnToMenu();
            }

            return;
        }

        // esc pauses and unpauses
        if (PausePressed())
        {
            SetPaused(!Paused);
        }

        if (Paused)
        {
            return;
        }

        // clicking selects an alchemist, clicking ui is ignored
        Vector2 ClickPosition;
        if (LeftClicked(out ClickPosition) && !IsPointerOverUI())
        {
            SelectAt(ClickPosition);
        }

        // e uses the active of the selected alchemist
        if (ActivePressed() && Selected != null)
        {
            Selected.TryUseActive();
        }
    }

    private void SelectAt(Vector2 ScreenPosition)
    {
        Camera MainCamera = Camera.main;
        if (MainCamera == null)
        {
            return;
        }

        // finding an alive alchemist under the mouse
        Vector3 WorldPosition = MainCamera.ScreenToWorldPoint(ScreenPosition);
        CharacterUnit Clicked = null;

        foreach (CharacterUnit Unit in Characters)
        {
            if (!Unit.IsAlive)
            {
                continue;
            }

            Vector2 Offset = (Vector2)Unit.transform.position - (Vector2)WorldPosition;
            if (Offset.magnitude <= CharacterUnit.ClickRadius)
            {
                Clicked = Unit;
            }
        }

        // clicking an empty spot deselects
        Select(Clicked);
    }

    private void Select(CharacterUnit Unit)
    {
        if (Selected != null)
        {
            Selected.SetSelected(false);
        }

        Selected = Unit;

        if (Selected != null)
        {
            Selected.SetSelected(true);
        }
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

#if ENABLE_INPUT_SYSTEM
    private static bool PausePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private static bool ActivePressed()
    {
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }

    private static bool LeftClicked(out Vector2 Position)
    {
        Position = Vector2.zero;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Position = Mouse.current.position.ReadValue();
        return true;
    }
#else
    private static bool PausePressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private static bool ActivePressed()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    private static bool LeftClicked(out Vector2 Position)
    {
        Position = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
    }
#endif

    public void SetPaused(bool NewPaused)
    {
        if (BattleOver)
        {
            return;
        }

        // freezing or resuming time
        Paused = NewPaused;
        if (NewPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (OnPauseChanged != null)
        {
            OnPauseChanged.Invoke(NewPaused);
        }
    }

    // called by enemies that are touching the tree
    public void DamageTree(int Row, float Damage)
    {
        // rows that lost their alchemist deal double damage to the tree
        float Multiplier = 1f;
        if (RowLostAlchemist != null && Row < RowLostAlchemist.Length && RowLostAlchemist[Row])
        {
            Multiplier = TreeDoubleDamage;
        }

        Tower.Health.TakeDamage(Damage * Multiplier);
    }

    // called by enemies when they die, it goes to the loot of the current wave
    public void AddLoot(IEnumerable<AlchemistInfo> Participants, int ResourcePerParticipant, int GoldDust)
    {
        if (BattleOver)
        {
            return;
        }

        Loot Pending = PendingLoot;
        Pending.GoldDust += GoldDust;

        // every alchemist that hit the enemy gives its resource, except the reserve one
        foreach (AlchemistInfo Participant in Participants)
        {
            if (Participant.Kit != Kit.Reserva)
            {
                Pending.Add(Participant.Resource, ResourcePerParticipant);
            }
        }

        PendingLoot = Pending;
    }

    public void Retire()
    {
        EndBattle(BattleResult.Retirada);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameConfig.Instance.MainMenuScene);
    }

    // loot the player keeps, victory keeps everything, otherwise only the secured part
    public Loot KeptLoot
    {
        get
        {
            Loot Kept = SecuredLoot;

            if (BattleOver && Result == BattleResult.Victoria)
            {
                Kept.Add(PendingLoot);
            }

            return Kept;
        }
    }

    private void EndBattle(BattleResult NewResult)
    {
        if (BattleOver)
        {
            return;
        }

        BattleOver = true;
        Result = NewResult;
        Paused = false;

        // stopping the waves and freezing the battle while the result is shown
        if (WaveManager != null)
        {
            WaveManager.Stop();
        }

        Time.timeScale = 0f;

        // giving the kept loot to the inventory
        Loot Kept = KeptLoot;
        PlayerInventory Inventory = PlayerInventory.Instance;

        if (Kept.GoldDust > 0)
        {
            Inventory.AddGoldDust(Kept.GoldDust);
        }

        foreach (ResourceType Type in Enum.GetValues(typeof(ResourceType)))
        {
            if (Kept.Get(Type) > 0)
            {
                Inventory.AddResource(Type, Kept.Get(Type));
            }
        }

        // letting the hud show the result, or going back to the menu if there is no hud
        if (OnBattleEnded != null)
        {
            OnBattleEnded.Invoke(NewResult);
        }
        else
        {
            AutoReturnTimer = AutoReturnDelay;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1f;
    }

    // events
    public event Action OnBattleStarted;
    public event Action<BattleResult> OnBattleEnded;
    public event Action<bool> OnPauseChanged;

    private void HandleWaveStarted(int WaveIndex)
    {
        // checking if the support alchemist is alive for its passive
        bool SupportAlive = false;
        foreach (CharacterUnit Unit in Characters)
        {
            if (Unit.IsAlive && Unit.Data.Kit == Kit.Apoyo)
            {
                SupportAlive = true;
            }
        }

        // every alive alchemist recovers part of its health when a wave starts
        float Percent = WaveRegenPercent;
        if (SupportAlive)
        {
            Percent += SupportWaveRegenBonus;
        }

        foreach (CharacterUnit Unit in Characters)
        {
            if (Unit.IsAlive)
            {
                Unit.Health.HealPercent(Percent);
            }
        }
    }

    private void HandleWaveCleared(int WaveIndex)
    {
        // the loot of the finished wave becomes secured
        Loot Secured = SecuredLoot;
        Secured.Add(PendingLoot);
        SecuredLoot = Secured;
        PendingLoot = new Loot();
    }

    private void HandleCharacterDied(CharacterUnit Character)
    {
        // the tree takes double damage from this row from now on
        RowLostAlchemist[Character.Row] = true;

        if (Selected == Character)
        {
            Select(null);
        }

        // losing when every alchemist is dead
        foreach (CharacterUnit Unit in Characters)
        {
            if (Unit.IsAlive)
            {
                return;
            }
        }

        EndBattle(BattleResult.Derrota);
    }

    private void HandleTowerDied(Health DeadHealth)
    {
        EndBattle(BattleResult.Derrota);
    }

    private void HandleAllWavesCleared()
    {
        EndBattle(BattleResult.Victoria);
    }
}
