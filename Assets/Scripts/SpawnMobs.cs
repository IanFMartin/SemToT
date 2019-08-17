using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMobs : MonoBehaviour
{
    public GameObject smoke;

    public GameObject summoner;
    public GameObject wizard;

    private void OnCollisionEnter(Collision collision)
    {
        Spawn(summoner);
        Spawn(wizard);
    }
    
    private void Spawn(GameObject mob)
    {
        Vector3 pos = new Vector3(transform.position.x + Random.Range(-1, 1), 1.5f, transform.position.z + Random.Range(-1, 1));
        Instantiate(smoke, pos, Quaternion.identity);
        Instantiate(mob, pos, Quaternion.identity);
    }
}
