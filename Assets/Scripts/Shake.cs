using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    public static Shake instance = null;

    public float shake;
    public float shakeAmount;
    public float shakeModifier;
    public float decreaseFactor;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Update()
    {
        if (shake > 0)
        {
            transform.position = Random.insideUnitSphere * shakeAmount * shakeModifier + transform.position;
            shake -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shake = 0f;
        }
    }
}
