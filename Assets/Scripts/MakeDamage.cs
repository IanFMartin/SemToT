using UnityEngine;

public class MakeDamage : MonoBehaviour
{
    public GameObject hitParticle;
    public float dmg;
    public int myLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            Instantiate(hitParticle, transform.position, Quaternion.identity);
            other.GetComponent<IDamageable>().TakeDamage(dmg, false);
        }
    }
}
