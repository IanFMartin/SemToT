using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject fire;
    internal GameObject zone;
    public Vector3 dir;
    public float speed;
    public float dmg;
    public float fireChance;
    public int myLayer;
    public bool invunerableBullet;

    private void Start()
    {
        fireChance *= 0.01f;
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            float fireValue = Random.value;
            if (fireValue <= fireChance)
            {
                var fireobj = Instantiate(fire, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
                fireobj.transform.Rotate(90, 0, 180);
                fireobj.GetComponent<MakeAreaDamage>().myLayer = 10;
            }
            if (zone != null)
                Destroy(zone);
            other.GetComponent<IDamageable>().TakeDamage(dmg, false);
            Instantiate(hitEffect, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
            if (!invunerableBullet)
                Destroy(this.gameObject);
        }
        else if (other.gameObject.layer == 14)
        {
            float fireValue = Random.value;
            if (fireValue <= fireChance)
            {
                var fireobj = Instantiate(fire, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
                fireobj.transform.Rotate(90, 0, 180);
                fireobj.GetComponent<MakeAreaDamage>().myLayer = 10;
            }
            if (zone != null)
                Destroy(zone);
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<IDamageable>() != null && other.gameObject.layer != myLayer)
        {
            float fireValue = Random.value;
            if (fireValue <= fireChance)
            {
                var fireobj = Instantiate(fire, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
                fireobj.transform.Rotate(90, 0, 180);
                fireobj.GetComponent<MakeAreaDamage>().myLayer = 10;
            }
            if (zone != null)
                Destroy(zone);
            other.gameObject.GetComponent<IDamageable>().TakeDamage(dmg, false);
            Instantiate(hitEffect, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
            if (!invunerableBullet)
            {
                Destroy(this.gameObject);
                Shake.instance.shake = 0.1f;
                Shake.instance.shakeAmount = 0.1f;
            }
        }
        else if (other.gameObject.layer != myLayer)
        {
            float fireValue = Random.value;
            if (fireValue <= fireChance)
            {
                var fireobj = Instantiate(fire, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
                fireobj.transform.Rotate(90, 0, 180);
                fireobj.GetComponent<MakeAreaDamage>().myLayer = 10;
            }
            if (zone != null)
                Destroy(zone);
            Instantiate(hitEffect, transform.position, transform.rotation);
            if (!invunerableBullet)
            {
                Destroy(this.gameObject);
                Shake.instance.shake = 0.1f;
                Shake.instance.shakeAmount = 0.1f;
            }
        }
    }

}
