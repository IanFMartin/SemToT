using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHhealStaff : Weapon
{
    public GameObject bulletPrefab;

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        bulletPrefab.GetComponent<HitHhealBullet>().myLayer = layer;
        bulletPrefab.GetComponent<HitHhealBullet>().dmg = damage;
        var bullet = Instantiate(bulletPrefab, offset, rot);
    }
}
