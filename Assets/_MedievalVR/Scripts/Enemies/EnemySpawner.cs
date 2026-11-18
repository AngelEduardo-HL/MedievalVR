using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform spawnPoint;


    public GameObject SpawnEnemy(int prefabIndex)
    {
        return Instantiate(enemyPrefabs[prefabIndex], spawnPoint.position, Quaternion.identity);
    }


    public GameObject SpawnRandomEnemy(int maxTypeIndex)
    {
        int randomIndex = Random.Range(0, maxTypeIndex + 1);
        return Instantiate(enemyPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
    }
}