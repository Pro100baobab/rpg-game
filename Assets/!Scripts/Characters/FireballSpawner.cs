using DigitalRuby.PyroParticles;
using UnityEngine;

public class FireballSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistance = 0.8f;
    [SerializeField] private float spawnAddedHeight = 0.8f;
    [SerializeField] private float playerAddedHeight = 0.5f;

    [Header("Damage")]
    [SerializeField] private float magicDamage = 15f;

    private void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void SpawnObject()
    {
        if (objectToSpawn == null)
        {
            Debug.LogWarning("Префаб не назначен!");
            return;
        }

        Vector3 editedPosition = new Vector3(transform.position.x, transform.position.y + spawnAddedHeight, transform.position.z);
        Vector3 editedPlayerPosition = new Vector3(player.position.x, player.position.y + playerAddedHeight, player.position.z);
        
        Vector3 spawnPosition = editedPosition + transform.forward * spawnDistance;
        Vector3 direction = (editedPlayerPosition - editedPosition).normalized;

        GameObject fireball = Instantiate(objectToSpawn, spawnPosition, Quaternion.LookRotation(direction));
        fireball.GetComponent<FireProjectileScript>().ProjectileDirection = Vector3.forward;
    }
}