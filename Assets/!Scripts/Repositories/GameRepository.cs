public class GameRepository : IGameRepository
{
    private const string SaveKey = "game_save";
    private readonly ISaveService _saveService;

    public GameRepository(ISaveService saveService) => _saveService = saveService;

    public void Save(GameData data) => _saveService.Save(SaveKey, data);
    public GameData Load() => _saveService.Load<GameData>(SaveKey, null);
    public bool HasSave() => _saveService.HasKey(SaveKey);
}