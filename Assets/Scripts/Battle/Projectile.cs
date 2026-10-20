using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // variables
    private int Row;
    private float Direction;
    private float Speed;
    private float Radius;
    private float Damage;
    private bool Piercing;
    private float SlowMultiplier = 1f;
    private float SlowDuration;

    private readonly HashSet<EnemyUnit> AlreadyHit = new HashSet<EnemyUnit>();
    private readonly List<EnemyUnit> Candidates = new List<EnemyUnit>();

    // references
    private AlchemistInfo Source;

    // functions
    public static Projectile Launch(int Row, Vector3 Origin, float Direction, float Speed, float Radius, float Damage, bool Piercing, AlchemistInfo Source, Color ProjectileColor, float SlowMultiplier = 1f, float SlowDuration = 0f)
    {
        // creating the projectile object
        string ObjectName = "Projectile";
        if (Piercing)
        {
            ObjectName = "ProjectilePiercing";
        }

        GameObject ProjectileObject = new GameObject(ObjectName);
        ProjectileObject.transform.position = Origin;
        ProjectileObject.transform.localScale = Vector3.one * Radius * 2f;

        // giving it a simple colored sprite
        SpriteRenderer ProjectileRenderer = ProjectileObject.AddComponent<SpriteRenderer>();
        ProjectileRenderer.sprite = HealthBar.GetWhiteSprite();
        ProjectileRenderer.color = ProjectileColor;
        ProjectileRenderer.sortingOrder = 8;

        // saving its settings
        Projectile NewProjectile = ProjectileObject.AddComponent<Projectile>();
        NewProjectile.Row = Row;
        NewProjectile.Direction = Direction;
        NewProjectile.Speed = Speed;
        NewProjectile.Radius = Radius;
        NewProjectile.Damage = Damage;
        NewProjectile.Piercing = Piercing;
        NewProjectile.Source = Source;
        NewProjectile.SlowMultiplier = SlowMultiplier;
        NewProjectile.SlowDuration = SlowDuration;

        return NewProjectile;
    }

    private void Update()
    {
        LaneManager Lane = LaneManager.Instance;
        float PreviousX = transform.position.x;
        float NextX = PreviousX + Direction * Speed * Time.deltaTime;

        // stopping at the spawn line instead of going past it
        float DistanceToEnd = (Lane.SpawnX - PreviousX) * Direction;
        bool ReachesEnd = DistanceToEnd <= Speed * Time.deltaTime;
        if (ReachesEnd)
        {
            NextX = Lane.SpawnX;
        }

        // checking the whole path of this frame, not only the final position
        float MinX = Mathf.Min(PreviousX, NextX);
        float MaxX = Mathf.Max(PreviousX, NextX);

        Candidates.Clear();
        IReadOnlyList<EnemyUnit> Enemies = Lane.GetEnemies(Row);
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyUnit Enemy = Enemies[i];

            // skipping dead enemies and enemies this projectile already hit
            if (Enemy.Health.IsDead || AlreadyHit.Contains(Enemy))
            {
                continue;
            }

            // the enemy is touched if its collider overlaps the path
            float Reach = Radius + Enemy.Data.ColliderRadius;
            float EnemyX = Enemy.transform.position.x;
            if (EnemyX >= MinX - Reach && EnemyX <= MaxX + Reach)
            {
                Candidates.Add(Enemy);
            }
        }

        if (Candidates.Count > 0)
        {
            if (Piercing)
            {
                // piercing projectiles hit everyone they touch and keep going
                for (int i = 0; i < Candidates.Count; i++)
                {
                    Hit(Candidates[i]);
                }
            }
            else
            {
                // normal projectiles hit only the first enemy and disappear
                Hit(FirstAlongPath(PreviousX));
                Destroy(gameObject);
                return;
            }
        }

        // moving forward
        transform.position = new Vector3(NextX, transform.position.y, transform.position.z);

        if (ReachesEnd)
        {
            Destroy(gameObject);
        }
    }

    private EnemyUnit FirstAlongPath(float FromX)
    {
        // finding the closest enemy in the travel direction
        // if two are almost at the same spot, the one that appeared first wins
        EnemyUnit First = null;
        float BestDistance = float.MaxValue;

        for (int i = 0; i < Candidates.Count; i++)
        {
            EnemyUnit Enemy = Candidates[i];
            float Distance = (Enemy.transform.position.x - FromX) * Direction;

            bool IsCloser = Distance < BestDistance - 0.01f;
            bool IsTie = First != null && Mathf.Abs(Distance - BestDistance) <= 0.01f && Enemy.SpawnIndex < First.SpawnIndex;

            if (First == null || IsCloser || IsTie)
            {
                First = Enemy;
                BestDistance = Distance;
            }
        }

        return First;
    }

    private void Hit(EnemyUnit Enemy)
    {
        AlreadyHit.Add(Enemy);

        // the support projectile also slows the enemy
        if (SlowDuration > 0f)
        {
            Enemy.ApplySlow(SlowMultiplier, SlowDuration);
        }

        Enemy.TakeDamage(Damage, Source);
    }
}
