using System;

[Serializable]
public struct RoomCoordinate : IEquatable<RoomCoordinate>
{
    public int X;
    public int Y;

    public RoomCoordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public RoomCoordinate Offset(RoomDirection direction)
    {
        RoomCoordinate offset = RoomDirectionUtility.ToOffset(direction);
        return new RoomCoordinate(X + offset.X, Y + offset.Y);
    }

    public int ManhattanDistance(RoomCoordinate other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }

    public bool Equals(RoomCoordinate other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is RoomCoordinate other && Equals(other);
    public override int GetHashCode() => unchecked((X * 397) ^ Y);
    public override string ToString() => $"({X}, {Y})";

    public static bool operator ==(RoomCoordinate left, RoomCoordinate right) => left.Equals(right);
    public static bool operator !=(RoomCoordinate left, RoomCoordinate right) => !left.Equals(right);
}
