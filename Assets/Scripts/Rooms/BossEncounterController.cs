using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossEncounterController : MonoBehaviour, IRoomFeature, IRoomLockSource
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BossEnemy bossPrefab;
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private NormalUpgradePickup rewardPrefab;

    private Player player;
    private RoomNode roomNode;
    private BossEnemy activeBoss;
    private int dungeonSeed;
    private bool rewardSpawned;

    public bool LocksRoom => roomNode != null && roomNode.Type == RoomType.Boss && !roomNode.IsCleared;
    public event Action Cleared;

    public void Configure(
        Transform bossSpawnPoint,
        BossEnemy dungeonBossPrefab,
        Transform bossRewardSpawnPoint,
        NormalUpgradePickup bossRewardPrefab)
    {
        spawnPoint = bossSpawnPoint;
        bossPrefab = dungeonBossPrefab;
        rewardSpawnPoint = bossRewardSpawnPoint;
        rewardPrefab = bossRewardPrefab;
    }

    public void Bind(Player playerReference, RoomNode node, int activeDungeonSeed)
    {
        player = playerReference;
        roomNode = node;
        dungeonSeed = activeDungeonSeed;
        rewardSpawned = false;

        if (roomNode.Type == RoomType.Boss && roomNode.IsCleared && !roomNode.IsItemClaimed)
        {
            SpawnBossReward();
        }
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(runtimeContext.Player, node, runtimeContext.ActiveSeed);
    }

    public void OnRoomEntered()
    {
        if (LocksRoom) BeginBossFight();
    }

    public void BeginBossFight()
    {
        if (roomNode == null || roomNode.Type != RoomType.Boss || roomNode.IsCleared ||
            activeBoss != null || player == null || bossPrefab == null || spawnPoint == null)
        {
            return;
        }

        activeBoss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity, transform);
        activeBoss.Initialize(player);
        activeBoss.Health.Died += OnBossDied;
    }

    private void OnBossDied(EnemyHealth health)
    {
        if (health != null) health.Died -= OnBossDied;
        activeBoss = null;
        Cleared?.Invoke();
        SpawnBossReward();
    }

    private void SpawnBossReward()
    {
        if (rewardSpawned || roomNode == null || roomNode.Type != RoomType.Boss ||
            roomNode.IsItemClaimed || player == null || rewardPrefab == null || rewardSpawnPoint == null)
        {
            return;
        }

        rewardSpawned = true;
        int seed = unchecked(dungeonSeed * 486187739 ^ roomNode.Coordinate.GetHashCode() ^ 0xB055);
        System.Random random = new System.Random(seed);
        NormalUpgradeType type = (NormalUpgradeType)random.Next(System.Enum.GetValues(typeof(NormalUpgradeType)).Length);
        NormalUpgradePickup reward = Instantiate(
            rewardPrefab,
            rewardSpawnPoint.position,
            Quaternion.identity,
            transform);
        reward.Initialize(player, roomNode.Rewards, type);
    }

    private void OnDestroy()
    {
        if (activeBoss != null) activeBoss.Health.Died -= OnBossDied;
    }
}
