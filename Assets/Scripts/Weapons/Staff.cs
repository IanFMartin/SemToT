using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staff : Weapon
{
    public GameObject bulletPrefab;
    public GameObject specialEffect;

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
        specialEffect.GetComponent<MakeAreaDamage>().myLayer = layer;
        specialEffect.GetComponent<MakeAreaDamage>().dmg = specialDamage;
        var special = Instantiate(specialEffect, offset, Quaternion.Euler(90,0,0));
    }
}
