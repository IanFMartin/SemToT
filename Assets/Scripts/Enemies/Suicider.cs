using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suicider : Enemy
{
    StateMachine<OnConditionSuicider> fsm;
    public string stateName;
    public Shader dissolve;
    public GameObject deadParticle;

    public float timeToExplode;
    float _timer;
    public float explosionRadius;
    public float explosionPower;
    public float explosionDamage;

    void Start()
    {
        var idle = new State<OnConditionSuicider>("IDLE");
        var chase = new State<OnConditionSuicider>("CHASE");
        var explode = new State<OnConditionSuicider>("EXPLODE");
        var die = new State<OnConditionSuicider>("DIE");

        idle.OnUpdate += () =>
        {
            anim.SetBool("Run", false);
            if (target != null)
                fsm.Feed(OnConditionSuicider.CHASE);
            else
                Patrol();
        };

        chase.OnEnter += () =>
        {
            sight = alertedSight;
            if (target != null)
                transform.forward = Vector3.Lerp(transform.forward,
                    new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z), 0.5f);
        };

        chase.OnUpdate += () =>
        {
            if (target != null)
            {
                distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                if (distanceToTarget < sight && distanceToTarget > range)
                {
                    aiAvoidance.CalculateVectors();
                    dirToGo += aiAvoidance.vectSeparacion + aiAvoidance.vectAvoidance;
                    anim.SetBool("Run", true);
                    navMeshAgent.SetDestination(target.position);
                }
                else if (dirToGo.magnitude <= range)
                {
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    fsm.Feed(OnConditionSuicider.EXPLODE);
                }
            }
        };

        chase.OnExit += () =>
        {
            //esto no se si es necesario
            //navMeshAgent.SetDestination(transform.position);
        };

        explode.OnEnter += () =>
        {
            anim.SetBool("Run", false);
            _timer = 0;
            //particula o algo q diga q va a explotar
        };

        explode.OnUpdate += () =>
        {
            _timer += Time.deltaTime;

            if(_timer >= timeToExplode)
            {
                fsm.Feed(OnConditionSuicider.EXPLODE);
            }

        };

        explode.OnExit += () =>
        {
            //explosion
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody colliderRb = hit.GetComponent<Rigidbody>();

                if (colliderRb == null) continue;

                if (colliderRb != null && hit.gameObject.layer == 10)
                {
                    colliderRb.AddExplosionForce(explosionPower, transform.position, explosionRadius);
                    colliderRb.velocity = Vector3.zero;
                    
                }

                //daño (deberia hacerle a todos)
                if (colliderRb.gameObject.GetComponent<IDamageable>() != null)
                {
                    colliderRb.gameObject.GetComponent<IDamageable>().TakeDamage(explosionDamage, false);
                }
            }
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

        idle.AddTransition(OnConditionSuicider.CHASE, chase);
        idle.AddTransition(OnConditionSuicider.DIE, die);

        chase.AddTransition(OnConditionSuicider.IDLE, idle);
        chase.AddTransition(OnConditionSuicider.EXPLODE, explode);
        chase.AddTransition(OnConditionSuicider.DIE, die);

        explode.AddTransition(OnConditionSuicider.DIE, die);

        fsm = new StateMachine<OnConditionSuicider>(idle);
    }
    
    void Update()
    {
        fsm.Update();
        if (target == null && stateName != "EXPLODE")
            fsm.Feed(OnConditionSuicider.IDLE);

        stateName = fsm.currentState.name;
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        base.TakeDamage(dmg, isCurseDmg);
    }

    public override void Die()
    {
        SpawnWeaponUnlocked();
        fsm.Feed(OnConditionSuicider.DIE);
    }

    public enum OnConditionSuicider { IDLE, CHASE, EXPLODE, DIE};

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
