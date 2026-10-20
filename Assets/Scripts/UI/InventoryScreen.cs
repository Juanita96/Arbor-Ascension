using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InventoryScreen : MonoBehaviour
{
    // variables
    [SerializeField] private string GachaScene = "Gacha";
    [SerializeField] private Color CardColor = new Color(0.22f, 0.25f, 0.31f);

    // references
    [SerializeField] private TextMeshProUGUI ResourcesLabel;
    [SerializeField] private TextMeshProUGUI TowerLabel;

    [SerializeField] private Button UpgradeButton;
    [SerializeField] private TextMeshProUGUI UpgradeButtonLabel;

    [SerializeField] private TextMeshProUGUI OwnedTitle;
    [SerializeField] private Transform OwnedGrid;
    [SerializeField] private AlchemistCard CardPrefab;

    [SerializeField] private Button BackButton;
    [SerializeField] private Button GachaButton;

    private PlayerInventory Inventory;

    // functions
    private void Awake()
    {
        // fetching inventory
        Inventory = PlayerInventory.Instance;

        // subscribing buttons
        BackButton.onClick.AddListener(GoBack);
        GachaButton.onClick.AddListener(GoToGacha);
        UpgradeButton.onClick.AddListener(UpgradeTower);

        // refreshing every time the inventory changes
        Inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (Inventory != null)
        {
            Inventory.OnChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        // updating text
        ResourcesLabel.text = "GOLD DUST: " + Inventory.GoldDust + " - SULPHUR: " + Inventory.GetResource(ResourceType.Sulfur) + " - MERCURIUS: " + Inventory.GetResource(ResourceType.Mercurius) + " - SALT: " + Inventory.GetResource(ResourceType.Sal);

        // updating owned count + tower
        RefreshTower();
        RefreshOwned();
    }

    private void RefreshTower()
    {
        // fetching tower configuration
        TowerConfig TowerConfiguration = GameConfig.Instance.Tower;

        if (TowerConfiguration == null)
        {
            UpgradeButton.interactable = false;
            return;
        }

        // getting level
        int Level = Mathf.Clamp(Inventory.TowerLevel, 1, TowerConfiguration.MaxLevel);

        string DesiredText = "ARBOR PHILOSOPHICA - LEVEL: " + Level + "/" + TowerConfiguration.MaxLevel + " - HP: " + TowerConfiguration.Get(Level).MaxHealth;

        // making sure level isn't the maximum
        bool IsMaxLevel = Level >= TowerConfiguration.MaxLevel;
        int NextLevel = Level + 1;

        if (IsMaxLevel)
        {
            DesiredText += "\nMAX LEVEL";
        }
        else
        {
            // telling the player if the next level unlocks the reserve alchemist
            AlchemistInfo Reserve = GameConfig.Instance.ReserveAlchemist;
            string Unlock = "";

            if (NextLevel == GameConfig.Instance.ReserveUnlockLevel && Reserve != null)
            {
                Unlock = " - UNLOCKS " + Reserve.DisplayName;
            }

            string Cost = UIUtils.CostLabel(TowerConfiguration.Get(NextLevel).UpgradeCost);
            DesiredText += $"\nUPGRADE TO LEVEL {NextLevel}{Unlock}: {Cost}";
        }

        // the button only works if there is a next level and the player can pay
        if (IsMaxLevel)
        {
            UpgradeButtonLabel.text = "MAXIMUM";
            UpgradeButton.interactable = false;
        }
        else
        {
            UpgradeButtonLabel.text = "UPGRADE";
            UpgradeButton.interactable = Inventory.CanUpgradeTower(TowerConfiguration);
        }

        TowerLabel.text = DesiredText;
    }

    private void RefreshOwned()
    {
        // fetching owned alchemists
        IReadOnlyList<AlchemistInfo> OwnedAssets = Inventory.Owned;
        OwnedTitle.text = "ALCHEMISTS (" + OwnedAssets.Count + "/" + GameConfig.Instance.AllAlchemists.Count + ")";

        // clearing display and creating one card per alchemist
        UIUtils.ClearChildren(OwnedGrid);

        for (int i = 0; i < OwnedAssets.Count; i++)
        {
            AlchemistInfo CurrentInfo = OwnedAssets[i];

            // the reserve alchemist doesn't give resources
            string Resource = UIUtils.ResourceLabel(CurrentInfo.Resource);
            if (CurrentInfo.IsReserveOnly)
            {
                Resource = "NO RESOURCE";
            }

            string Footer = Resource + " - HP " + CurrentInfo.MaxHealth;

            // showing in which team the alchemist is
            if (Inventory.AttackTeam.Contains(CurrentInfo))
            {
                Footer += "\nIN ATTACK TEAM";
            }
            else if (Inventory.ReserveTeam.Contains(CurrentInfo))
            {
                Footer += "\nIN RESERVE TEAM";
            }

            AlchemistCard AlchemistCard = Instantiate(CardPrefab, OwnedGrid);
            AlchemistCard.Setup(CurrentInfo, Footer, CardColor, null);
        }
    }

    // events
    private void GoBack()
    {
        SceneManager.LoadScene(GameConfig.Instance.PlayMenuScene);
    }

    private void GoToGacha()
    {
        SceneManager.LoadScene(GachaScene);
    }

    private void UpgradeTower()
    {
        Inventory.TryUpgradeTower(GameConfig.Instance.Tower);
    }
}
