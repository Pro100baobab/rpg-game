using System.Collections.Generic;
using UnityEngine;

public class GameInteractor
{
    private readonly IGameRepository _repository;
    private readonly GameModel _model;

    public GameInteractor(IGameRepository repository, GameModel model)
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

    public bool LoadGame()
    {
        if (!_repository.HasSave()) return false;

        var data = _repository.Load();
        if (data == null) return false;

        // Восстанавливаем игрока
        if (_model.Player != null && data.player != null)
        {
            _model.Player.SetActive(false);
            _model.Player.transform.position = new Vector3(data.player.posX, data.player.posY, data.player.posZ);
            _model.Player.SetActive(true);

            var playerHealth = _model.Player.GetComponent<IHealth>();
            playerHealth.SetHealth(data.player.health);

            var playerController = _model.Player.GetComponent<PlayerController>();
            playerController.SetRemainingCooldown(data.player.remainingCooldown);

            float elapsed = playerController.CooldownTime - data.player.remainingCooldown;
            EventSystem.Instance.AbilityCooldown(playerController.CooldownTime, elapsed); // обновляем UI
        }

        // Восстанавливаем мобов
        for (int i = 0; i < _model.Enemies.Count && i < data.enemies.Count; i++)
        {
            var enemy = _model.Enemies[i];
            var enemyData = data.enemies[i];

            enemy.transform.position = new Vector3(enemyData.posX, enemyData.posY, enemyData.posZ);
            var health = enemy.GetComponent<IHealth>();
            health.SetHealth(enemyData.health);
        }

        return true;
    }
}