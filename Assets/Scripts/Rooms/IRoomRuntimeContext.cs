public interface IRoomRuntimeContext
{
    Player Player { get; }
    int ActiveSeed { get; }

    bool IsSecretPassageCandidate(RoomNode room, RoomDirection direction);
    bool TryTransition(RoomController sourceRoom, RoomDirection direction);
    bool TryOpenSecretPassage(RoomController sourceRoom, RoomDirection direction);
    bool TryAdvanceToNextFloor(RoomNode sourceRoom);
}
