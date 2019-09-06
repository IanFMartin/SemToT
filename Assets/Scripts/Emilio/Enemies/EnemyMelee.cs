using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;

public class EnemyMelee : EnemyBase
{


    
    enum MeleeInput { IDLE, PREP, PURSUIT, ATTACK, DIE}
    EventFSM<MeleeInput> _myFsm;

    void Start()
    {
        base.BaseStart();

        //cambiar ia2.state por state cuando se borre la otra clase de state
        var idle = new IA2.State<MeleeInput>("IDLE");
        var prep = new IA2.State<MeleeInput>("PREP");
        var pursuit = new IA2.State<MeleeInput>("PURSUIT");
        var attack = new IA2.State<MeleeInput>("ATTACK");
        var die = new IA2.State<MeleeInput>("DIE");

        StateConfigurer.Create(idle).SetTransition(MeleeInput.PREP, prep).SetTransition(MeleeInput.PURSUIT, pursuit).SetTransition(MeleeInput.DIE, die).Done();
        StateConfigurer.Create(prep).SetTransition(MeleeInput.ATTACK, attack).SetTransition(MeleeInput.DIE, die).SetTransition(MeleeInput.IDLE, idle).Done();
        StateConfigurer.Create(pursuit).SetTransition(MeleeInput.PREP, prep).SetTransition(MeleeInput.IDLE, idle).SetTransition(MeleeInput.DIE, die).Done();
        StateConfigurer.Create(attack).SetTransition(MeleeInput.IDLE, idle).SetTransition(MeleeInput.DIE, die).Done();
        StateConfigurer.Create(die).Done();




        _myFsm = new EventFSM<MeleeInput>(idle);
    }
    

    void Update()
    {
        _myFsm.Update();
    }
}
