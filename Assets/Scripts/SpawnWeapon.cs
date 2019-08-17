using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWeapon
{
    public void Spawn(GameObject gO,Vector3 where)
    {
        GameObject.Instantiate(gO, where + Vector3.up/2, Quaternion.identity);
    }
}
