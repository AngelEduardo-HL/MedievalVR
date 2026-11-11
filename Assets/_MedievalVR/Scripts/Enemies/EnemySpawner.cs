using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public Transform spawnPoint; 
    public int spawnInterval = 5; 
    public int maxEnemies = 5;
    int currentEnemies;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartSpawning(maxEnemies);
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public void SpawnEnemy()
    {
        if (currentEnemies >= maxEnemies) return;
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        currentEnemies++;

    }

    public void StartSpawning(int count)
    {
        InvokeRepeating("SpawnEnemy", 1f, spawnInterval);
    }


}
