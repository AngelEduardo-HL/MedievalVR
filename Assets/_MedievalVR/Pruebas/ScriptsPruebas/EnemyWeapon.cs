using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public GameObject[] weapons;
    public GameObject[] playerWeapons;
    public Transform spawnWeaponPlayer;

    GameObject currentWeapon;
    GameObject weaponPlayerDropped;



    public void EquipRandomWeapon()
    {
        if (weapons.Length == 0) return;

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(false);

        int chosen = Random.Range(0, weapons.Length);
        weapons[chosen].SetActive(true);
        currentWeapon = weapons[chosen];
        weaponPlayerDropped = playerWeapons[chosen];
    }

    public void UnequipWeapon()
    {
        currentWeapon.transform.parent = null;
    }

    public void DropPlayerWeapon()
    {
        Instantiate(weaponPlayerDropped,spawnWeaponPlayer);
    }

}