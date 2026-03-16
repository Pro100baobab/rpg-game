using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerSpawnPosition;
    [SerializeField] private Quaternion playerSpawnRotation = Quaternion.identity;
    [SerializeField] private Quaternion cameraRotation = new Quaternion(10, 0, 0, 0);
    [SerializeField] private Quaternion cameraHolderRotation = Quaternion.identity;

    [Header("Ememies Settings")]
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();
    [SerializeField] private List<GameObject> spawnPoints = new List<GameObject>();

    public List<GameObject> Enemies => enemies;

    private void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        playerSpawnPosition.gameObject.GetComponent<MeshRenderer>().enabled = false;

        GameObject[] enemiesSplit = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemiesSplit) 
            enemies.Add(enemy);

        GameObject[] pointsSplit = GameObject.FindGameObjectsWithTag("SpawnPoints");
        foreach (GameObject point in pointsSplit)
        {
            spawnPoints.Add(point);
            point.GetComponent<MeshRenderer>().enabled = false;
        }

        Restart();
        Invoke("SetUpAfterDelay", 1);

        Debug.Log($"Найдено врагов: {enemies.Count}");
        Debug.Log($"Найдено точек: {spawnPoints.Count}");
    }
    private void OnDisable() => EventSystem.Instance.OnRestart -= Restart;

    private void SetUpAfterDelay()
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.OnRestart += Restart;
            Debug.Log("SpawnController подписался на EventSystem");
        }
    }

    private void Restart()
    {
        Camera.main.transform.parent.rotation = cameraHolderRotation;
        Camera.main.transform.rotation = cameraRotation;

        player.SetActive(false);
        player.transform.position = playerSpawnPosition.position;
        player.transform.rotation = playerSpawnRotation;
        player.SetActive(true);

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].SetActive(false);
            enemies[i].transform.position = spawnPoints[i].transform.position;
            enemies[i].SetActive(true);
        }
    }
}
