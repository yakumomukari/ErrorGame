using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MinimapController : MonoBehaviour
{
    [SerializeField] private RectTransform nodesRoot;
    [SerializeField] private Image nodeTemplate;
    [SerializeField] private Image connectionTemplate;

    private readonly List<GameObject> spawnedVisuals = new List<GameObject>();
    private GameSession session;

    public void Configure(RectTransform mapNodesRoot, Image roomNodeTemplate, Image roomConnectionTemplate)
    {
        nodesRoot = mapNodesRoot;
        nodeTemplate = roomNodeTemplate;
        connectionTemplate = roomConnectionTemplate;
    }

    public void Bind(GameSession gameSession)
    {
        if (session != null) session.RoomChanged -= OnRoomChanged;
        session = gameSession;
        if (session != null) session.RoomChanged += OnRoomChanged;
        Refresh();
    }

    public static bool ShouldDisplayRoom(DungeonLayout layout, RoomNode room)
    {
        if (layout == null || room == null) return false;
        if (RoomTypeUtility.IsHiddenRoom(room.Type)) return room.IsVisited;
        if (room.IsVisited) return true;

        foreach (RoomNode visitedRoom in layout.VisibleRooms)
        {
            if (!visitedRoom.IsVisited) continue;
            foreach (RoomDirection direction in visitedRoom.Connections)
            {
                if (layout.TryGetConnectedRoom(visitedRoom, direction, out RoomNode neighbor) && neighbor == room)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static string GetRoomMarker(RoomType type)
    {
        return RoomPresentationCatalog.Get(type).MapMarker;
    }

    public static bool ShouldDisplayConnection(DungeonLayout layout, RoomNode first, RoomNode second)
    {
        if (!ShouldDisplayRoom(layout, first) || !ShouldDisplayRoom(layout, second)) return false;
        foreach (RoomDirection direction in first.Connections)
        {
            if (layout.TryGetConnectedRoom(first, direction, out RoomNode neighbor) && neighbor == second)
            {
                return true;
            }
        }
        return false;
    }

    private void OnRoomChanged(RoomNode room)
    {
        Refresh();
    }

    private void Refresh()
    {
        ClearNodes();
        if (session == null || session.Layout == null || nodesRoot == null ||
            nodeTemplate == null || connectionTemplate == null) return;

        CalculateVisibleBounds(session.Layout, out int minX, out int maxX, out int minY, out int maxY);
        float width = nodesRoot.rect.width > 0f ? nodesRoot.rect.width : nodesRoot.sizeDelta.x;
        float height = nodesRoot.rect.height > 0f ? nodesRoot.rect.height : nodesRoot.sizeDelta.y;
        float step = 42f;
        if (maxX > minX) step = Mathf.Min(step, Mathf.Max(16f, (width - 32f) / (maxX - minX)));
        if (maxY > minY) step = Mathf.Min(step, Mathf.Max(16f, (height - 28f) / (maxY - minY)));
        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;
        float nodeSize = Mathf.Clamp(step * 0.68f, 14f, 28f);

        DrawConnections(session.Layout, centerX, centerY, step, nodeSize);

        foreach (RoomNode room in session.Layout.Rooms.Values)
        {
            if (!ShouldDisplayRoom(session.Layout, room)) continue;

            Image node = Instantiate(nodeTemplate, nodesRoot);
            node.gameObject.name = $"{room.Type} {room.Coordinate}";
            node.gameObject.SetActive(true);
            node.rectTransform.anchoredPosition = new Vector2(
                (room.Coordinate.X - centerX) * step,
                (room.Coordinate.Y - centerY) * step);
            node.rectTransform.sizeDelta = new Vector2(nodeSize, nodeSize * 0.78f);

            bool isCurrent = session.CurrentRoom != null && session.CurrentRoom.Node == room;
            node.color = GetNodeColor(room, isCurrent);
            node.transform.localScale = isCurrent ? Vector3.one * 1.18f : Vector3.one;

            Text marker = node.GetComponentInChildren<Text>(true);
            if (marker != null)
            {
                marker.text = GetRoomMarker(room.Type);
                marker.color = isCurrent ? new Color(0.02f, 0.08f, 0.12f) : Color.white;
            }
            spawnedVisuals.Add(node.gameObject);
        }
    }

    private void DrawConnections(DungeonLayout layout, float centerX, float centerY, float step, float nodeSize)
    {
        RoomDirection[] forwardDirections = { RoomDirection.North, RoomDirection.East };
        foreach (RoomNode room in layout.Rooms.Values)
        {
            if (!ShouldDisplayRoom(layout, room)) continue;
            foreach (RoomDirection direction in forwardDirections)
            {
                if (!layout.TryGetConnectedRoom(room, direction, out RoomNode neighbor) ||
                    !ShouldDisplayConnection(layout, room, neighbor))
                {
                    continue;
                }

                Vector2 firstPosition = GetMapPosition(room.Coordinate, centerX, centerY, step);
                Vector2 secondPosition = GetMapPosition(neighbor.Coordinate, centerX, centerY, step);
                Vector2 delta = secondPosition - firstPosition;
                Image connection = Instantiate(connectionTemplate, nodesRoot);
                connection.gameObject.name = $"Connection {room.Coordinate} to {neighbor.Coordinate}";
                connection.gameObject.SetActive(true);
                connection.rectTransform.anchoredPosition = (firstPosition + secondPosition) * 0.5f;
                float thickness = Mathf.Clamp(nodeSize * 0.2f, 3f, 5f);
                connection.rectTransform.sizeDelta = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? new Vector2(Mathf.Abs(delta.x), thickness)
                    : new Vector2(thickness, Mathf.Abs(delta.y));
                connection.color = room.IsVisited && neighbor.IsVisited
                    ? new Color(0.48f, 0.72f, 0.88f, 0.85f)
                    : new Color(0.38f, 0.46f, 0.58f, 0.42f);
                spawnedVisuals.Add(connection.gameObject);
            }
        }
    }

    private static Vector2 GetMapPosition(RoomCoordinate coordinate, float centerX, float centerY, float step)
    {
        return new Vector2((coordinate.X - centerX) * step, (coordinate.Y - centerY) * step);
    }

    private static Color GetNodeColor(RoomNode room, bool isCurrent)
    {
        if (isCurrent) return new Color(0.25f, 0.92f, 1f, 1f);
        if (!room.IsVisited) return new Color(0.4f, 0.46f, 0.56f, 0.42f);
        return RoomPresentationCatalog.Get(room.Type).MapColor;
    }

    private static void CalculateVisibleBounds(
        DungeonLayout layout,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;
        foreach (RoomNode room in layout.Rooms.Values)
        {
            if (RoomTypeUtility.IsHiddenRoom(room.Type) && !room.IsVisited) continue;
            minX = Mathf.Min(minX, room.Coordinate.X);
            maxX = Mathf.Max(maxX, room.Coordinate.X);
            minY = Mathf.Min(minY, room.Coordinate.Y);
            maxY = Mathf.Max(maxY, room.Coordinate.Y);
        }

        if (minX != int.MaxValue) return;
        minX = maxX = minY = maxY = 0;
    }

    private void ClearNodes()
    {
        foreach (GameObject visual in spawnedVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        spawnedVisuals.Clear();
    }

    private void OnDestroy()
    {
        if (session != null) session.RoomChanged -= OnRoomChanged;
    }
}
