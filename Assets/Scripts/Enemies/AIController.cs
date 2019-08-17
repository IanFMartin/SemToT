using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    internal StateMachine<OnCondition> fsm;
    private Enemy enemyType;

    public string stateName;

    void Awake()
    {
        enemyType = GetComponent<Enemy>();
        // States
        var patrol = new State<OnCondition>("Patrol");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var escape = new State<OnCondition>("Escape");
        var die = new State<OnCondition>("Die");

        // States functionality
        patrol.OnUpdate += () => enemyType.Patrol();

        attack.OnUpdate += () => enemyType.Attack();

        // Transitions
        patrol.AddTransition(OnCondition.Attack, attack);
        patrol.AddTransition(OnCondition.Escape, escape);
        patrol.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Patrol, patrol);
        attack.AddTransition(OnCondition.Escape, escape);
        attack.AddTransition(OnCondition.Die, die);

        escape.AddTransition(OnCondition.Patrol, patrol);
        escape.AddTransition(OnCondition.Attack, attack);
        escape.AddTransition(OnCondition.Die, die);

        fsm = new StateMachine<OnCondition>(patrol);
    }

    void Update()
    {
        fsm.Update();

        if (enemyType.target == null || !enemyType.target.gameObject.activeInHierarchy)
            fsm.Feed(OnCondition.Patrol);
        if (enemyType.target != null)
            fsm.Feed(OnCondition.Attack);

        stateName = fsm.currentState.name;
    }
}

public enum OnCondition
{
    Patrol,
    Persuit,
    Attack,
    Escape,
    Die
}

