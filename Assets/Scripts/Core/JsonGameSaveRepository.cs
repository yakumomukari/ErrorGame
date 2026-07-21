using System;
using System.IO;
using UnityEngine;

public sealed class JsonGameSaveRepository : IGameSaveRepository
{
    public const int CurrentVersion = 5;
    private const string DefaultSaveFileName = "errorgame-save.json";

    public string SavePath { get; }
    public bool HasSave => File.Exists(SavePath);

    public JsonGameSaveRepository(string savePath = null)
    {
        SavePath = string.IsNullOrWhiteSpace(savePath)
            ? Path.Combine(Application.persistentDataPath, DefaultSaveFileName)
            : savePath;
    }

    public void Save(GameSaveData data)
    {
        if (data == null) return;
        data.version = CurrentVersion;
        try
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, Serialize(data));
            File.Copy(temporaryPath, SavePath, true);
            File.Delete(temporaryPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not save ErrorGame data: {exception.Message}");
        }
    }

    public bool TryLoad(out GameSaveData data)
    {
        data = null;
        if (!HasSave) return false;
        try
        {
            data = Deserialize(File.ReadAllText(SavePath));
            return IsValid(data);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load ErrorGame data: {exception.Message}");
            data = null;
            return false;
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            string temporaryPath = SavePath + ".tmp";
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not delete ErrorGame data: {exception.Message}");
        }
    }

    public string Serialize(GameSaveData data)
    {
        return JsonUtility.ToJson(data, true);
    }

    public GameSaveData Deserialize(string json)
    {
        return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<GameSaveData>(json);
    }

    public bool IsValid(GameSaveData data)
    {
        if (data == null || data.version != CurrentVersion || data.player == null ||
            data.rooms == null || (!data.beginsAtFloorStart && data.rooms.Count < 4) ||
            (data.beginsAtFloorStart && data.rooms.Count != 0) ||
            (data.beginsAtFloorStart && data.floorNumber < 1) ||
            data.openedSecretPassages == null)
        {
            return false;
        }

        PlayerSaveData player = data.player;
        if (player.maxHealthUnits < 1 || player.currentHealthUnits < 1 ||
            player.currentHealthUnits > player.maxHealthUnits || player.coins < 0 || player.bombs < 0 ||
            player.moveSpeed < 0f || player.fireRate <= 0f || player.damage < 0f ||
            player.range <= 0f || player.projectileSpeed <= 0f)
        {
            return false;
        }

        return data.rooms.TrueForAll(room => room != null &&
            room.purchasedShopSlots != null && room.collectedSecretRewards != null);
    }
}
