using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyHero : MonoBehaviour, IDamageable
{
    private PlayerController _target;
    public ParticleSystem ExplosionCurseParticle;
    public List<Transform> InvisibleWalls;
    public Canvas uiCanvas;

    public void TakeDamage(float damage, bool isCurseDmg)
    {
        Instantiate(ExplosionCurseParticle, transform.position, transform.rotation);
        foreach (var Wall in InvisibleWalls)
        {
            Destroy(Wall.gameObject);
        }
        uiCanvas.GetComponent<TowerEntryUI>().Cursing();
        Destroy(gameObject);

    }
}
