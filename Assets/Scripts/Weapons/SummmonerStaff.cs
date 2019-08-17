using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummmonerStaff : Weapon
{
    public GameObject summonPrefab;
    public GameObject summonSmoke;
    public int count;

    private void Start()
    {
        rateOfFire = 0;
    }

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        base.WeaponAttack(offset, rot, layer);
        count++;
        if (count <= 3)
        {
            var summon = Instantiate(summonPrefab, offset, transform.rotation);
            var smoke = Instantiate(summonSmoke, offset, transform.rotation);
        }
    }
}
