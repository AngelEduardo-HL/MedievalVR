using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public GameObject[] weapons;
    public GameObject[] playerWeapons;
    public Transform spawnWeaponPlayer;

    GameObject currentWeapon;
    GameObject weaponPlayerDropped;

    void Start()
    {
        if (currentWeapon == null) EquipRandomWeapon();
    }

    public void EquipRandomWeapon()
    {
        if (weapons == null || weapons.Length == 0) return;
        foreach (var weapon in weapons)
        {
            if (weapon != null) weapon.SetActive(false);
        }

        int chosen = Random.Range(0, weapons.Length);
        currentWeapon = weapons[chosen];
        weaponPlayerDropped = null;
        if (currentWeapon == null) return;

        currentWeapon.SetActive(true);
        if (playerWeapons != null && chosen < playerWeapons.Length)
            weaponPlayerDropped = playerWeapons[chosen];
    }

    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;
        currentWeapon.SetActive(false);
        currentWeapon = null;
    }

    public void DropPlayerWeapon()
    {
        if (weaponPlayerDropped == null || currentWeapon == null) return;
        Transform dropPoint = spawnWeaponPlayer != null ? spawnWeaponPlayer : currentWeapon.transform;
        Instantiate(weaponPlayerDropped, dropPoint.position, dropPoint.rotation);
        weaponPlayerDropped = null;
    }
}
