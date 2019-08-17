using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondCityGuardian : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;

    public bool booleano;
    public float cooldownSequence;
    public float timeToAttack;
    private float timeToSkill;
    public float timeToAppear;
    private float followingTime;
    public GameObject redZone;
    public GameObject smoke;
    public GameObject expansion;
    private GameObject myRedZoneInstance;
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
        var dig = new State<OnCondition>("Dig");
        var appear = new State<OnCondition>("Appear");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            if (timeToSkill >= cooldownSequence && target != null)
                fsm.Feed(OnCondition.Dig);
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
                    //dirToGo += aiAvoidance.vectSeparacion + aiAvoidance.vectAvoidance;
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
                fsm.Feed(OnCondition.Dig);
            }
            else
            {
                if(timeToAttack < 0.3f)
                {
                    Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                    transform.forward = Vector3.Lerp(transform.forward, dirToGo, 0.09f);
                }

                if (timeToAttack >= 0.5f)
                {
                    anim.SetBool("Run", false);
                    anim.SetTrigger("Attack");
                }
                if (timeToAttack > 1f)
                {
                    fsm.Feed(OnCondition.Idle);
                }
            }
        };
        attack.OnExit += () =>
        {
            Patrol();
        };

        dig.OnEnter += () =>
        {
            timeToSkill = 0;
            booleano = true;
            myRedZoneInstance = Instantiate(redZone, transform.position - Vector3.up * 1.7f, transform.rotation);
            myRedZoneInstance.transform.Rotate(90, 0, 0);
            //transform.position = new Vector3(transform.position.x, 100000, transform.position.z);
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            GetComponentInChildren<Weapon>().GetComponent<MeshRenderer>().enabled = false;
        };
        dig.OnUpdate += () =>
        {
            followingTime += Time.deltaTime;
            myRedZoneInstance.transform.position = Vector3.Lerp(myRedZoneInstance.transform.position, target.transform.position, 0.015f);
            if (followingTime >= timeToAppear)
                fsm.Feed(OnCondition.Appear);
        };
        dig.OnExit += () =>
        {
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            GetComponentInChildren<Weapon>().GetComponent<MeshRenderer>().enabled = true;

            followingTime = 0;
        };

        appear.OnEnter += () =>
        {
            Instantiate(smoke, transform.position - Vector3.up * 1.5f, transform.rotation);
            transform.position = myRedZoneInstance.transform.position + Vector3.up * 30;
            Destroy(myRedZoneInstance);
        };
        appear.OnUpdate += () =>
        {
            //transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 2.5f, transform.position.z), 0.08f);
            int floorLayer = 1 << 9;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 3, floorLayer))
            {
                fsm.Feed(OnCondition.Idle);
            }
        };
        appear.OnExit += () =>
        {
            if (Vector3.Distance(target.transform.position, transform.position) < 3)
            {
                Shake.instance.shake = 0.08f;
                Shake.instance.shakeAmount = 0.1f;
                target.GetComponent<PlayerLife>().TakeDamage(500);
                Instantiate(smoke, transform.position - Vector3.up * 1.5f, transform.rotation);
                var exp = Instantiate(expansion, transform.position - Vector3.up * 1.5f, transform.rotation);
                //exp.transform.Rotate(90, 0, 0);
            }
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
        idle.AddTransition(OnCondition.Dig, dig);
        idle.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Idle, idle);
        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Dig, dig);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Persuit, persuit);
        attack.AddTransition(OnCondition.Dig, dig);
        attack.AddTransition(OnCondition.Die, die);

        dig.AddTransition(OnCondition.Idle, idle);
        dig.AddTransition(OnCondition.Appear, appear);
        dig.AddTransition(OnCondition.Die, die);

        appear.AddTransition(OnCondition.Idle, idle);
        appear.AddTransition(OnCondition.Die, die);

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

    public override void Attack()
    {

    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
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
        Dig,
        Appear,
        Die
    }
}
