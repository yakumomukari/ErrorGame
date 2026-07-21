using UnityEngine;

[DisallowMultipleComponent]
public sealed class SecretRoomController : MonoBehaviour, IRoomFeature
{
    [SerializeField] private BasicResourcePickup[] rewardPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    public void Configure(BasicResourcePickup[] basicRewardPrefabs, Transform[] rewardSpawnPoints)
    {
        rewardPrefabs = basicRewardPrefabs;
        spawnPoints = rewardSpawnPoints;
    }

    public void Bind(RoomNode node)
    {
        if (node.Type != RoomType.Secret) return;

        int rewardCount = Mathf.Min(rewardPrefabs.Length, spawnPoints.Length);
        for (int index = 0; index < rewardCount; index++)
        {
            if (node.IsSecretRewardCollected(index) || rewardPrefabs[index] == null || spawnPoints[index] == null) continue;
            int rewardIndex = index;
            BasicResourcePickup reward = Instantiate(
                rewardPrefabs[index],
                spawnPoints[index].position,
                Quaternion.identity,
                transform);
            reward.Collected += () => node.MarkSecretRewardCollected(rewardIndex);
        }
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(node);
    }

    public void OnRoomEntered()
    {
    }
}
