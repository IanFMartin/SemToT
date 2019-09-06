using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBase : MonoBehaviour
{
    //se puede curar?
    public float maxHealth;
    protected float _currentHealth;
    public float speed;
    public float viewRange;
    protected Rigidbody _rb;
    protected GameObject _target;




    void Start()
    {
        BaseStart();
    }

    protected void BaseStart()
    {
        _rb = GetComponent<Rigidbody>();
        _currentHealth = maxHealth;
        _target = FindObjectOfType<PlayerModel>().gameObject;
    }
    
    void Update()
    {
        
    }

    public virtual void TakeDamage(float dmg)
    {

        _currentHealth -= dmg;
        if(_currentHealth >= 0)
        {
            Death();
        }
    }

    protected virtual void Death()
    {
        //pool?
    }
}
