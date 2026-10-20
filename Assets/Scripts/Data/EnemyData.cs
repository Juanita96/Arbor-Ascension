using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Arbor/Enemy", fileName = "NewEnemy")]
public class EnemyData : ScriptableObject
{
    // variables
    public string DisplayName;
    public bool IsBoss;

    [Header("Stats")]
    public float MaxHealth = 30f;
    public float MoveSpeed = 5f;
    public float Damage = 5f;
    public float AttackInterval = 1f;
    [Tooltip("distance from the alchemist or tree center where the enemy stops to attack")]
    [FormerlySerializedAs("AttackRange")] public float ContactDistance = 1f;
    [Tooltip("radius used to detect projectile hits")]
    public float ColliderRadius = 0.4f;

    [Header("Visual")]
    public Color Tint = Color.white;
    public float Scale = 1f;

    [Header("Reward")]
    [Tooltip("resource units given by each alchemist that hit this enemy")]
    public int ResourceReward = 1;
    [FormerlySerializedAs("CrystalReward")] public int GoldDustReward = 10;

    // references
    public EnemyUnit Prefab;
}
