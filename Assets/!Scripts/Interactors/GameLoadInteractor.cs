using System.Collections.Generic;
using UnityEngine;


public class GameLoadInteractor
{
    private readonly IGameRepository _repository;
    private readonly GameModel _model;

    public GameLoadInteractor(IGameRepository repository, GameModel model)
    {
        _repository = repository;
        _model = model;
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