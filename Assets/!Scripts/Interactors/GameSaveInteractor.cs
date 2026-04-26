using System.Collections.Generic;


public class GameSaveInteractor
{
    private readonly IGameRepository _repository;
    private readonly GameModel _model;

    public GameSaveInteractor(IGameRepository repository, GameModel model)
    {
        _repository = repository;
        _model = model;
    }

    public void SaveGame()
    {
        var data = new GameData();

        if (_model.Player != null)
        {
            var playerHealth = _model.Player.GetComponent<IHealth>();
            var playerController = _model.Player.GetComponent<PlayerController>();

            data.player = new PlayerData
            {
                posX = _model.Player.transform.position.x,
                posY = _model.Player.transform.position.y,
                posZ = _model.Player.transform.position.z,
                health = playerHealth.CurrentHealth,
                remainingCooldown = playerController.GetRemainingCooldown()
            };
        }

        data.enemies = new List<EnemyData>();
        foreach (var enemy in _model.Enemies)
        {
            if (enemy == null) continue;

            var health = enemy.GetComponent<IHealth>();
            string type = enemy.GetComponent<MeleeEnemy>() != null ? "Melee" : "EvilWatcher";

            data.enemies.Add(new EnemyData
            {
                enemyType = type,
                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y,
                posZ = enemy.transform.position.z,
                health = health.CurrentHealth
            });
        }

        _repository.Save(data);
    }
}
