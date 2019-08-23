using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ElectricSpell : Bullet
{
    public int radius;
    public int TotalJumps;
    private int JumpsLeft;

    private void Update()
    {
        
    }
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            other.GetComponent<IDamageable>().TakeDamage(dmg, false);
            NextJump(other.transform);
            var magicParticle = Instantiate(hitEffect, transform.position, transform.rotation);
            if (!invunerableBullet)
                Destroy(this.gameObject);
        }
        else if (other.gameObject.layer == 13)
        {
            Destroy(this.gameObject);
        }
    }

    public void NextJump(Transform Start)
    {
        if (JumpsLeft <= 0) return;

        var area = Physics.OverlapSphere(Start.position, radius);
        if (area.Any(x => x.GetComponent<IDamageable>() != null))
        {
           var next = area.Where(x => x.GetComponent<IDamageable>() != null).Skip(Random.Range(0, area.Count() - 2)).First();
            next.GetComponent<IDamageable>().TakeDamage(dmg, false);
            JumpsLeft--;
            NextJump(next.transform);
        }
    }
}
