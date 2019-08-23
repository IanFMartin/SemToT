using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class FirstCityGuardian : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;

    public event Action<float, bool> ITookDamage;
    public GameObject childToSpawn;

    public float cooldownSequence;
    public float timeToAttack;
    private float timeToSkill;
    public List<GameObject> doorList = new List<GameObject>();
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

        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var skill = new State<OnCondition>("Skill");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            if (timeToSkill >= cooldownSequence && target != null)
                fsm.Feed(OnCondition.SkillReady);
            if (target != null)
            {
                fsm.Feed(OnCondition.Persuit);
                timeToSkill += Time.deltaTime;
            }
            else
                Patrol();
        };

        persuit.OnUpdate += () =>
        {
            if (target != null)
            {
                timeToSkill += Time.deltaTime;
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
            timeToSkill += Time.deltaTime;
            timeToAttack += Time.deltaTime;
            if (timeToSkill >= cooldownSequence)
            {
                fsm.Feed(OnCondition.SkillReady);
            }
            else
            {
                if (timeToAttack >= 0.5f)
                {
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
            cooldownSequence *= 1.5f;
            timeToSkill = 0;
            Clone();
            fsm.Feed(OnCondition.Idle);
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

        //////////////////

        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.SkillReady, skill);
        idle.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Idle, idle);
        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.SkillReady, skill);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Persuit, persuit);
        attack.AddTransition(OnCondition.SkillReady, skill);
        attack.AddTransition(OnCondition.Die, die);

        skill.AddTransition(OnCondition.Idle, idle);
        skill.AddTransition(OnCondition.Die, die);

        ///////////////

        fsm = new StateMachine<OnCondition>(idle);
    }

    private void Update()
    {
        fsm.Update();

        if (target == null)
            fsm.Feed(OnCondition.Idle);

        stateName = fsm.currentState.name;
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        base.TakeDamage(dmg, false);
        if (ITookDamage != null)
            ITookDamage.Invoke(dmg, false);
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

    public void Clone()
    {
        for (int i = 0; i < 1; i++)
        {
            var clony = Instantiate(childToSpawn, transform.position, transform.rotation);
            clony.GetComponent<FirstCityGuardian>().life = life;
            ITookDamage += clony.GetComponent<FirstCityGuardian>().TakeDamage;
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

    public enum OnCondition
    {
        Idle,
        Persuit,
        Attack,
        SkillReady,
        Die
    }
}
