using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RWS : MonoBehaviour
{
    public Dictionary<Action, float> actions = new Dictionary<Action, float>();

    void Start()
    {
        actions.Add(Heal, 0);
        actions.Add(NormalAttack, 10);
        actions.Add(Summon, 5);
    }
    
    void Update()
    {
        
    }

    Action GetAction(Dictionary<Action, float> actions)
    {
        float x = UnityEngine.Random.Range(0, 1f);
        return null;
    }

    void Heal()
    {

    }

    void NormalAttack()
    {

    }

    void Summon()
    {

    }
}
