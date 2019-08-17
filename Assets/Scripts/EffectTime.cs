using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTime : MonoBehaviour
{
    public float time;

    void Start()
    {
        Destroy(this.gameObject, time);
    }
    
}
