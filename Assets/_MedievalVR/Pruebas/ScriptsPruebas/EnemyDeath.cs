using System.Collections;
using MedievalVR.Combat;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public GameObject scorePoints;
    public GameObject healthText;
    public GameObject healthDrop;
    public int scoreValue = 10;

    [SerializeField, Range(0, 100)] int healthDropChance = 30;

    DamageableTarget health;

    void OnEnable()
    {
        health = GetComponentInChildren<DamageableTarget>();
        if (health != null) health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health != null) health.Died -= OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        if (GameManager.instance != null) GameManager.instance.AddPoints(scoreValue);
        if (TryGetComponent(out Chase chase)) chase.enabled = false;
        if (healthText != null) healthText.SetActive(false);

        var animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Die");

        var weapon = GetComponentInChildren<EnemyWeapon>();
        if (weapon != null)
        {
            weapon.DropPlayerWeapon();
            weapon.UnequipWeapon();
        }

        StartCoroutine(ShowPoints());
        Destroy(gameObject, 5f);
    }

    IEnumerator ShowPoints()
    {
        if (scorePoints != null) scorePoints.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (scorePoints != null) scorePoints.SetActive(false);
        DropHealth();
    }

    public void DropHealth()
    {
        if (healthDrop != null && Random.Range(0, 100) < healthDropChance)
            Instantiate(healthDrop, transform.position, Quaternion.identity);
    }
}
