using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : Enemy
{
    private SummmonerStaff summmonerStaff;
    private Staff otherStaff;
    private bool summoned;
    private int summonCount;
    private float timeElapsed;
    public Shader dissolve;
    public GameObject deadParticle;

    internal StateMachine<OnCondition> fsm;
    public string stateName;

    void Start()
    {
        summmonerStaff = GetComponentInChildren<SummmonerStaff>();
        otherStaff = GetComponentInChildren<Staff>();

        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            if (target != null)
                fsm.Feed(OnCondition.Persuit);
            else
                Patrol();
            timeElapsed += Time.deltaTime;
        };

        persuit.OnEnter += () =>
        {
            sight = alertedSight;
            if (target != null)
                transform.forward = Vector3.Lerp(transform.forward,
                    new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z), 0.5f);
        };
        persuit.OnUpdate += () =>
        {
            if (target != null)
            {
                timeElapsed += Time.deltaTime;
                distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                if (distanceToTarget < sight && distanceToTarget > range)
                {
                    aiAvoidance.CalculateVectors();
                    dirToGo += aiAvoidance.vectSeparacion + aiAvoidance.vectAvoidance;
                    anim.SetBool("Run", true);
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    transform.position += transform.forward * 5 * Time.deltaTime;
                }
                else if (dirToGo.magnitude <= range)
                {
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    fsm.Feed(OnCondition.Attack);
                }
            }
        };

        attack.OnEnter += () =>
        {
            if (summonCount == 0)
                summmonerStaff.Attack(transform.position + transform.forward.normalized * 2, transform.rotation, 10);
            else if (summonCount == 1)
                summmonerStaff.Attack(transform.position + (transform.forward.normalized - transform.right.normalized) * 3, transform.rotation, 10);
            else if (summonCount == 2)
                summmonerStaff.Attack(transform.position + (transform.forward.normalized + transform.right.normalized) * 3, transform.rotation, 10);
            else
                otherStaff.Attack(transform.position + transform.forward.normalized / 3, transform.rotation, 10);

            summonCount++;
            anim.SetBool("Run", false);
            anim.SetTrigger("Attack");
        };
        attack.OnUpdate += () =>
        {
            StartCoroutine(ToIdle());
        };
        die.OnEnter += () =>
        {
            float rnd = Random.value;
            Vector3 rndVector = new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
            if (rnd >= 0.7)
                Instantiate(itemToSpawn, transform.position + rndVector, transform.rotation);
            anim.speed = 0;
            var rends = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var rend in rends)
            {
                rend.material.shader = dissolve;
            }
            if (GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>() != null)
                GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>().enabled = false;
            Instantiate(deadParticle, transform.position, transform.rotation);
            timeElapsed = 0;
            if (GetComponent<Collider>() != null)
                GetComponent<Collider>().enabled = false;
            Destroy(this.gameObject, 3f);
        };

        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Die, die);

        fsm = new StateMachine<OnCondition>(idle);
    }

    IEnumerator ToIdle()
    {
        yield return new WaitForSeconds(1f);
        fsm.Feed(OnCondition.Idle);
    }

    public override void Attack()
    {
    }

    public override void Die()
    {
        SpawnWeaponUnlocked();
        fsm.Feed(OnCondition.Die);
    }

    void Update()
    {
        fsm.Update();

        if (target == null)
            fsm.Feed(OnCondition.Idle);

        stateName = fsm.currentState.name;
    }

    public enum OnCondition
    {
        Idle,
        Persuit,
        Attack,
        Die
    }
}
