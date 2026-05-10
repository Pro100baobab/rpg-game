using System.Collections.Generic;
using UnityEngine;


public class GameLoadInteractor
{
    private readonly IGameRepository _repository;
    private readonly GameModel _model;
    private readonly SpawnManager _spawnManager;

    public GameLoadInteractor(IGameRepository repository, GameModel model, SpawnManager spawnManager)
    {
        _repository = repository;
        _model = model;
        _spawnManager = spawnManager;
    }


    public bool LoadGame()
    {
        if (!_repository.HasSave()) return false;

        var data = _repository.Load();
        if (data == null) return false;

        // Восстанавливаем игровой режим
        _model.IsPeacefulMode = data.isPeacefulMode;

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

        _spawnManager.ClearAllEnemies();

        foreach (var enemyData in data.enemies)
        {
            _spawnManager.RestoreEnemy(enemyData);
        }

        return true;
    }
}