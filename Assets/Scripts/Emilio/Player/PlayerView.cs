using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    Animator _myAnim;

    void Start()
    {
        _myAnim = GetComponent<Animator>();
    }
    
    void Update()
    {
        
    }

    public void OnMove(bool isMoving)
    {

    }

    public void OnAttack(int slashNumbr)
    {

    }

    public void OnRangeAttack()
    {

    }

    public void OnSpecial()
    {

    }

    public void OnDash()
    {

    }

    public void OnJump()
    {

    }

    public void OnIdle()
    {

    }

    public void OnHit()
    {

    }

    public void OnDeath()
    {

    }
}
