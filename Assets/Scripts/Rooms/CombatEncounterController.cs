using System;
using System.Collections.Generic;
using UnityEngine;

public enum CombatEncounterState
{
    Dormant,
    Active,
    Cleared
}

[DisallowMultipleComponent]
public sealed class CombatEncounterController : MonoBehaviour, IRoomFeature, IRoomLockSource
{
    [Header("Room References")]
    [SerializeField] private Transform enemyParent;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform rewardSpawnPoint;

    [Header("Spawn Prefabs")]
    [SerializeField] private EnemySpawnCatalog enemySpawnCatalog;
    [SerializeField] private EnemySpawnTable enemySpawnTable;
    [SerializeField] private BasicResourcePickup[] rewardPrefabs;

    private readonly List<EnemyHealth> livingEnemies = new List<EnemyHealth>();
    private Player player;
    private RoomNode roomNode;
    private int encounterSeed;

    public CombatEncounterState State { get; private set; } = CombatEncounterState.Dormant;
    public int ActiveEnemyCount => livingEnemies.Count;
    public IReadOnlyList<EnemyHealth> LivingEnemies => livingEnemies;
    public EnemySpawnCatalog SpawnCatalog => enemySpawnCatalog;
    public EnemySpawnTable SpawnTable => enemySpawnCatalog != null
        ? enemySpawnCatalog.SpawnTable
        : enemySpawnTable;
    public bool LocksRoom => roomNode != null && roomNode.Type == RoomType.Combat && State != CombatEncounterState.Cleared;
    public event Action Cleared;

    public void Configure(
        Transform enemiesRoot,
        Transform[] enemySpawnPoints,
        Transform rewardPoint,
        EnemySpawnTable spawnTable,
        BasicResourcePickup[] basicRewardPrefabs)
    {
        enemyParent = enemiesRoot;
        spawnPoints = enemySpawnPoints;
        rewardSpawnPoint = rewardPoint;
        enemySpawnTable = spawnTable;
        rewardPrefabs = basicRewardPrefabs;
    }

    public void SetSpawnCatalog(EnemySpawnCatalog catalog)
    {
        enemySpawnCatalog = catalog;
    }

    public void Bind(Player playerReference, RoomNode node, int dungeonSeed)
    {
        player = playerReference;
        roomNode = node;
        encounterSeed = CalculateEncounterSeed(dungeonSeed, node.Coordinate);
        State = node.IsCleared ? CombatEncounterState.Cleared : CombatEncounterState.Dormant;
        if (node.Type == RoomType.Combat && node.IsCleared) SpawnRoomReward();
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(runtimeContext.Player, node, runtimeContext.ActiveSeed);
    }

    public void OnRoomEntered()
    {
        if (roomNode != null && roomNode.Type == RoomType.Combat && !roomNode.IsCleared) BeginCombat();
    }

    public void BeginCombat()
    {
        if (State != CombatEncounterState.Dormant || player == null) return;

        State = CombatEncounterState.Active;
        for (int slotIndex = 0; slotIndex < spawnPoints.Length; slotIndex++)
        {
            EnemyHealth health = SpawnWeightedEnemy(spawnPoints[slotIndex], slotIndex);
            if (health == null) continue;
            health.Died += OnEnemyDied;
            livingEnemies.Add(health);
        }

        if (livingEnemies.Count == 0) CompleteEncounter();
    }

    private EnemyHealth SpawnWeightedEnemy(Transform spawnPoint, int slotIndex)
    {
        if (spawnPoint == null) return null;
        EnemySpawnTable activeSpawnTable = SpawnTable;
        int totalWeight = activeSpawnTable != null ? activeSpawnTable.TotalWeight : 0;
        if (totalWeight <= 0) return null;

        int slotSeed = unchecked(encounterSeed * 486187739 + slotIndex * 16777619);
        int roll = new System.Random(slotSeed).Next(totalWeight);
        if (!activeSpawnTable.TrySelect(roll, out MonoBehaviour enemyPrefab)) return null;

        MonoBehaviour enemyBehaviour = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity,
            enemyParent);
        if (!(enemyBehaviour is ICombatEnemy enemy))
        {
            Destroy(enemyBehaviour.gameObject);
            return null;
        }

        enemy.Initialize(player);
        return enemy.Health;
    }

    private void OnEnemyDied(EnemyHealth enemy)
    {
        enemy.Died -= OnEnemyDied;
        livingEnemies.Remove(enemy);
        if (State == CombatEncounterState.Active && livingEnemies.Count == 0)
        {
            CompleteEncounter();
        }
    }

    private void CompleteEncounter()
    {
        State = CombatEncounterState.Cleared;
        SpawnRoomReward();
        Cleared?.Invoke();
    }

    private void SpawnRoomReward()
    {
        if (rewardPrefabs == null || rewardPrefabs.Length == 0 || rewardSpawnPoint == null) return;

        // Zero represents the valid "no reward" result; the remaining values map
        // one-to-one to the configured coin, heart, and bomb prefabs.
        if (roomNode == null || roomNode.IsCombatRewardCollected) return;
        if (roomNode.CombatRewardType == -2)
        {
            int roll = new System.Random(unchecked(encounterSeed ^ 0x4D3A91)).Next(rewardPrefabs.Length + 1);
            roomNode.SetCombatReward(roll - 1);
        }
        if (roomNode.CombatRewardType < 0 || roomNode.CombatRewardType >= rewardPrefabs.Length) return;

        BasicResourcePickup reward = Instantiate(
            rewardPrefabs[roomNode.CombatRewardType],
            rewardSpawnPoint.position,
            Quaternion.identity,
            transform);
        reward.Collected += roomNode.MarkCombatRewardCollected;
    }

    private void OnDestroy()
    {
        foreach (EnemyHealth enemy in livingEnemies)
        {
            if (enemy != null) enemy.Died -= OnEnemyDied;
        }
    }

    private static int CalculateEncounterSeed(int dungeonSeed, RoomCoordinate coordinate)
    {
        unchecked
        {
            int hash = dungeonSeed;
            hash = hash * 397 ^ coordinate.X;
            hash = hash * 397 ^ coordinate.Y;
            return hash;
        }
    }
}
