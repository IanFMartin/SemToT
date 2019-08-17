using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepDeathController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 12) //Layer 12 Hero
        {
            other.GetComponent<PlayerLife>().Dead();
            //playerSpawner.Respawn();
        }
    }    
}
