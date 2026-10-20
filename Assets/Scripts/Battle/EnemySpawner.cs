using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // variables
    private readonly List<int> ActiveRows = new List<int>();
    private int NextRow;

    // functions
    public void SetActiveRows(IEnumerable<int> Rows)
    {
        // only rows with an alchemist receive enemies
        ActiveRows.Clear();
        ActiveRows.AddRange(Rows);
        NextRow = 0;
    }

    public EnemyUnit Spawn(EnemyData Data, float HealthMultiplier)
    {
        // nothing to do if there are no active rows
        if (ActiveRows.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] There are no active rows");
            return null;
        }

        // going through the active rows in order, one per enemy
        int Row = ActiveRows[NextRow % ActiveRows.Count];
        NextRow++;

        // creating the enemy at the spawn line of that row
        EnemyUnit Enemy = Instantiate(Data.Prefab, LaneManager.Instance.GetSpawnPosition(Row), Quaternion.identity);
        Enemy.Init(Data, Row, HealthMultiplier);

        return Enemy;
    }
}
