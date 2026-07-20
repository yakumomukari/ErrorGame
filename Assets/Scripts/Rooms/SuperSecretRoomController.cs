using UnityEngine;

[DisallowMultipleComponent]
public sealed class SuperSecretRoomController : MonoBehaviour, IRoomFeature
{
    [SerializeField] private SuperMushroomPickup mushroomPrefab;
    [SerializeField] private Transform spawnPoint;

    public void Configure(SuperMushroomPickup superMushroomPrefab, Transform mushroomSpawnPoint)
    {
        mushroomPrefab = superMushroomPrefab;
        spawnPoint = mushroomSpawnPoint;
    }

    public void Bind(Player player, RoomNode node)
    {
        if (node.Type != RoomType.SuperSecret || node.IsItemClaimed ||
            mushroomPrefab == null || spawnPoint == null)
        {
            return;
        }

        SuperMushroomPickup mushroom = Instantiate(
            mushroomPrefab,
            spawnPoint.position,
            Quaternion.identity,
            transform);
        mushroom.Initialize(player, node.Rewards);
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(runtimeContext.Player, node);
    }

    public void OnRoomEntered()
    {
    }
}
