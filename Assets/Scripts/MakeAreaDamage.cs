using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeAreaDamage : MonoBehaviour
{
    public GameObject hitParticle;
    public float dmg;
    public int myLayer;
    public float timeToDamage;
    private float timer;

    private void OnTriggerStay(Collider other)
    {
        timer += Time.deltaTime;
        if (timer >= timeToDamage)
        {
            if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
            {
                Instantiate(hitParticle, other.transform.position + Vector3.up / 3, Quaternion.identity);
                other.GetComponent<IDamageable>().TakeDamage(dmg, false);
                timer = 0;
            }
        }
    }
}
