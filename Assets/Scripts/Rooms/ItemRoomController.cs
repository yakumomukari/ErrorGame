using UnityEngine;

[DisallowMultipleComponent]
public sealed class ItemRoomController : MonoBehaviour, IRoomFeature
{
    [SerializeField] private NormalUpgradePickup pickupPrefab;
    [SerializeField] private Transform spawnPoint;

    public void Configure(NormalUpgradePickup normalUpgradePrefab, Transform itemSpawnPoint)
    {
        pickupPrefab = normalUpgradePrefab;
        spawnPoint = itemSpawnPoint;
    }

    public void Bind(Player player, RoomNode node, int dungeonSeed)
    {
        if (node.Type != RoomType.Item || node.IsItemClaimed) return;

        int seed = unchecked(dungeonSeed * 486187739 ^ node.Coordinate.GetHashCode() ^ 0x41A7);
        System.Random random = new System.Random(seed);
        NormalUpgradeType type = (NormalUpgradeType)random.Next(System.Enum.GetValues(typeof(NormalUpgradeType)).Length);
        NormalUpgradePickup pickup = Instantiate(pickupPrefab, spawnPoint.position, Quaternion.identity, transform);
        pickup.Initialize(player, node.Rewards, type);
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(runtimeContext.Player, node, runtimeContext.ActiveSeed);
    }

    public void OnRoomEntered()
    {
    }
}
