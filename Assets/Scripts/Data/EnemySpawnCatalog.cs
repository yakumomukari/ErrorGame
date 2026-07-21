using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemySpawnCatalog",
    menuName = "Error Game/Combat/Enemy Spawn Catalog")]
public sealed class EnemySpawnCatalog : ScriptableObject
{
    [SerializeField] private EnemySpawnTable spawnTable = new EnemySpawnTable(null);

    public EnemySpawnTable SpawnTable => spawnTable;
    public IReadOnlyList<EnemySpawnEntry> Entries => spawnTable.Entries;
    public int TotalWeight => spawnTable.TotalWeight;

    public bool TrySelect(int weightedRoll, out MonoBehaviour enemyPrefab)
    {
        return spawnTable.TrySelect(weightedRoll, out enemyPrefab);
    }

    public void Configure(IEnumerable<EnemySpawnEntry> entries)
    {
        spawnTable = new EnemySpawnTable(entries);
    }
}
