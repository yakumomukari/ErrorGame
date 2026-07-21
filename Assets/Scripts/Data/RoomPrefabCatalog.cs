using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RoomPrefabEntry
{
    [SerializeField] private string stableId;
    [SerializeField] private RoomType roomType;
    [SerializeField] private RoomController prefab;
    [SerializeField, Min(1)] private int weight = 1;

    public string StableId => stableId;
    public RoomType RoomType => roomType;
    public RoomController Prefab => prefab;
    public int Weight => Mathf.Max(1, weight);

    public RoomPrefabEntry(string id, RoomType type, RoomController roomPrefab, int selectionWeight = 1)
    {
        stableId = id;
        roomType = type;
        prefab = roomPrefab;
        weight = Mathf.Max(1, selectionWeight);
    }

    internal void Sanitize()
    {
        stableId = stableId != null ? stableId.Trim() : string.Empty;
        weight = Mathf.Max(1, weight);
    }
}

[CreateAssetMenu(fileName = "RoomPrefabCatalog", menuName = "ErrorGame/Rooms/Prefab Catalog")]
public sealed class RoomPrefabCatalog : ScriptableObject
{
    [SerializeField] private List<RoomPrefabEntry> entries = new List<RoomPrefabEntry>();

    public IReadOnlyList<RoomPrefabEntry> Entries
    {
        get
        {
            if (entries == null) entries = new List<RoomPrefabEntry>();
            return entries;
        }
    }

    public void Configure(IEnumerable<RoomPrefabEntry> configuredEntries)
    {
        entries = configuredEntries != null
            ? new List<RoomPrefabEntry>(configuredEntries)
            : new List<RoomPrefabEntry>();
        SanitizeEntries();
    }

    public bool TryResolve(RoomType roomType, string stableId, out RoomPrefabEntry selected)
    {
        selected = null;
        if (string.IsNullOrWhiteSpace(stableId)) return false;

        foreach (RoomPrefabEntry entry in entries)
        {
            if (!IsUsable(entry, roomType) ||
                !string.Equals(entry.StableId, stableId, StringComparison.Ordinal))
            {
                continue;
            }

            selected = entry;
            return true;
        }
        return false;
    }

    public bool TrySelect(RoomType roomType, int selectionKey, out RoomPrefabEntry selected)
    {
        selected = null;
        long totalWeight = 0;
        foreach (RoomPrefabEntry entry in entries)
        {
            if (IsUsable(entry, roomType)) totalWeight += entry.Weight;
        }
        if (totalWeight <= 0) return false;

        ulong roll = MixToUInt(selectionKey) % (ulong)totalWeight;
        foreach (RoomPrefabEntry entry in entries)
        {
            if (!IsUsable(entry, roomType)) continue;
            if (roll < (ulong)entry.Weight)
            {
                selected = entry;
                return true;
            }
            roll -= (ulong)entry.Weight;
        }
        return false;
    }

    private void OnValidate()
    {
        SanitizeEntries();
    }

    private void SanitizeEntries()
    {
        if (entries == null) entries = new List<RoomPrefabEntry>();
        foreach (RoomPrefabEntry entry in entries) entry?.Sanitize();
    }

    private static bool IsUsable(RoomPrefabEntry entry, RoomType roomType)
    {
        return entry != null && entry.RoomType == roomType && entry.Prefab != null &&
               !string.IsNullOrWhiteSpace(entry.StableId) && entry.Weight > 0;
    }

    private static ulong MixToUInt(int value)
    {
        unchecked
        {
            uint hash = (uint)value;
            hash ^= hash >> 16;
            hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            hash *= 0x846CA68Bu;
            hash ^= hash >> 16;
            return hash;
        }
    }
}
