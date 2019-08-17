using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeItem : Item
{
    public GameObject lifeParticle;
    public float lifeRestore;

    public override void MyActionLoot(Transform t)
    {
        Instantiate(lifeParticle, t.position, t.rotation);
        t.GetComponent<PlayerLife>().TakeDamage(-lifeRestore);
        Destroy(this.gameObject);
    }
}
