using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyUnit : MonoBehaviour
{
    // variables
    private static int NextSpawnIndex;

    // margin to consider the enemy arrived, without it distances like 1.4 (not exact in float)
    // leave the enemy stopped a tiny bit away from the contact point and it never attacks
    private const float ContactTolerance = 0.001f;

    public int Row { get; private set; }
    public int SpawnIndex { get; private set; }

    private float AttackTimer;
    private float SlowTimer;
    private float SlowMultiplier = 1f;

    // alchemists that hit this enemy at least once, each one gives its resource when it dies
    private readonly HashSet<AlchemistInfo> Participants = new HashSet<AlchemistInfo>();

    // references
    [SerializeField] private SpriteRenderer Visual;

    public EnemyData Data { get; private set; }
    public Health Health { get; private set; }

    // functions
    public void Init(EnemyData NewData, int NewRow, float HealthMultiplier)
    {
        Data = NewData;
        Row = NewRow;
        SpawnIndex = NextSpawnIndex;
        NextSpawnIndex++;

        // applying the visual of this enemy type
        if (Visual == null)
        {
            Visual = GetComponentInChildren<SpriteRenderer>();
        }

        if (Visual != null)
        {
            Visual.color = NewData.Tint;
        }

        transform.localScale = Vector3.one * NewData.Scale;

        // setting the health
        Health = GetComponent<Health>();
        Health.SetMax(NewData.MaxHealth * HealthMultiplier);
        Health.OnDeath += HandleDeath;

        // the timer starts ready, so the first hit happens as soon as it reaches contact
        AttackTimer = NewData.AttackInterval;

        LaneManager.Instance.RegisterEnemy(this);
    }

    public void TakeDamage(float Amount, AlchemistInfo Source)
    {
        if (Health.IsDead)
        {
            return;
        }

        // remembering who hit this enemy for the rewards
        if (Source != null)
        {
            Participants.Add(Source);
        }

        Health.TakeDamage(Amount);
    }

    public void ApplySlow(float Multiplier, float Duration)
    {
        // the slow doesn't stack, a new hit only restarts the duration
        SlowMultiplier = Multiplier;
        SlowTimer = Duration;
    }

    private void Update()
    {
        if (Data == null || Health.IsDead)
        {
            return;
        }

        LaneManager Lane = LaneManager.Instance;
        float MoveDirection = Lane.Direction;
        float X = transform.position.x;
        float DeltaTime = Time.deltaTime;

        // updating timers
        AttackTimer = Mathf.Min(Data.AttackInterval, AttackTimer + DeltaTime);
        if (SlowTimer > 0f)
        {
            SlowTimer -= DeltaTime;
        }

        // checking if there is an alive alchemist in front of this enemy
        float StopDistance;
        CharacterUnit Blocker = Lane.GetAliveCharacter(Row);
        bool BlockerIsAhead = Blocker != null && (Blocker.transform.position.x - X) * MoveDirection >= 0f;

        if (BlockerIsAhead)
        {
            // stopping at contact distance and attacking the alchemist
            StopDistance = (Blocker.transform.position.x - X) * MoveDirection - Data.ContactDistance;
            if (StopDistance <= ContactTolerance)
            {
                if (TryAttack())
                {
                    Blocker.Health.TakeDamage(Data.Damage);
                }

                return;
            }
        }
        else
        {
            // no alchemist in this row, so it walks to the tree and attacks it
            StopDistance = (Lane.TowerX - X) * MoveDirection - Data.ContactDistance;
            if (StopDistance <= ContactTolerance)
            {
                if (TryAttack() && BattleManager.Instance != null)
                {
                    BattleManager.Instance.DamageTree(Row, Data.Damage);
                }

                return;
            }
        }

        // moving forward, slower if it's slowed, without going past the contact point
        float Speed = Data.MoveSpeed;
        if (SlowTimer > 0f)
        {
            Speed *= SlowMultiplier;
        }

        float Step = Mathf.Min(Speed * DeltaTime, StopDistance);
        transform.position += new Vector3(MoveDirection * Step, 0f, 0f);
    }

    private bool TryAttack()
    {
        // only attacking when the timer is full
        if (AttackTimer < Data.AttackInterval)
        {
            return false;
        }

        AttackTimer = 0f;
        return true;
    }

    // events
    public event Action<EnemyUnit> OnDied;

    private void HandleDeath(Health DeadHealth)
    {
        LaneManager.Instance.UnregisterEnemy(this);

        // giving the rewards to the loot of the current wave
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.AddLoot(Participants, Data.ResourceReward, Data.GoldDustReward);
        }

        if (OnDied != null)
        {
            OnDied.Invoke(this);
        }

        Destroy(gameObject);
    }
}
