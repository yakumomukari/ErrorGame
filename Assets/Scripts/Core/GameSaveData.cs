using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameSaveData
{
    public int version = 5;
    public int dungeonSeed;
    public int floorNumber = 1;
    public bool beginsAtFloorStart;
    public int currentRoomX;
    public int currentRoomY;
    public bool hasCurrentRoomEntrance;
    public int currentRoomEntranceDirection;
    public PlayerSaveData player = new PlayerSaveData();
    public List<RoomSaveData> rooms = new List<RoomSaveData>();
    public List<SecretPassageSaveData> openedSecretPassages = new List<SecretPassageSaveData>();
}

[Serializable]
public sealed class PlayerSaveData
{
    public int currentHealthUnits;
    public int maxHealthUnits;
    public int coins;
    public int bombs;
    public float moveSpeed;
    public float fireRate;
    public float damage;
    public float range;
    public float projectileSpeed;
    public float luck;
}

[Serializable]
public sealed class RoomSaveData
{
    public int x;
    public int y;
    public bool visited;
    public bool cleared;
    public bool itemClaimed;
    public List<int> purchasedShopSlots = new List<int>();
    public int combatRewardType = -2;
    public bool combatRewardCollected;
    public List<int> collectedSecretRewards = new List<int>();
    public string roomVariantId;
}

[Serializable]
public sealed class SecretPassageSaveData
{
    public int sourceX;
    public int sourceY;
    public int direction;
}
