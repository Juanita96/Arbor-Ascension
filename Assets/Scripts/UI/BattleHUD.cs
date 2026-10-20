using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    // variables
    [SerializeField] private float BannerDuration = 1.8f;

    private float BannerTimer;
    private float RefreshTimer;

    // references
    [SerializeField] private BattleManager Battle;

    [Header("Top bar")]
    [SerializeField] private TextMeshProUGUI WaveLabel;
    [Tooltip("its anchor max x is changed to show the tree health")]
    [SerializeField] private RectTransform TowerFill;
    [SerializeField] private TextMeshProUGUI TowerLabel;
    [SerializeField] private TextMeshProUGUI LootLabel;
    [Tooltip("opens the pause menu, same as esc")]
    [SerializeField] private Button RetireButton;

    [Header("Team and banner")]
    [SerializeField] private TextMeshProUGUI TeamLabel;
    [SerializeField] private TextMeshProUGUI BannerLabel;

    [Header("Pause and result")]
    [SerializeField] private GameObject ResultPanel;
    [SerializeField] private TextMeshProUGUI ResultTitle;
    [SerializeField] private TextMeshProUGUI ResultBody;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private TextMeshProUGUI ContinueLabel;
    [SerializeField] private Button PauseRetireButton;

    // functions
    private void Awake()
    {
        // subscribing buttons
        RetireButton.onClick.AddListener(OpenPause);
        ContinueButton.onClick.AddListener(HandleContinue);
        PauseRetireButton.onClick.AddListener(Battle.Retire);

        // subscribing battle events
        Battle.OnBattleEnded += HandleBattleEnded;
        Battle.OnPauseChanged += HandlePauseChanged;

        if (Battle.Waves != null)
        {
            Battle.Waves.OnWaveStarted += HandleWaveStarted;
        }

        // starting with everything hidden
        ResultPanel.SetActive(false);
        BannerLabel.text = "";
    }

    private void OnDestroy()
    {
        if (Battle == null)
        {
            return;
        }

        // unsubscribing battle events
        Battle.OnBattleEnded -= HandleBattleEnded;
        Battle.OnPauseChanged -= HandlePauseChanged;

        if (Battle.Waves != null)
        {
            Battle.Waves.OnWaveStarted -= HandleWaveStarted;
        }
    }

    private void Start()
    {
        // the battle may end before the hud starts (empty team)
        if (Battle.BattleOver)
        {
            HandleBattleEnded(Battle.Result);
        }
    }

    private void Update()
    {
        // fading out the wave banner
        if (BannerTimer > 0f)
        {
            BannerTimer -= Time.unscaledDeltaTime;

            Color BannerColor = BannerLabel.color;
            BannerColor.a = Mathf.Clamp01(BannerTimer);
            BannerLabel.color = BannerColor;
        }

        // refreshing the texts a few times per second instead of every frame
        RefreshTimer -= Time.unscaledDeltaTime;
        if (RefreshTimer > 0f)
        {
            return;
        }

        RefreshTimer = 0.1f;

        RefreshWave();
        RefreshTower();
        RefreshTeam();

        LootLabel.text = "REWARD: " + LootText(Battle.SecuredLoot, " - ")
            + "\nIN THIS WAVE " + LootText(Battle.PendingLoot, " - ");
    }

    private void RefreshWave()
    {
        WaveManager Waves = Battle.Waves;
        if (Waves == null)
        {
            return;
        }

        // showing the current wave, the last one is the boss wave
        int Current = Mathf.Min(Mathf.Max(0, Waves.CurrentWave) + 1, Waves.TotalWaves);

        if (Current == Waves.TotalWaves)
        {
            WaveLabel.text = "BOSS " + Current + "/" + Waves.TotalWaves;
        }
        else
        {
            WaveLabel.text = "WAVE " + Current + "/" + Waves.TotalWaves;
        }
    }

    private void RefreshTower()
    {
        Tower CurrentTower = Battle.BattleTower;
        if (CurrentTower == null || CurrentTower.Health == null)
        {
            return;
        }

        // resizing the health bar of the tree
        float Ratio = 0f;
        if (CurrentTower.Health.Max > 0f)
        {
            Ratio = CurrentTower.Health.Current / CurrentTower.Health.Max;
        }

        TowerFill.anchorMax = new Vector2(Mathf.Clamp01(Ratio), 1f);
        TowerLabel.text = "TREE LEVEL " + CurrentTower.Level + " - " + Mathf.CeilToInt(CurrentTower.Health.Current) + "/" + Mathf.CeilToInt(CurrentTower.Health.Max);
    }

    private void RefreshTeam()
    {
        StringBuilder Builder = new StringBuilder();
        IReadOnlyList<CharacterUnit> Team = Battle.Team;

        // one line per alchemist with its health and active cooldown
        for (int i = 0; i < Team.Count; i++)
        {
            CharacterUnit Unit = Team[i];
            if (Unit == null || Unit.Data == null)
            {
                continue;
            }

            if (Unit == Battle.Selected)
            {
                Builder.Append("> ");
            }

            Builder.Append("ROW ").Append(i + 1).Append(": ").Append(Unit.Data.DisplayName);

            if (Unit.IsLeader)
            {
                Builder.Append(" (LEADER)");
            }

            Builder.Append(" - ").Append(UIUtils.KitLabel(Unit.Data.Kit)).Append(" - ");

            if (!Unit.IsAlive)
            {
                Builder.Append("DEFEATED\n");
                continue;
            }

            Builder.Append("HP ").Append(Mathf.CeilToInt(Unit.Health.Current)).Append('/').Append(Mathf.CeilToInt(Unit.Health.Max));

            if (Unit.CooldownRemaining > 0f)
            {
                Builder.Append(" - E: " + Unit.CooldownRemaining.ToString("0.0") + "S");
            }
            else
            {
                Builder.Append(" - E: READY");
            }

            if (Unit.ShieldActive)
            {
                Builder.Append(" - SHIELD");
            }

            Builder.Append('\n');
        }

        // team bonuses
        TeamBonuses Bonuses = Battle.Bonuses;
        Builder.Append("SYNERGY: ").Append(Battle.DistinctElements).Append(" ELEMENTS");

        if (Bonuses.Synergy > 0f)
        {
            Builder.Append(" (+" + Mathf.RoundToInt(Bonuses.Synergy * 100f) + "%)");
        }
        else
        {
            Builder.Append(" (NO BONUS)");
        }

        if (Bonuses.ReserveBasic > 0f)
        {
            Builder.Append(" - RESERVE: +" + Mathf.RoundToInt(Bonuses.ReserveBasic * 100f) + "% BASIC DAMAGE");
        }

        if (Bonuses.CooldownMultiplier < 1f)
        {
            Builder.Append(", -" + Mathf.RoundToInt((1f - Bonuses.CooldownMultiplier) * 100f) + "% COOLDOWN");
        }

        // controls reminder
        Builder.Append("\nCLICK: SELECT - E: ACTIVE - ESC: PAUSE");

        TeamLabel.text = Builder.ToString();
    }

    private static string LootText(Loot CurrentLoot, string Separator)
    {
        return "GOLD DUST " + CurrentLoot.GoldDust + Separator
            + "SULPHUR " + CurrentLoot.Sulfur + Separator
            + "MERCURIUS " + CurrentLoot.Mercurius + Separator
            + "SALT " + CurrentLoot.Sal;
    }

    // events
    private void OpenPause()
    {
        Battle.SetPaused(true);
    }

    private void HandleWaveStarted(int WaveIndex)
    {
        // showing a banner when a wave starts
        bool IsBoss = Battle.Waves != null && WaveIndex == Battle.Waves.TotalWaves - 1;

        if (IsBoss)
        {
            BannerLabel.text = "BOSS WAVE!";
        }
        else
        {
            BannerLabel.text = "WAVE " + (WaveIndex + 1);
        }

        BannerTimer = BannerDuration;
    }

    private void HandleContinue()
    {
        // the same button goes back to the menu after the battle or resumes during the pause
        if (Battle.BattleOver)
        {
            Battle.ReturnToMenu();
        }
        else
        {
            Battle.SetPaused(false);
        }
    }

    private void HandlePauseChanged(bool Paused)
    {
        ResultPanel.SetActive(Paused);

        if (!Paused)
        {
            return;
        }

        // showing the pause menu with the loot that would be lost
        ResultTitle.text = "PAUSED";
        ResultBody.text = "IF YOU RETREAT YOU KEEP THE SECURED REWARD\nAND LOSE THE REWARD OF THIS WAVE:\n\n" + LootText(Battle.PendingLoot, "\n");
        ContinueLabel.text = "CONTINUE";
        PauseRetireButton.gameObject.SetActive(true);
    }

    private void HandleBattleEnded(BattleResult Result)
    {
        // showing the result screen
        RetireButton.interactable = false;
        PauseRetireButton.gameObject.SetActive(false);
        ResultPanel.SetActive(true);
        ContinueLabel.text = "BACK TO MENU";

        switch (Result)
        {
            case BattleResult.Victoria:
                ResultTitle.text = "VICTORY!";
                break;
            case BattleResult.Derrota:
                ResultTitle.text = "DEFEAT";
                break;
            default:
                ResultTitle.text = "RETREAT";
                break;
        }

        // showing the loot kept, or why the battle couldn't start
        if (Battle.Team.Count == 0)
        {
            ResultBody.text = "THERE IS NO ATTACK TEAM.\nBUILD ONE IN ARRANGE TEAM.";
        }
        else
        {
            ResultBody.text = "REWARD\n\n" + LootText(Battle.KeptLoot, "\n");
        }
    }
}
