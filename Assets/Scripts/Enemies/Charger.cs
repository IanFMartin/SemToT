using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Enemy
{
    public enum OnConditionCharger { IDLE, LOCKIN, CHARGE, STAGGER,DIE}
    StateMachine<OnConditionCharger> fsm;
    public string stateName;
    public Shader dissolve;
    public GameObject deadParticle;

    //public float distance;
    Vector3 _dir;
    public float damage;
    public float chargeTimer;
    public float stunTimer;
    public float chargeForce;
    public float bounceForce = 4;
    float _timer;
    bool _charge;

    // Start is called before the first frame update
    void Start()
    {
        var idle = new State<OnConditionCharger>("IDLE");
        var lockIn = new State<OnConditionCharger>("LOCKIN");
        var charge = new State<OnConditionCharger>("CHARGE");
        var stagger = new State<OnConditionCharger>("STAGGER");
        var die = new State<OnConditionCharger>("DIE");

        //logica

        idle.OnUpdate += () =>
        {
            anim.SetBool("Run", false);
            if (target != null)
                fsm.Feed(OnConditionCharger.LOCKIN);
            else
                Patrol();
        };

        lockIn.OnEnter += () =>
        {
            sight = alertedSight;
            if (target != null)
                transform.forward = Vector3.Lerp(transform.forward,
                    new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z), 0.5f);
        };

        lockIn.OnUpdate += () =>
        {
            if(target != null)
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
                    fsm.Feed(OnConditionCharger.CHARGE);
                }
            }            
        };

        lockIn.OnExit += () =>
        {
            //esto no se si es necesario
            //navMeshAgent.SetDestination(transform.position);
        };

        charge.OnEnter += () =>
        {
            anim.SetBool("Run", false);
            _charge = true;
            _dir = target.transform.position - transform.position;
            transform.forward = new Vector3(_dir.x, 0, _dir.z).normalized;
        };

        charge.OnUpdate += () =>
        {
            _timer += Time.deltaTime;

            if(_timer <= chargeTimer)
            {
                anim.SetBool("Run", true);//quizas se rompa aca
                GetComponent<Rigidbody>().AddForce(_dir.normalized * _timer * chargeForce, ForceMode.Acceleration);
            }

        };

        charge.OnExit += () =>
        {
            _charge = false;
            _timer = 0;
        };

        stagger.OnEnter += () =>
        {
            anim.SetBool("Run", false);
            _timer = 0;
        };

        stagger.OnUpdate += () =>
        {
            _timer += Time.deltaTime;

            if(_timer >= stunTimer)
            {
                _timer = 0;
                fsm.Feed(OnConditionCharger.IDLE);
                //particula de stuneado?
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

        idle.AddTransition(OnConditionCharger.LOCKIN, lockIn);
        idle.AddTransition(OnConditionCharger.DIE, die);

        lockIn.AddTransition(OnConditionCharger.CHARGE, charge);
        lockIn.AddTransition(OnConditionCharger.DIE, die);
        lockIn.AddTransition(OnConditionCharger.IDLE, idle);

        charge.AddTransition(OnConditionCharger.STAGGER, stagger);
        charge.AddTransition(OnConditionCharger.DIE, die);
        //charge.AddTransition(OnConditionCharger.IDLE, idle);

        stagger.AddTransition(OnConditionCharger.IDLE, idle);
        stagger.AddTransition(OnConditionCharger.DIE, die);

        fsm = new StateMachine<OnConditionCharger>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update();
        if (target == null && stateName != "CHARGE")
            fsm.Feed(OnConditionCharger.IDLE);

        stateName = fsm.currentState.name;
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        base.TakeDamage(dmg, isCurseDmg);
    }

    public override void Die()
    {
        SpawnWeaponUnlocked();
        fsm.Feed(OnConditionCharger.DIE);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_charge) return;        

        if (collision.collider.gameObject.layer == 13)
        {
            fsm.Feed(OnConditionCharger.STAGGER);
            _charge = false;
            GetComponent<Rigidbody>().AddForce(-transform.forward * bounceForce, ForceMode.VelocityChange);            Shake.instance.shake = 0.2f;
            Shake.instance.shakeAmount = 0.3f;
        }

        //cambiar x playermodel
        if (collision.gameObject.GetComponent<PlayerLife>())
        {
            var playerLife = collision.gameObject.GetComponent<PlayerLife>();
            playerLife.TakeDamage(damage, false);

            fsm.Feed(OnConditionCharger.STAGGER);
            _charge = false;
            GetComponent<Rigidbody>().AddForce(-transform.forward * bounceForce/2, ForceMode.VelocityChange);

            //quizas nmmo queda bien
            playerLife.GetComponent<Rigidbody>().AddForce(transform.forward * bounceForce / 2, ForceMode.VelocityChange);
        }
    }
}
