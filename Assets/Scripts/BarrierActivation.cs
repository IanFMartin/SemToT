using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierActivation : MonoBehaviour
{
    public List<GameObject> DoorList = new List<GameObject>();
    public GameObject smoke;

    public void ActivateDoors()
    {
        foreach (var door in DoorList)
        {
            door.SetActive(true);
            var pos = door.GetComponentsInChildren<Collider>();
            foreach (var collider in pos)
            {
                Instantiate(smoke, door.GetComponentInChildren<Collider>().transform.position - Vector3.up, Quaternion.identity);
            }
        }
    }
}
