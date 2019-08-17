using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform transformToFollow;

    void Start()
    {
        if (transformToFollow == null)
            transformToFollow = FindObjectOfType<PlayerController>().transform;
    }
    
    void Update()
    {
        transform.position = transformToFollow.position;
    }
}
