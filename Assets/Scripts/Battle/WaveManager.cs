using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    // variables
    [Serializable]
    public class WaveEntry
    {
        public EnemyData Enemy;
        public int Count = 5;
    }

    [Serializable]
    public class Wave
    {
        public List<WaveEntry> Enemies = new List<WaveEntry>();
        public float SpawnInterval = 1.5f;
        public float HealthMultiplier = 1f;
    }

    [SerializeField] private List<Wave> Waves = new List<Wave>();
    [SerializeField] private float TimeBetweenWaves = 3f;

    public int CurrentWave { get; private set; } = -1;
    public int TotalWaves => Waves.Count;

    private int AliveEnemies;

    // references
    [SerializeField] private EnemySpawner Spawner;

    // functions
    public void StartWaves()
    {
        StartCoroutine(RunWaves());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < Waves.Count; i++)
        {
            // starting the wave
            CurrentWave = i;
            if (OnWaveStarted != null)
            {
                OnWaveStarted.Invoke(i);
            }

            // spawning every enemy and waiting until they are all dead
            yield return SpawnWave(Waves[i]);
            yield return new WaitUntil(() => AliveEnemies <= 0);

            if (OnWaveCleared != null)
            {
                OnWaveCleared.Invoke(i);
            }

            // short pause before the next wave
            if (i < Waves.Count - 1)
            {
                yield return new WaitForSeconds(TimeBetweenWaves);
            }
        }

        if (OnAllWavesCleared != null)
        {
            OnAllWavesCleared.Invoke();
        }
    }

    private IEnumerator SpawnWave(Wave CurrentWaveData)
    {
        // spawning the enemies in the order of the list
        foreach (WaveEntry Entry in CurrentWaveData.Enemies)
        {
            if (Entry.Enemy == null)
            {
                continue;
            }

            for (int i = 0; i < Entry.Count; i++)
            {
                EnemyUnit Enemy = Spawner.Spawn(Entry.Enemy, CurrentWaveData.HealthMultiplier);

                // counting the enemy so the wave knows when it's over
                if (Enemy != null)
                {
                    AliveEnemies++;
                    Enemy.OnDied += HandleEnemyDied;
                }

                yield return new WaitForSeconds(CurrentWaveData.SpawnInterval);
            }
        }
    }

    // events
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    private void HandleEnemyDied(EnemyUnit Enemy)
    {
        AliveEnemies--;
    }
}
