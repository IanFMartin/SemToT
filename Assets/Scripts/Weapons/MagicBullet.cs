using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicBullet : Bullet
{
    public int radius;
    public bool oscilant;
    private float dir;

    private void Start()
    {
        dir = 1;
        float rnd = Random.value;
        if (rnd < 0.5f)
            dir = -1;
    }

    private void Update()
    {
        transform.position += transform.forward * speed;
        if(oscilant)
        {
            Vector3 pos = transform.position;
            pos.x += Mathf.Sin(Time.time * 10) * Time.deltaTime * 5 * dir;
            pos.z += Mathf.Sin(Time.time * 10) * Time.deltaTime * 5 * dir;
            transform.position = pos;
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            var enemiesNear = AreaOfEffect();
            foreach (var enemy in enemiesNear)
            {
                enemy.TakeDamage(dmg, false);
            }
            var magicParticle = Instantiate(hitEffect, transform.position, transform.rotation);
            StartCoroutine(ToDestroy(magicParticle.gameObject));
            if (!invunerableBullet)
                Destroy(this.gameObject);
        }
        else if (other.gameObject.layer == 13)
        {
            Destroy(this.gameObject);
        }
    }

    private List<IDamageable> AreaOfEffect()
    {   
        var area = Physics.OverlapSphere(this.gameObject.transform.position, radius);
        List<IDamageable> listToReturn = new List<IDamageable>();
        foreach (var collider in area)
        {
            if(collider.GetComponent<IDamageable>() != null && collider.gameObject.layer != myLayer)
            {
                listToReturn.Add(collider.GetComponent<IDamageable>());
            }
        }

        return listToReturn;
    }

    IEnumerator ToDestroy(GameObject toDestroy)
    {
        yield return new WaitForSeconds(1);
        Destroy(toDestroy);
    }
}
