using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class EnemyWeaponDrop 
{
    public  int GetWeapon(Dictionary<int, float> weaponChance)
    {

        float x = Random.Range(0, 1f);
        float sum = 0;
        List<int> keys = new List<int>(weaponChance.Keys);
        foreach (var item in weaponChance)
        {
            sum += item.Value;
        }
        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            weaponChance[key] = weaponChance[key] / sum;
        }
        float count = 0;
        foreach (var item in weaponChance)
        {
            count += item.Value;
            if (count >= x)
                return item.Key;
        }
        return -1;
    }
}
