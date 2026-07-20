using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemySpawnEntry
{
    [SerializeField] private MonoBehaviour enemyPrefab;
    [SerializeField, Min(0)] private int weight = 1;

    public MonoBehaviour EnemyPrefab => enemyPrefab;
    public int Weight => weight;
    public bool IsUsable => enemyPrefab is ICombatEnemy && weight > 0;

    public EnemySpawnEntry(MonoBehaviour prefab, int spawnWeight)
    {
        enemyPrefab = prefab;
        weight = Mathf.Max(0, spawnWeight);
    }
}

[Serializable]
public sealed class EnemySpawnTable
{
    [SerializeField] private List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();

    public IReadOnlyList<EnemySpawnEntry> Entries => entries;

    public int TotalWeight
    {
        get
        {
            int total = 0;
            foreach (EnemySpawnEntry entry in entries)
            {
                if (entry != null && entry.IsUsable) total += entry.Weight;
            }
            return total;
        }
    }

    public EnemySpawnTable(IEnumerable<EnemySpawnEntry> spawnEntries)
    {
        if (spawnEntries != null) entries.AddRange(spawnEntries);
    }

    public bool TrySelect(int weightedRoll, out MonoBehaviour enemyPrefab)
    {
        enemyPrefab = null;
        if (weightedRoll < 0 || weightedRoll >= TotalWeight) return false;

        int remaining = weightedRoll;
        foreach (EnemySpawnEntry entry in entries)
        {
            if (entry == null || !entry.IsUsable) continue;
            if (remaining < entry.Weight)
            {
                enemyPrefab = entry.EnemyPrefab;
                return true;
            }
            remaining -= entry.Weight;
        }
        return false;
    }
}
