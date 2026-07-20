using System.Collections.Generic;
using UnityEngine;

public sealed class RoomPresentationDefinition
{
    public string MapMarker { get; }
    public Color RoomColor { get; }
    public Color MapColor { get; }

    public RoomPresentationDefinition(string mapMarker, Color roomColor, Color mapColor)
    {
        MapMarker = mapMarker;
        RoomColor = roomColor;
        MapColor = mapColor;
    }
}

public static class RoomPresentationCatalog
{
    private static readonly RoomPresentationDefinition Fallback = new RoomPresentationDefinition(
        string.Empty,
        Color.clear,
        new Color(0.58f, 0.68f, 0.82f, 0.9f));

    private static readonly Dictionary<RoomType, RoomPresentationDefinition> Definitions =
        new Dictionary<RoomType, RoomPresentationDefinition>
        {
            [RoomType.Start] = new RoomPresentationDefinition(
                "S",
                new Color(0.25f, 0.72f, 1f, 0.35f),
                new Color(0.36f, 0.68f, 0.94f, 0.95f)),
            [RoomType.Combat] = new RoomPresentationDefinition(
                string.Empty,
                new Color(0.8f, 0.16f, 0.18f, 0.25f),
                new Color(0.58f, 0.68f, 0.82f, 0.9f)),
            [RoomType.Item] = new RoomPresentationDefinition(
                "I",
                new Color(0.65f, 0.32f, 0.92f, 0.4f),
                new Color(0.68f, 0.38f, 0.94f, 0.95f)),
            [RoomType.Shop] = new RoomPresentationDefinition(
                "$",
                new Color(1f, 0.76f, 0.12f, 0.4f),
                new Color(1f, 0.76f, 0.12f, 0.95f)),
            [RoomType.Boss] = new RoomPresentationDefinition(
                "B",
                new Color(0.86f, 0.08f, 0.12f, 0.42f),
                new Color(0.92f, 0.12f, 0.16f, 0.98f)),
            [RoomType.Secret] = new RoomPresentationDefinition(
                "?",
                new Color(0.48f, 0.24f, 0.68f, 0.32f),
                new Color(0.58f, 0.68f, 0.82f, 0.9f)),
            [RoomType.SuperSecret] = new RoomPresentationDefinition(
                "M",
                new Color(0.12f, 0.72f, 0.32f, 0.38f),
                new Color(0.18f, 0.92f, 0.48f, 0.98f))
        };

    public static RoomPresentationDefinition Get(RoomType type)
    {
        return Definitions.TryGetValue(type, out RoomPresentationDefinition definition)
            ? definition
            : Fallback;
    }
}
