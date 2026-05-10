using UnityEngine;
using UnityEngine.UI;

// Этот класс предназначен для отладки и позволяет добавлять новые точки спауна и вызывать спаун врагов через UI
// (поэтому для упрощения не использует MVC)
public class SpawnerUI : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private Button addPointButton;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Transform playerTransform; // для определения позиции новой точки
    [SerializeField] private float spawnDistance = 5f;

    private void Start()
    {
        addPointButton?.onClick.AddListener(OnAddPointClicked);
        spawnButton?.onClick.AddListener(OnSpawnClicked);
    }

    private void OnAddPointClicked()
    {
        if (playerTransform == null)
            playerTransform = Camera.main?.transform;

        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position + playerTransform.forward * spawnDistance;
            spawnManager.AddSpawnPoint(pos);
        }
    }

    private void OnSpawnClicked()
    {
        spawnManager.SpawnEnemies();
    }
}