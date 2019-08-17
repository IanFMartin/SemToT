using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, ILooteable
{
    private float timeLooting;
    private float timeToLoot = 1;

    public void Loot(Transform t)
    {
        timeLooting += Time.deltaTime;
        if(timeLooting >= timeToLoot)
        {
            Vector3 distance = t.position - transform.position;
            transform.position += distance * 4 * Time.deltaTime;
            if (distance.magnitude < 1)
                MyActionLoot(t);
        }
    }

    public virtual void MyActionLoot(Transform t)
    {

    }
}
