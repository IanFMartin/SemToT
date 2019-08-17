using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHhealBullet : MonoBehaviour
{
    public GameObject hitEffect;
    internal DesertBoss myOwner;
    public float speed;
    public float dmg;
    public int myLayer;
    public bool invunerableBullet;

    private void Start()
    {
        myOwner = FindObjectOfType<DesertBoss>();
    }

    void Update()
    {
        transform.position += transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            other.GetComponent<IDamageable>().TakeDamage(dmg);
            Instantiate(hitEffect, transform.position, transform.rotation);
            if (myOwner != null)
                myOwner.Heal(200);
            if (!invunerableBullet)
                Destroy(this.gameObject, 0.1f);
        }
        else if (other.gameObject.layer == 13)
        {
            Destroy(this.gameObject);
        }
    }
}
