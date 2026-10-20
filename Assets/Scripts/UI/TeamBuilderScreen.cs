using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TeamBuilderScreen : MonoBehaviour
{
    // variables
    [Header("Card colors")]
    [SerializeField] private Color FreeColor = new Color(0.22f, 0.25f, 0.31f);
    [SerializeField] private Color AttackColor = new Color(0.20f, 0.33f, 0.25f);
    [SerializeField] private Color ReserveColor = new Color(0.20f, 0.26f, 0.38f);

    private readonly List<AlchemistInfo> Attack = new List<AlchemistInfo>();
    private readonly List<AlchemistInfo> Reserve = new List<AlchemistInfo>();
    private AlchemistInfo AttackLeader;
    private AlchemistInfo ReserveLeader;

    // references
    [SerializeField] private TeamSlot[] AttackSlots;
    [SerializeField] private TeamSlot[] ReserveSlots;
    [SerializeField] private Transform OwnedGrid;
    [SerializeField] private AlchemistCard CardPrefab;
    [SerializeField] private TextMeshProUGUI MessageLabel;
    [SerializeField] private Button BackButton;
    [SerializeField] private Button BattleButton;

    private PlayerInventory Inventory;

    // functions
    private void Awake()
    {
        // copying the current teams from the inventory
        Inventory = PlayerInventory.Instance;
        Attack.AddRange(Inventory.AttackTeam);
        Reserve.AddRange(Inventory.ReserveTeam);
        AttackLeader = Inventory.AttackLeader;
        ReserveLeader = Inventory.ReserveLeader;

        // subscribing buttons
        BackButton.onClick.AddListener(GoBack);
        BattleButton.onClick.AddListener(GoToBattle);

        ShowMessage("");
        Refresh();
    }

    private void Refresh()
    {
        // updating both teams
        RefreshSlots(AttackSlots, Attack, AttackLeader, true);
        RefreshSlots(ReserveSlots, Reserve, ReserveLeader, false);

        // recreating the list of owned alchemists
        UIUtils.ClearChildren(OwnedGrid);

        IReadOnlyList<AlchemistInfo> OwnedAssets = Inventory.Owned;
        if (OwnedAssets.Count == 0)
        {
            ShowMessage("YOU HAVE NO ALCHEMISTS!! TRY YOUR LUCK WITH THE GACHA MACHINE!!");
            return;
        }

        for (int i = 0; i < OwnedAssets.Count; i++)
        {
            AlchemistInfo CurrentInfo = OwnedAssets[i];

            // showing in which team the alchemist is
            string Footer = "";
            Color CardColor = FreeColor;

            if (Attack.Contains(CurrentInfo))
            {
                Footer = "IN ATTACK";
                CardColor = AttackColor;
            }
            else if (Reserve.Contains(CurrentInfo))
            {
                Footer = "IN RESERVE";
                CardColor = ReserveColor;
            }

            // left click on the card adds it to its team
            AlchemistCard Card = Instantiate(CardPrefab, OwnedGrid);
            Card.Setup(CurrentInfo, Footer, CardColor, () => Add(CurrentInfo));
        }
    }

    private void RefreshSlots(TeamSlot[] Slots, List<AlchemistInfo> Team, AlchemistInfo Leader, bool IsAttack)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            // empty slots show their row or reserve number
            if (i >= Team.Count)
            {
                string SlotName = "RESERVE ";
                if (IsAttack)
                {
                    SlotName = "ROW ";
                }

                Slots[i].ShowEmpty(SlotName + (i + 1) + "\nEMPTY");
                continue;
            }

            AlchemistInfo CurrentInfo = Team[i];

            // attack alchemists show the row they use
            string Prefix = "";
            if (IsAttack)
            {
                Prefix = "ROW " + (i + 1) + " - ";
            }

            Slots[i].Show(CurrentInfo, Prefix, CurrentInfo == Leader, () => SetLeader(CurrentInfo, IsAttack), () => Remove(CurrentInfo));
        }
    }

    // the reserve alchemist goes to the reserve, the others go to the attack team
    private void Add(AlchemistInfo Info)
    {
        // an alchemist can only be in one team
        if (Attack.Contains(Info) || Reserve.Contains(Info))
        {
            ShowMessage(Info.DisplayName + " IS ALREADY ON A TEAM. RIGHT CLICK TO REMOVE.");
            return;
        }

        if (Info.IsReserveOnly)
        {
            if (Reserve.Count >= PlayerInventory.MaxReserveTeam)
            {
                ShowMessage("RESERVE IS FULL!!");
                return;
            }

            Reserve.Add(Info);
        }
        else
        {
            if (Attack.Count >= PlayerInventory.MaxAttackTeam)
            {
                ShowMessage("ATTACK TEAM IS FULL!!");
                return;
            }

            Attack.Add(Info);
        }

        Commit();
    }

    private void Remove(AlchemistInfo Info)
    {
        // removing it moves the others up one row
        Attack.Remove(Info);
        Reserve.Remove(Info);
        Commit();
    }

    private void SetLeader(AlchemistInfo Info, bool IsAttack)
    {
        if (IsAttack)
        {
            AttackLeader = Info;
        }
        else
        {
            ReserveLeader = Info;
        }

        Commit();
    }

    private void Commit()
    {
        // saving the teams in the inventory
        Inventory.SetTeams(Attack, Reserve, AttackLeader, ReserveLeader);

        // the inventory may pick a new leader if the old one left the team
        AttackLeader = Inventory.AttackLeader;
        ReserveLeader = Inventory.ReserveLeader;

        ShowMessage("");
        Refresh();
    }

    private void ShowMessage(string Message)
    {
        MessageLabel.text = Message;
    }

    // events
    private void GoBack()
    {
        SceneManager.LoadScene(GameConfig.Instance.PlayMenuScene);
    }

    private void GoToBattle()
    {
        // a battle can't start without attack team
        if (Attack.Count == 0)
        {
            ShowMessage("YOU NEED AT LEAST ONE ALCHEMIST IN YOUR ATTACK TEAM!!");
            return;
        }

        SceneManager.LoadScene(GameConfig.Instance.CombatScene);
    }
}
