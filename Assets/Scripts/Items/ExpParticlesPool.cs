using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpParticlesPool : MonoBehaviour
{
    public ExpItem particle;

    public Pool<ExpItem> pool;

    void Start()
    {
        pool = new Pool<ExpItem>(80, GetParticle, OnReleaseParticle, OnGetParticle);
    }

    public virtual ExpItem GetParticle()
    {
        var particle = Instantiate(this.particle);
        particle.gameObject.transform.parent = this.transform;
        particle.gameObject.SetActive(false);
        return particle;
    }

    private void OnReleaseParticle(ExpItem particle)
    {
        particle.gameObject.SetActive(false);
        particle.OnParticleDeath -= pool.ReleaseObject;
    }

    private void OnGetParticle(ExpItem particle)
    {
        particle.gameObject.SetActive(true);
        particle.OnParticleDeath += pool.ReleaseObject;
    }
}
