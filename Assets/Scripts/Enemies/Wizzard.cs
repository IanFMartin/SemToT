using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizzard : Enemy
{
    public override void MyAttack()
    {
        weapon.Attack(transform.position + transform.forward.normalized, transform.rotation, 10);
    }
}
