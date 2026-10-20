using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class CharacterUnit : MonoBehaviour
{
    // variables
    private const float ProjectileOffset = 0.5f;

    private const float AttackProjectileSpeed = 12f;
    private const float AttackProjectileRadius = 0.2f;
    private const float PiercingSpeed = 15f;
    private const float PiercingRadius = 0.5f;
    private const float AttackPassiveThreshold = 0.5f;
    private const float AttackPassiveBonus = 0.2f;

    private const float SupportProjectileSpeed = 10f;
    private const float SupportProjectileRadius = 0.2f;
    private const float SlowMultiplier = 0.7f;
    private const float SlowDuration = 2f;

    private const float DefenseMeleeRange = 1.5f;
    private const float ShieldDuration = 5f;
    private const float ShieldReduction = 0.5f;
    private const float DefenseRegenPerSecond = 1f;

    public const float ClickRadius = 0.5f;

    [Tooltip("height in world units the alchemist image is scaled to")]
    [SerializeField] private float VisualHeight = 1.4f;
    [SerializeField] private Color SelectedTint = new Color(1f, 0.92f, 0.5f);

    public int Row { get; private set; }
    public bool IsLeader { get; private set; }
    public bool IsAlive => Health != null && !Health.IsDead;
    public float CooldownRemaining { get; private set; }
    public float CooldownDuration { get; private set; }
    public bool ShieldActive => ShieldTimer > 0f;

    private TeamBonuses Bonuses = TeamBonuses.None;
    private float AttackTimer;
    private float ShieldTimer;
    private Color BaseColor = Color.white;

    // references
    [SerializeField] private SpriteRenderer Visual;

    public AlchemistInfo Data { get; private set; }
    public Health Health { get; private set; }

    // functions
    public void Init(AlchemistInfo NewData, int NewRow, TeamBonuses NewBonuses, bool NewIsLeader)
    {
        Data = NewData;
        Row = NewRow;
        Bonuses = NewBonuses;
        IsLeader = NewIsLeader;
        name = "Alchemist_" + NewData.DisplayName;

        // showing the alchemist image scaled to the right height
        if (Visual == null)
        {
            Visual = GetComponentInChildren<SpriteRenderer>();
        }

        if (Visual != null)
        {
            if (NewData.image != null)
            {
                Visual.sprite = NewData.image;
                Visual.color = Color.white;

                float ImageHeight = NewData.image.bounds.size.y;
                if (ImageHeight > 0f)
                {
                    Visual.transform.localScale = Vector3.one * (VisualHeight / ImageHeight);
                }
            }

            BaseColor = Visual.color;
        }

        // the defense leader has more max health
        float MaxHealth = NewData.MaxHealth;
        if (NewData.Kit == Kit.Defensa && NewIsLeader)
        {
            MaxHealth = NewData.LeaderValue;
        }

        Health = GetComponent<Health>();
        Health.SetMax(MaxHealth);
        Health.DamageTakenMultiplier = 1f;
        Health.OnDeath += HandleDeath;

        // the attack timer and the active start ready
        AttackTimer = NewData.BasicInterval;
        CooldownDuration = NewData.ActiveCooldown * NewBonuses.CooldownMultiplier;
        CooldownRemaining = 0f;
    }

    private void Update()
    {
        if (Data == null || !IsAlive)
        {
            return;
        }

        float DeltaTime = Time.deltaTime;

        // updating timers
        CooldownRemaining = Mathf.Max(0f, CooldownRemaining - DeltaTime);
        AttackTimer = Mathf.Min(Data.BasicInterval, AttackTimer + DeltaTime);

        // removing the shield when its time is over
        if (ShieldTimer > 0f)
        {
            ShieldTimer -= DeltaTime;

            if (ShieldTimer <= 0f)
            {
                Health.DamageTakenMultiplier = 1f;
            }
        }

        // the defense passive regenerates health every second
        if (Data.Kit == Kit.Defensa)
        {
            Health.Heal(DefenseRegenPerSecond * DeltaTime);
        }

        // attacking when the timer is full and there is a target
        if (AttackTimer >= Data.BasicInterval && TryBasicAttack())
        {
            AttackTimer = 0f;
        }
    }

    private float BasicDamage()
    {
        // adding every bonus that affects the basic attack
        float Bonus = Bonuses.Synergy + Bonuses.ReserveBasic;

        // the attack passive gives extra damage while health is at 50% or more
        if (Data.Kit == Kit.Ataque && Health.Current >= Health.Max * AttackPassiveThreshold)
        {
            Bonus += AttackPassiveBonus;
        }

        return Data.BasicDamage * (1f + Bonus);
    }

    private bool TryBasicAttack()
    {
        LaneManager Lane = LaneManager.Instance;
        float X = transform.position.x;

        switch (Data.Kit)
        {
            case Kit.Ataque:
                // shooting a projectile if there is any enemy in the row
                if (Lane.GetFirstEnemy(Row, X, float.MaxValue) == null)
                {
                    return false;
                }

                Launch(AttackProjectileSpeed, AttackProjectileRadius, BasicDamage(), false, new Color(1f, 0.6f, 0.3f), 1f, 0f);
                return true;

            case Kit.Apoyo:
                // shooting a projectile that also slows
                if (Lane.GetFirstEnemy(Row, X, float.MaxValue) == null)
                {
                    return false;
                }

                Launch(SupportProjectileSpeed, SupportProjectileRadius, BasicDamage(), false, new Color(0.45f, 0.7f, 1f), SlowMultiplier, SlowDuration);
                return true;

            case Kit.Defensa:
                // hitting the closest enemy in melee range
                EnemyUnit Target = Lane.GetFirstEnemy(Row, X, DefenseMeleeRange);
                if (Target == null)
                {
                    return false;
                }

                Target.TakeDamage(BasicDamage(), Data);
                return true;

            default:
                // the reserve alchemist doesn't fight
                return false;
        }
    }

    private void Launch(float Speed, float Radius, float Damage, bool Piercing, Color ProjectileColor, float Slow, float Duration)
    {
        // projectiles travel towards the spawn line
        float ShootDirection = -LaneManager.Instance.Direction;
        Vector3 Origin = transform.position + new Vector3(ShootDirection * ProjectileOffset, 0f, 0f);

        Projectile.Launch(Row, Origin, ShootDirection, Speed, Radius, Damage, Piercing, Data, ProjectileColor, Slow, Duration);
    }

    // used when the player presses e with this alchemist selected
    // if it's on cooldown nothing happens, if it has no useful effect it's used anyway
    public bool TryUseActive()
    {
        if (!IsAlive || CooldownRemaining > 0f || Data.Kit == Kit.Reserva)
        {
            return false;
        }

        CooldownRemaining = CooldownDuration;

        // the leader replaces the base value of the active
        float Value = Data.ActiveValue;
        if (IsLeader)
        {
            Value = Data.LeaderValue;
        }

        switch (Data.Kit)
        {
            case Kit.Ataque:
                // piercing projectile that hits every enemy in the row
                Launch(PiercingSpeed, PiercingRadius, Value * (1f + Bonuses.Synergy), true, new Color(1f, 0.35f, 0.2f), 1f, 0f);
                break;

            case Kit.Apoyo:
                // healing every alive alchemist of the team
                float HealAmount = Value * (1f + Bonuses.Synergy);
                foreach (CharacterUnit Ally in LaneManager.Instance.GetAliveCharacters())
                {
                    Ally.Health.Heal(HealAmount);
                }
                break;

            case Kit.Defensa:
                // taking less damage for a few seconds
                ShieldTimer = ShieldDuration;
                Health.DamageTakenMultiplier = 1f - ShieldReduction;
                break;
        }

        return true;
    }

    public void SetSelected(bool Selected)
    {
        if (Visual == null)
        {
            return;
        }

        // tinting the alchemist while it's selected
        if (Selected)
        {
            Visual.color = SelectedTint;
        }
        else
        {
            Visual.color = BaseColor;
        }
    }

    // events
    public event Action<CharacterUnit> OnDied;

    private void HandleDeath(Health DeadHealth)
    {
        ShieldTimer = 0f;

        if (OnDied != null)
        {
            OnDied.Invoke(this);
        }

        gameObject.SetActive(false);
    }
}
