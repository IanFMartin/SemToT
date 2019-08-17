using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    public GameObject bulletPrefab;
    public GameObject specialBulletPrefab;

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        base.WeaponAttack(offset, rot, layer);
        bulletPrefab.GetComponent<Bullet>().myLayer = layer;
        bulletPrefab.GetComponent<Bullet>().dmg = damage;
        var bullet = Instantiate(bulletPrefab, offset, rot);
    }

    public override void WeaponSpecialAttack(Vector3 offset, Quaternion rot, int layer)
    {
        base.WeaponSpecialAttack(offset, rot, layer);
        specialBulletPrefab.GetComponent<Bullet>().myLayer = layer;
        specialBulletPrefab.GetComponent<Bullet>().dmg = specialDamage;
        SpawnArrows(offset, rot, Vector3.zero);
        SpawnArrows(offset, rot, new Vector3(0, 25, 0));
        SpawnArrows(offset, rot, new Vector3(0, -25, 0));
    }

    private void SpawnArrows(Vector3 offset, Quaternion rot, Vector3 addedRot)
    {
        addedRot += rot.eulerAngles;
        Quaternion newRot = Quaternion.Euler(addedRot);
        var special = Instantiate(specialBulletPrefab, offset, newRot);         
    }
}

