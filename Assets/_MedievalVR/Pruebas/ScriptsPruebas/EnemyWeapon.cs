using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public GameObject[] weapons;
    GameObject currentWeapon;

    public void EquipRandomWeapon()
    {
        if (weapons.Length == 0) return;

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(false);

        int chosen = Random.Range(0, weapons.Length);
        weapons[chosen].SetActive(true);
        currentWeapon = weapons[chosen];
    }

    public void UnequipWeapon()
    {
        currentWeapon.transform.parent = null;
    }

}