using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FirstBoss : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;

    public Transform tpPos;
    public GameObject smoke;
    public GameObject tpParticle;
    public GameObject enemySummon;
    public GameObject littleSmoke;
    public GameObject healSpell;
    public GameObject zoneAttack;
    private BossSword bossSword;
    public UnityEvent portal;
    public List<GameObject> doorList = new List<GameObject>();

    public float cooldownSequence;
    private float timeToSequence;
    public float timeToAttack;
    public float timeSkill;
    private EnemyWeaponDrop dropWeapon;
    public WeaponTable wT;
    private SpawnWeapon SW;
    public Shader dissolve;
    public GameObject deadParticle;
    private ExpParticlesPool xpPool;
    private ExpController expController;

    public List<Weapon> dropList = new List<Weapon>();
    public List<float> dropChanceList = new List<float>();
    private Dictionary<int, float> DicToUseInDrop = new Dictionary<int, float>();

    public void Start()
    {
        dropWeapon = new EnemyWeaponDrop();
        SW = new SpawnWeapon();
        var weaponTable = GameObject.Find("Weapon Table");
        wT = FindObjectOfType<WeaponTable>();
        xpPool = FindObjectOfType<ExpParticlesPool>();
        expController = FindObjectOfType<ExpController>();

        bossSword = GetComponentInChildren<BossSword>();
        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var skill = new State<OnCondition>("Skill");
        var teletransport = new State<OnCondition>("Teletransport");
        var summon = new State<OnCondition>("Summon");
        var heal = new State<OnCondition>("Heal");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            if (timeToSequence >= cooldownSequence && target != null)
                fsm.Feed(OnCondition.TP);
            if (target != null)
            {
                fsm.Feed(OnCondition.Persuit);
                timeToSequence += Time.deltaTime;
            }
            else
                Patrol();
        };

        persuit.OnUpdate += () =>
        {
            if (target != null)
            {
                timeToSequence += Time.deltaTime;
                distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                if (distanceToTarget < sight && distanceToTarget > range)
                {
                    aiAvoidance.CalculateVectors();
                    dirToGo += aiAvoidance.vectSeparacion + aiAvoidance.vectAvoidance;
                    anim.SetBool("Run", true);
                    navMeshAgent.SetDestination(target.transform.position);
                }
                else if (dirToGo.magnitude <= range)
                {
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                    fsm.Feed(OnCondition.Attack);
                }
            }
        };
        persuit.OnExit += () => navMeshAgent.SetDestination(transform.position);

        attack.OnEnter += () =>
        {
            sight = alertedSight;
            Patrol();
            timeToAttack = 0;
        };
        attack.OnUpdate += () =>
        {
            timeToSequence += Time.deltaTime;
            timeToAttack += Time.deltaTime;
            timeSkill += Time.deltaTime;
            if (timeSkill >= 3)
            {
                fsm.Feed(OnCondition.SkillReady);
            }
            else
            {
                if (timeToAttack >= 0.5f)
                {
                    zoneAttack.gameObject.SetActive(false);
                    anim.SetBool("Run", false);
                    anim.SetTrigger("Attack");
                }
                if (timeToAttack > 1.5f)
                {
                    fsm.Feed(OnCondition.Idle);
                }
            }
        };

        skill.OnEnter += () =>
        {
            zoneAttack.gameObject.SetActive(true);
            Patrol();
            timeSkill = 0;
        };
        skill.OnUpdate += () =>
        {
            timeToSequence += Time.deltaTime;
            timeToAttack += Time.deltaTime;

            if (timeToAttack >= 3f)
            {
                zoneAttack.gameObject.SetActive(false);
                bossSword.Attack(transform.position + transform.forward.normalized, transform.rotation, 10, target);
                //anim.SetFloat("Attack", 1);
                //anim.SetBool("Run", false);
            }
            if (timeToAttack > 4f)
                fsm.Feed(OnCondition.Idle);
        };

        teletransport.OnEnter += () =>
        {
            Patrol();
            Instantiate(tpParticle, transform.position, transform.rotation);
        };
        teletransport.OnUpdate += () =>
        {
            StartCoroutine(ToState(OnCondition.Summon, 0.3f));
        };
        teletransport.OnExit += () =>
        {
            transform.position = tpPos.position;
            Instantiate(smoke, transform.position, Quaternion.Euler(-90, 0, 0));
        };

        summon.OnEnter += () =>
        {

        };
        summon.OnUpdate += () => StartCoroutine(ToState(OnCondition.Heal, 0.5f));
        summon.OnExit += () =>
        {
            if (target != null)
                transform.forward = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);

            Instantiate(littleSmoke, transform.position + transform.forward.normalized, transform.rotation);
            Instantiate(littleSmoke, transform.position + transform.forward.normalized + transform.right.normalized, transform.rotation);
            Instantiate(littleSmoke, transform.position + transform.forward.normalized - transform.right.normalized, transform.rotation);
            Vector3 posToSpawn = new Vector3(transform.position.x, 1.5f, transform.position.z);
            Instantiate(enemySummon, posToSpawn + transform.forward.normalized, transform.rotation);
            timeToSequence = 0;
            cooldownSequence = cooldownSequence * 2;
        };

        heal.OnEnter += () => Instantiate(healSpell, transform.position, transform.rotation);
        heal.OnUpdate += () =>
        {
            TakeDamage(-5, false);
            StartCoroutine(ToState(OnCondition.Idle, 4f));
        };

        die.OnEnter += () =>
        {
            GetWeapon();
            for (int i = 0; i < 50; i++)
            {
                var p = xpPool.pool.GetObject();
                p.Spawn(new Vector3(transform.position.x, 0, transform.position.z));
                p.expController = this.expController;
            }
            Vector3 posToSpawn = new Vector3(transform.position.x, 0.5f, transform.position.z);
            Instantiate(itemToSpawn, posToSpawn, transform.rotation);
            portal.Invoke();
            DeactivateDoors(doorList);
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
        idle.AddTransition(OnCondition.TP, teletransport);
        idle.AddTransition(OnCondition.Summon, teletransport);
        idle.AddTransition(OnCondition.Heal, heal);
        idle.AddTransition(OnCondition.Die, die);


        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.TP, teletransport);
        persuit.AddTransition(OnCondition.Summon, summon);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.TP, teletransport);
        attack.AddTransition(OnCondition.SkillReady, skill);
        attack.AddTransition(OnCondition.Die, die);

        skill.AddTransition(OnCondition.Idle, idle);
        skill.AddTransition(OnCondition.Die, die);

        teletransport.AddTransition(OnCondition.Idle, idle);
        teletransport.AddTransition(OnCondition.Summon, summon);
        teletransport.AddTransition(OnCondition.Die, die);

        summon.AddTransition(OnCondition.Idle, idle);
        summon.AddTransition(OnCondition.Heal, heal);
        summon.AddTransition(OnCondition.Attack, attack);
        summon.AddTransition(OnCondition.Die, die);

        heal.AddTransition(OnCondition.Idle, idle);
        heal.AddTransition(OnCondition.Die, idle);

        fsm = new StateMachine<OnCondition>(idle);
    }

    IEnumerator ToState(OnCondition state, float time)
    {
        yield return new WaitForSeconds(time);
        fsm.Feed(state);
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
        SkillReady,
        Summon,
        TP,
        Heal,
        Die
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        base.TakeDamage(dmg, false);
        if (!alerted)
            AlertFriends();
        sight = alertedSight;
        healthBar.gameObject.SetActive(true);
        life -= dmg;
        if (life >= maxLife)
            life = maxLife;
        if (life <= 0)
        {
            fsm.Feed(OnCondition.Die);
        }
    }

    public void DeactivateDoors(List<GameObject> Doors)
    {
        if (Doors.Count > 0)
        {
            foreach (var door in Doors)
            {
                door.gameObject.SetActive(false);
            }
        }
    }

    private void GetWeapon()
    {
        foreach (var weapon in dropList)
        {

            GameObject weaponPrefab = weapon.gameObject;
            SW.Spawn(weaponPrefab, new Vector3(transform.position.x, 0, transform.position.z));
            if (!wT.weaponiDSpawn.Contains(weapon.iD))
            {
                wT.weaponiDSpawn.Add(weapon.iD);
                wT.weaponModelSpawn.Add(weapon.gameObject);
                wT.UpdateWeaponList();
            }

        }
    }
}
