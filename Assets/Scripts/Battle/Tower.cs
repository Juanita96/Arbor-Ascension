using UnityEngine;

[RequireComponent(typeof(Health))]
public class Tower : MonoBehaviour
{
    // variables
    public int Level { get; private set; }

    // references
    [Tooltip("optional, if empty the one in the game config is used")]
    [SerializeField] private TowerConfig Config;
    [Tooltip("optional, where the visual of each level is created")]
    [SerializeField] private Transform VisualRoot;

    public Health Health { get; private set; }

    // functions
    private void Awake()
    {
        Health = GetComponent<Health>();

        // using the assigned config or the one in the game config
        TowerConfig TowerConfiguration = Config;
        if (TowerConfiguration == null)
        {
            TowerConfiguration = GameConfig.Instance.Tower;
        }

        if (TowerConfiguration == null)
        {
            Debug.LogWarning("[Tower] There is no TowerConfig assigned");
            Level = 1;
            return;
        }

        // getting the tree level from the inventory
        Level = Mathf.Clamp(PlayerInventory.Instance.TowerLevel, 1, TowerConfiguration.MaxLevel);

        // setting the health of that level
        TowerConfig.TowerLevel LevelData = TowerConfiguration.Get(Level);
        Health.SetMax(LevelData.MaxHealth);

        // creating the visual of the level if there is one
        if (LevelData.VisualPrefab != null)
        {
            Transform Parent = VisualRoot;
            if (Parent == null)
            {
                Parent = transform;
            }

            Instantiate(LevelData.VisualPrefab, Parent);
        }
    }
}
