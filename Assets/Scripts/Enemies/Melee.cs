using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Melee : Enemy
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;
    public Shader dissolve;
    public GameObject deadParticle;

    //private float beenDamage;

    float _prepTime;
    public float recoveryTime;
    public float maxPrepTime;
    public float AnticipationTime;
    public ParticleSystem prepParticule;

    //delete l8er
    bool _hasPlayParticle = false;

    void Start()
    {
        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var stun = new State<OnCondition>("Stun");
        var die = new State<OnCondition>("Dead");
        var prep = new State<OnCondition>("Prep");

        idle.OnUpdate += () =>
        {
            anim.SetBool("Run", false);
            if (target != null)
                fsm.Feed(OnCondition.Persuit);
            else
                Patrol();
            //beenDamage -= Time.deltaTime *2;
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
                //beenDamage -= Time.deltaTime *2;
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
                    fsm.Feed(OnCondition.Prep);
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
            /*
            if(beenDamage <= 0)
            {
                anim.SetTrigger("Attack");
            }
            else
            {
                fsm.Feed(OnCondition.Idle);
            }*/
            anim.SetTrigger("Attack");
        };

        attack.OnUpdate += () =>
        {
            _prepTime += Time.deltaTime;
            if (_prepTime >= recoveryTime)
            {
                fsm.Feed(OnCondition.Idle);
            }
        };

        attack.OnExit += () =>
        {
            _prepTime = 0;
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

        prep.OnEnter += () =>
        {
            anim.SetBool("Run", false);
            /*
            prepParticule.gameObject.SetActive(true);
            prepParticule.Play();*/
        };

        prep.OnUpdate += () =>
        {
            _prepTime += Time.deltaTime;

            //tiempo dps del frenado para prender la particula. delete cuando haya animacion de prep
            //if(_prepTime >= 0.3f && !_hasPlayParticle)
            //{
            //    prepParticule.gameObject.SetActive(true);
            //    prepParticule.Play();
            //    _hasPlayParticle = true;
            //}

            if (_prepTime >= maxPrepTime)
            {
                fsm.Feed(OnCondition.Attack);
            }
        };

        prep.OnExit += () =>
        {
            prepParticule.Stop();
            prepParticule.gameObject.SetActive(false);
            _prepTime = 0;
            _hasPlayParticle = false;
        };

        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Die, die);
        idle.AddTransition(OnCondition.Stun, stun);

        persuit.AddTransition(OnCondition.Idle, idle);
        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Die, die);
        persuit.AddTransition(OnCondition.Stun, stun);
        persuit.AddTransition(OnCondition.Prep, prep);

        stun.AddTransition(OnCondition.Idle, idle);
        stun.AddTransition(OnCondition.Persuit, persuit);
        stun.AddTransition(OnCondition.Attack, attack);
        stun.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Die, die);
        attack.AddTransition(OnCondition.Stun, stun);

        prep.AddTransition(OnCondition.Attack, attack);
        prep.AddTransition(OnCondition.Die, die);
        prep.AddTransition(OnCondition.Idle, idle);

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
        //fsm.Feed(OnCondition.Stun);
    }

    // para darle anticipacion
    public void StopEvent()
    {
        anim.speed = 0;
        StartCoroutine(TimerToAttack());
    }

    IEnumerator TimerToAttack()
    {
        yield return new WaitForSeconds(AnticipationTime);
        anim.speed = 1;
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
        Die,
        Prep
    }
}
