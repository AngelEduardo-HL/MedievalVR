using UnityEngine;
using MedievalVR.Combat;

public class WaveLogic : MonoBehaviour
{
    public EnemySpawner spawner;
    public float spawnInterval = 3f;
    public float timeBetweenWaves = 5f;

    public int currentWave = 1;
    public int enemiesPerWave;
    public float enemiesHealth;

    public int enemiesSpawned;
    public int enemiesAlive;

    void Start()
    {
        SetWave();
    }

    void SetWave()
    {
        switch (currentWave)
        {
            case 1:
                enemiesPerWave = 5;
                enemiesHealth = 50f;
                break;
            case 2:
                enemiesPerWave = 10;
                enemiesHealth = 80f;
                break;
            case 3:
                enemiesPerWave = 15;
                enemiesHealth = 120f;
                break;
            case 4:
                enemiesPerWave = 20;
                enemiesHealth = 160f;
                break;
            case 5:
                enemiesPerWave = 25;
                enemiesHealth = 220f;
                break;
            default:
                Debug.Log("¡Victoria!");
                return;
        }

        enemiesSpawned = 0;
        enemiesAlive = enemiesPerWave;
        InvokeRepeating(nameof(SpawnNextEnemy), 1f, spawnInterval);
    }

    void SpawnNextEnemy()
    {
        if (enemiesSpawned >= enemiesPerWave)
        {
            CancelInvoke(nameof(SpawnNextEnemy));
            return;
        }

        int maxEnemyIndex = Mathf.Min(currentWave - 1, 4);
        int enemyType = Random.Range(0, maxEnemyIndex + 1);

        GameObject enemy = spawner.SpawnEnemy(enemyType);
        enemiesSpawned++;

        AssignWeaponForEnemy(enemy, enemyType);

        DamageableTarget target = enemy.GetComponentInChildren<DamageableTarget>();
        if (target != null)
        {
            target.SetHealth(enemiesHealth);
            target.Died += OnEnemyDied;
        }
    }

    void AssignWeaponForEnemy(GameObject enemy, int enemyType)
    {
        EnemyWeapon weaponHolder = enemy.GetComponent<EnemyWeapon>();
        if (weaponHolder != null) weaponHolder.EquipRandomWeapon();
    }

    void OnEnemyDied(DamageableTarget deadEnemy)
    {
        deadEnemy.Died -= OnEnemyDied;
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            currentWave++;
            Invoke(nameof(SetWave), timeBetweenWaves);
        }
    }

    [ContextMenu("Matar Todos (Prueba)")]
    void KillAllEnemies()
    {
        foreach (var enemy in FindObjectsByType<DamageableTarget>(FindObjectsSortMode.None))
        {
            enemy.TakeDamage(enemy.MaximumHealth);
        }
    }
}