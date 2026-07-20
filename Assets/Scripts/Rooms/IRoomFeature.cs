using System;

public interface IRoomFeature
{
    void Initialize(IRoomRuntimeContext runtimeContext, RoomNode roomNode);
    void OnRoomEntered();
}

public interface IRoomLockSource
{
    bool LocksRoom { get; }
    event Action Cleared;
}
