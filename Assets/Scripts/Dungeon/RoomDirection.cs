using System;
using System.Collections.Generic;

public enum RoomDirection
{
    North,
    South,
    West,
    East
}

public static class RoomDirectionUtility
{
    private static readonly RoomDirection[] directions =
    {
        RoomDirection.North,
        RoomDirection.South,
        RoomDirection.West,
        RoomDirection.East
    };

    public static IReadOnlyList<RoomDirection> All => directions;

    public static RoomCoordinate ToOffset(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.North: return new RoomCoordinate(0, 1);
            case RoomDirection.South: return new RoomCoordinate(0, -1);
            case RoomDirection.West: return new RoomCoordinate(-1, 0);
            case RoomDirection.East: return new RoomCoordinate(1, 0);
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public static RoomDirection Opposite(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.West: return RoomDirection.East;
            case RoomDirection.East: return RoomDirection.West;
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}
