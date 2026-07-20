public interface IGameSaveRepository
{
    bool HasSave { get; }
    void Save(GameSaveData data);
    bool TryLoad(out GameSaveData data);
    void Delete();
}
