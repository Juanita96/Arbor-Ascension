using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    // variables
    public static LaneManager Instance { get; private set; }

    public int RowCount => RowAnchors.Length;
    public float TowerX => TowerLine.position.x;
    public float SpawnX => SpawnLine.position.x;

    // +1 if enemies move to the right, -1 if they move to the left
    public float Direction => Mathf.Sign(TowerLine.position.x - SpawnLine.position.x);

    private List<EnemyUnit>[] EnemiesByRow;
    private CharacterUnit[] CharacterByRow;

    // references
    [Tooltip("one transform per row, its position is where the alchemist of that row stands")]
    [SerializeField] private Transform[] RowAnchors;
    [Tooltip("only its x is used, enemies appear here")]
    [SerializeField] private Transform SpawnLine;
    [Tooltip("only its x is used, it's the center of the tree")]
    [SerializeField] private Transform TowerLine;

    // functions
    private void Awake()
    {
        Instance = this;

        // creating an empty enemy list for each row
        EnemiesByRow = new List<EnemyUnit>[RowAnchors.Length];
        for (int i = 0; i < RowAnchors.Length; i++)
        {
            EnemiesByRow[i] = new List<EnemyUnit>();
        }

        CharacterByRow = new CharacterUnit[RowAnchors.Length];
    }

    public Vector3 GetCharacterPosition(int Row)
    {
        return RowAnchors[Row].position;
    }

    public Vector3 GetSpawnPosition(int Row)
    {
        // spawn line x with the height of the row
        return new Vector3(SpawnLine.position.x, RowAnchors[Row].position.y, 0f);
    }

    public void RegisterEnemy(EnemyUnit Enemy)
    {
        EnemiesByRow[Enemy.Row].Add(Enemy);
    }

    public void UnregisterEnemy(EnemyUnit Enemy)
    {
        EnemiesByRow[Enemy.Row].Remove(Enemy);
    }

    public IReadOnlyList<EnemyUnit> GetEnemies(int Row)
    {
        return EnemiesByRow[Row];
    }

    // closest alive enemy between the spawn line and the given x, inside the range
    // if two are almost at the same spot, the one that appeared first wins
    public EnemyUnit GetFirstEnemy(int Row, float FromX, float Range)
    {
        EnemyUnit Closest = null;
        float ClosestDistance = float.MaxValue;

        List<EnemyUnit> Enemies = EnemiesByRow[Row];
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyUnit Enemy = Enemies[i];

            // skipping dead enemies
            if (Enemy.Health.IsDead)
            {
                continue;
            }

            // skipping enemies behind the given x or out of range
            float Distance = (FromX - Enemy.transform.position.x) * Direction;
            if (Distance < 0f || Distance > Range)
            {
                continue;
            }

            // keeping the closest one, using the spawn order to break ties
            bool IsCloser = Distance < ClosestDistance - 0.01f;
            bool IsTie = Closest != null && Mathf.Abs(Distance - ClosestDistance) <= 0.01f && Enemy.SpawnIndex < Closest.SpawnIndex;

            if (Closest == null || IsCloser || IsTie)
            {
                ClosestDistance = Distance;
                Closest = Enemy;
            }
        }

        return Closest;
    }

    public void SetCharacter(int Row, CharacterUnit Character)
    {
        CharacterByRow[Row] = Character;
    }

    public CharacterUnit GetAliveCharacter(int Row)
    {
        // only returning the alchemist if it's alive
        CharacterUnit Character = CharacterByRow[Row];

        if (Character != null && Character.IsAlive)
        {
            return Character;
        }

        return null;
    }

    public IEnumerable<CharacterUnit> GetAliveCharacters()
    {
        // going through every row and returning the alive alchemists
        for (int i = 0; i < CharacterByRow.Length; i++)
        {
            CharacterUnit Character = GetAliveCharacter(i);

            if (Character != null)
            {
                yield return Character;
            }
        }
    }
}
