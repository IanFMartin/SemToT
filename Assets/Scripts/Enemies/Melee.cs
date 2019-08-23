using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Melee : Enemy
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;
    public Shader dissolve;
    public GameObject deadParticle;

    private float beenDamage;

    void Start()
    {
        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var stun = new State<OnCondition>("Stun");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            anim.SetBool("Run", false);
            if (target != null)
                fsm.Feed(OnCondition.Persuit);
            else
                Patrol();
            beenDamage -= Time.deltaTime;
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
                beenDamage -= Time.deltaTime;
                distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                if (distanceToTarget < sight && distanceToTarget > range)
                {
                    aiAvoidance.CalculateVectors();
                    dirToGo += aiAvoidance.vectSeparacion + aiAvoidance.vectAvoidance;
                    anim.SetBool("Run", true);
                    navMeshAgent.SetDestination(target.position);
                    //transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    //transform.position += transform.forward * 5 * Time.deltaTime;
                }
                else if (dirToGo.magnitude <= range)
                {
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    fsm.Feed(OnCondition.Attack);
                }
            }
        };
        persuit.OnExit += () =>
        {
            navMeshAgent.SetDestination(transform.position);
        };
        
        stun.OnEnter += () => anim.SetTrigger("Stun");
        stun.OnUpdate += () => fsm.Feed(OnCondition.Idle);

        attack.OnEnter += () =>
        {
            if(beenDamage <= 0)
            {
                anim.SetTrigger("Attack");
            }
            else
            {
                fsm.Feed(OnCondition.Idle);
            }
        };
        attack.OnUpdate += () =>
        {
            fsm.Feed(OnCondition.Idle);
            //StartCoroutine(ToIdle());
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
            GetComponent<Collider>().enabled = false;
            Destroy(this.gameObject, 3f);
        };

        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Die, die);
        idle.AddTransition(OnCondition.Stun, stun);

        persuit.AddTransition(OnCondition.Idle, idle);
        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Die, die);
        persuit.AddTransition(OnCondition.Stun, stun);

        stun.AddTransition(OnCondition.Idle, idle);
        stun.AddTransition(OnCondition.Persuit, persuit);
        stun.AddTransition(OnCondition.Attack, attack);
        stun.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Die, die);
        attack.AddTransition(OnCondition.Stun, stun);

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

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        base.TakeDamage(dmg, false);
        fsm.Feed(OnCondition.Stun);
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
        Stun,
        Die
    }
}
