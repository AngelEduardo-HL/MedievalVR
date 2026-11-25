using UnityEngine;
using MedievalVR.Combat;

public class WaveLogic : MonoBehaviour
{
    const int FinalWave = 5;

    public EnemySpawner spawner;
    public float spawnInterval = 3f;
    public float timeBetweenWaves = 5f;
    [SerializeField] DamageableTarget playerHealth;
    [SerializeField, Min(0f), Tooltip("Vida entre rondas.")] float roundHealing = 20f;

    public int currentWave = 1;
    public int enemiesPerWave;
    public float enemiesHealth;

    public int enemiesSpawned;
    public int enemiesAlive;

    public Victory victory;

    void Start()
    {
        SetWave();
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += StopWaves;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= StopWaves;
    }

    void StopWaves(DamageableTarget player) => CancelInvoke();

    void SetWave()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (currentWave == FinalWave + 1)
        {
            if (victory != null) victory.PlayFireworks();
            return;
        }

        switch (currentWave)
        {
            case 1:
                enemiesPerWave = 3;
                enemiesHealth = 20f;
                break;
            case 2:
                enemiesPerWave = 5;
                enemiesHealth = 40f;
                break;
            case 3:
                enemiesPerWave = 7;
                enemiesHealth = 60f;
                break;
            case 4:
                enemiesPerWave = 10;
                enemiesHealth = 80f;
                break;
            case 5:
                enemiesPerWave = 15;
                enemiesHealth = 100f;
                break;
            default:
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

        AssignWeaponForEnemy(enemy);

        DamageableTarget target = enemy.GetComponentInChildren<DamageableTarget>();
        if (target != null)
        {
            target.SetHealth(enemiesHealth);
            target.Died += OnEnemyDied;
        }
    }

    void AssignWeaponForEnemy(GameObject enemy)
    {
        EnemyWeapon weaponHolder = enemy.GetComponent<EnemyWeapon>();
        if (weaponHolder != null) weaponHolder.EquipRandomWeapon();
    }

    void OnEnemyDied(DamageableTarget deadEnemy)
    {
        deadEnemy.Died -= OnEnemyDied;
        enemiesAlive--;
        if (enemiesAlive != 0) return;

        CancelInvoke(nameof(SpawnNextEnemy));
        if (playerHealth != null && playerHealth.IsDead) return;
        if (currentWave < FinalWave && playerHealth != null) playerHealth.TryHeal(roundHealing);

        currentWave++;
        Invoke(nameof(SetWave), timeBetweenWaves);
    }

    [ContextMenu("Matar Todos (Prueba)")]
    void KillAllEnemies()
    {
        foreach (var enemy in FindObjectsByType<DamageableTarget>(FindObjectsSortMode.None))
        {
            if (enemy != playerHealth && enemy.GetComponent<PlayerCombatRig>() == null)
                enemy.TakeDamage(enemy.MaximumHealth);
        }
    }
}
