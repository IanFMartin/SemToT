using UnityEngine;

public class Sword : Weapon
{
    public GameObject slashEffect;
    public GameObject specialEffect;    

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
        slashEffect.GetComponent<MakeDamage>().myLayer = layer;
        slashEffect.GetComponent<MakeDamage>().dmg = damage;
        var slash = Instantiate(slashEffect, offset, rot);

        specialEffect.GetComponent<Bullet>().myLayer = layer;
        specialEffect.GetComponent<Bullet>().dmg = specialDamage;
        var special = Instantiate(specialEffect, offset, rot);
    }
}
