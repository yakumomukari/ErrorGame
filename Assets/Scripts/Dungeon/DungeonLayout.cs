using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class DungeonLayout
{
    private readonly Dictionary<RoomCoordinate, RoomNode> rooms = new Dictionary<RoomCoordinate, RoomNode>();

    public int Seed { get; }
    public RoomCoordinate StartCoordinate { get; internal set; }
    public RoomCoordinate? SecretCoordinate { get; internal set; }
    public RoomCoordinate? SuperSecretCoordinate { get; internal set; }
    public IReadOnlyDictionary<RoomCoordinate, RoomNode> Rooms => rooms;
    public IEnumerable<RoomNode> VisibleRooms => rooms.Values.Where(room => !RoomTypeUtility.IsHiddenRoom(room.Type));

    public DungeonLayout(int seed)
    {
        Seed = seed;
    }

    internal void AddRoom(RoomNode room)
    {
        if (rooms.ContainsKey(room.Coordinate))
        {
            throw new InvalidOperationException($"A room already exists at {room.Coordinate}.");
        }
        rooms.Add(room.Coordinate, room);
    }

    internal void Connect(RoomCoordinate coordinate, RoomDirection direction)
    {
        RoomCoordinate neighborCoordinate = coordinate.Offset(direction);
        if (!rooms.TryGetValue(coordinate, out RoomNode room) ||
            !rooms.TryGetValue(neighborCoordinate, out RoomNode neighbor))
        {
            throw new InvalidOperationException("Both rooms must exist before they can be connected.");
        }

        room.AddConnection(direction);
        neighbor.AddConnection(RoomDirectionUtility.Opposite(direction));
    }

    public bool TryGetRoom(RoomCoordinate coordinate, out RoomNode room)
    {
        return rooms.TryGetValue(coordinate, out room);
    }

    public bool TryGetConnectedRoom(RoomNode room, RoomDirection direction, out RoomNode neighbor)
    {
        neighbor = null;
        return room != null && room.HasConnection(direction) &&
               rooms.TryGetValue(room.Coordinate.Offset(direction), out neighbor);
    }

    public bool IsSecretPassageCandidate(RoomNode room, RoomDirection direction)
    {
        if (room == null || !rooms.TryGetValue(room.Coordinate, out RoomNode storedRoom) || storedRoom != room)
        {
            return false;
        }

        if (!rooms.TryGetValue(room.Coordinate.Offset(direction), out RoomNode neighbor)) return false;
        bool exactlyOneHiddenRoom = RoomTypeUtility.IsHiddenRoom(room.Type) !=
                                    RoomTypeUtility.IsHiddenRoom(neighbor.Type);
        return exactlyOneHiddenRoom && !room.HasConnection(direction);
    }

    public bool TryOpenSecretPassage(RoomNode room, RoomDirection direction)
    {
        if (!IsSecretPassageCandidate(room, direction)) return false;
        Connect(room.Coordinate, direction);
        return true;
    }

    public string GetDeterministicSignature()
    {
        StringBuilder signature = new StringBuilder();
        foreach (RoomNode room in rooms.Values.OrderBy(node => node.Coordinate.X).ThenBy(node => node.Coordinate.Y))
        {
            signature.Append(room.Coordinate.X).Append(',').Append(room.Coordinate.Y)
                .Append(':').Append((int)room.Type).Append(':');
            foreach (RoomDirection direction in room.Connections.OrderBy(direction => (int)direction))
            {
                signature.Append((int)direction);
            }
            signature.Append('|');
        }
        return signature.ToString();
    }
}
