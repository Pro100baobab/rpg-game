public interface IGameRepository
{
    void Save(GameData data);
    GameData Load();
    bool HasSave();
}