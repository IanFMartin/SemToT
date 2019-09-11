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

    public void OnMove(bool isMoving, bool isMovingBack)
    {
        if (isMoving)
        {
            if (isMovingBack)
                _myAnim.SetBool("isMovingBackwards", true);
            else
                _myAnim.SetBool("isMoving", true);
        }
        else
        {
            _myAnim.SetBool("isMoving", false);
            _myAnim.SetBool("isMovingBackwards", false);
        }
        
            
    }

    public void OnAttack(int slashNumbr)
    {
        //particulas?
        _myAnim.SetInteger("Slash", slashNumbr);
    }

    public void OnRangeAttack()
    {
        _myAnim.SetTrigger("RangeAttack");
    }

    public void OnSpecial()
    {
        _myAnim.SetTrigger("SpecialAttack");
    }

    public void OnDash()
    {
        _myAnim.SetTrigger("Dash");
    }

    public void OnJump()
    {
        _myAnim.SetTrigger("Jump");
    }

    public void OnIdle(bool isSlash)
    {
        if (isSlash)
            _myAnim.SetTrigger("AttackToIdle");
        else
            _myAnim.SetTrigger("ToIdle");
    }

    public void OnHit()
    {
        _myAnim.SetTrigger("GotHit");
    }

    public void OnDeath()
    {
        _myAnim.SetTrigger("Death");
    }
}
