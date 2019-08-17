using UnityEngine;

public class BossSword : Weapon
{
    public GameObject slashEffect;

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        slashEffect.GetComponent<MakeDamage>().myLayer = layer;
        slashEffect.GetComponent<MakeDamage>().dmg = damage;
        var slash = Instantiate(slashEffect, offset, rot);
    }
}
