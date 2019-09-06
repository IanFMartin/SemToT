using UnityEngine;

public class SkeletonBullet : MonoBehaviour
{
    public GameObject hitEffect;
    public float speed;
    public float dmg;
    public int myLayer;
    public float timeAlive;
    public bool invunerableBullet;
    public bool ignoreWalls;

    void Update()
    {
        transform.position += -transform.right * speed;
        if (timeAlive <= 0) Destroy(gameObject);
        else
            timeAlive -= Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            other.GetComponent<IDamageable>().TakeDamage(dmg, false);
            var HE = Instantiate(hitEffect, transform.position, transform.rotation);
            if (!invunerableBullet)
                Destroy(this.gameObject);
        }
        if (!ignoreWalls)
        {
            if (other.gameObject.layer == 13)
            {
                Instantiate(hitEffect, transform.position + Vector3.up * 2, transform.rotation);
                Destroy(this.gameObject);
            }

        }
    }
}
