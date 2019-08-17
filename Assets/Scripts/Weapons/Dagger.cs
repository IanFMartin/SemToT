using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : Weapon
{
    public GameObject slashEffect;
    public GameObject specialEffect;
    public GameObject specialBulletPrefab;

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        base.WeaponAttack(offset, rot, layer);
        slashEffect.GetComponent<MakeDamage>().myLayer = layer;
        slashEffect.GetComponent<MakeDamage>().dmg = damage;
        var slash = Instantiate(slashEffect, offset, rot);
    }

    public override void WeaponSpecialAttack(Vector3 offset, Quaternion rot, int layer)
    {
        base.WeaponSpecialAttack(offset, rot, layer);
        specialBulletPrefab.GetComponent<Bullet>().myLayer = layer;
        specialBulletPrefab.GetComponent<Bullet>().dmg = specialDamage;
        SpawnArrows(offset, rot, Vector3.zero);
        SpawnArrows(offset + transform.forward, rot, Vector3.zero);
        SpawnArrows(offset - transform.forward, rot, Vector3.zero);
    }

    private void SpawnArrows(Vector3 offset, Quaternion rot, Vector3 addedRot)
    {
        addedRot += rot.eulerAngles;
        Quaternion newRot = Quaternion.Euler(addedRot);
        var special = Instantiate(specialBulletPrefab, offset, newRot);
    }
}
