using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class GachaHUD : MonoBehaviour
{
    // variables
    [SerializeField] private float ResultDuration = 4f;

    private float ResultTimer;

    // references
    [FormerlySerializedAs("CrystalsLabel")]
    [SerializeField] private TextMeshProUGUI GoldDustLabel;
    [SerializeField] private TextMeshProUGUI ResultLabel;

    private PlayerInventory Inventory;

    // functions
    private void Awake()
    {
        // fetching inventory
        Inventory = PlayerInventory.Instance;

        // subscribing events
        Inventory.OnChanged += HandleInventoryChanged;
        GachaManager.OnPulled += HandlePulled;
        GachaManager.OnNotEnoughGoldDust += HandleNotEnough;

        // starting with an empty result and the current gold dust
        ResultLabel.text = "";
        HandleInventoryChanged();
    }

    private void OnDestroy()
    {
        // unsubscribing events
        if (Inventory != null)
        {
            Inventory.OnChanged -= HandleInventoryChanged;
        }

        GachaManager.OnPulled -= HandlePulled;
        GachaManager.OnNotEnoughGoldDust -= HandleNotEnough;
    }

    private void Update()
    {
        if (ResultTimer <= 0f)
        {
            return;
        }

        // clearing the result message after a few seconds
        ResultTimer -= Time.unscaledDeltaTime;
        if (ResultTimer <= 0f)
        {
            ResultLabel.text = "";
        }
    }

    private void ShowResult(string Message)
    {
        ResultLabel.text = Message;
        ResultTimer = ResultDuration;
    }

    // events
    private void HandleInventoryChanged()
    {
        // updating the gold dust and the pull cost
        GoldDustLabel.text = "GOLD DUST: " + Inventory.GoldDust + " - PULL COST: " + GameConfig.Instance.PullCost;
    }

    private void HandlePulled(AlchemistInfo Info, bool IsNew, string Rarity)
    {
        string DisplayName = "?";
        if (Info != null)
        {
            DisplayName = Info.DisplayName;
        }

        // new alchemists and duplicates show different messages
        if (IsNew)
        {
            ShowResult("NEW!! " + DisplayName + " (" + Rarity + ")");
        }
        else
        {
            ShowResult(DisplayName + " DUPLICATED: +" + GameConfig.Instance.DuplicateGoldDustReward + " Gold Dust!");
        }
    }

    private void HandleNotEnough()
    {
        ShowResult("INSUFFICIENT GOLD DUST..");
    }
}
