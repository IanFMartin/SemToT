using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerExplosion : MonoBehaviour, IDamageable
{
    public ParticleSystem flowerExplosion;
    public void TakeDamage(float damage, bool isCurseDmg)
    {
        var exp = Instantiate(flowerExplosion);
        exp.transform.position = transform.position;
        exp.Play();
        Destroy(gameObject);
    }

}
