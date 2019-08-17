using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpItem : Item
{
    public delegate void ParticleHandler(ExpItem thisItem);
    public event ParticleHandler OnParticleDeath = delegate { };

    internal ExpController expController;

    public float exp;

    public void Spawn(Vector3 position)
    {
        Vector3 rnd = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 0.5f), Random.Range(-1f, 1f));
        transform.position = position + rnd;
    }

    public override void MyActionLoot(Transform t)
    {
        expController.GetExp(exp);
        t.GetComponent<PlayerLife>().TakeCurseDamage(-1);
        OnParticleDeath(this);
    }
}
