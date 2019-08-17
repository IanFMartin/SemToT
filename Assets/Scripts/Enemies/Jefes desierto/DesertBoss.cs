using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DesertBoss : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;
    
    public GameObject healParticles;
    public GameObject meteorite;
    public GameObject explotionFire;
    public GameObject cirlceZone;
    public List<GameObject> doorList = new List<GameObject>();

    public float cooldownSequence;
    public float timeToAttack;
    private float timerSecuence;
    private float timerAttack;
    private float timerSkill;
    private float timerToIdle;
    private int meteoritesCount;
    
    private bool attacked;

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

    public UnityEvent portal;

    public void Start()
    {
        dropWeapon = new EnemyWeaponDrop();
        SW = new SpawnWeapon();
        var weaponTable = GameObject.Find("Weapon Table");
        wT = weaponTable.GetComponent<WeaponTable>();
        xpPool = FindObjectOfType<ExpParticlesPool>();
        expController = FindObjectOfType<ExpController>();

        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var meteorSkill = new State<OnCondition>("Skill");
        var explotionSkill = new State<OnCondition>("Explotion");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
            if (target != null)
            {
                fsm.Feed(OnCondition.Persuit);
                timerSecuence += Time.deltaTime;
            }
            else
                Patrol();
        };

        persuit.OnUpdate += () =>
        {
            if (target != null)
            {
                timerSecuence += Time.deltaTime;
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
            sight = alertedSight;
            Patrol();
            timerAttack = 0;
            attacked = false;
        };
        attack.OnUpdate += () =>
        {
            timerSecuence += Time.deltaTime;
            timerAttack += Time.deltaTime;
            timerSkill += Time.deltaTime;
            if (timerAttack <= timeToAttack - 0.3f)
            {
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                transform.forward = Vector3.Lerp(transform.forward, dirToGo, 1);
            }
            if (timerAttack >= timeToAttack && !attacked)
            {
                attacked = true;
                anim.SetBool("Run", false);
                anim.SetTrigger("Attack");
            }
            if (timerAttack > timeToAttack + 1)
            {
                fsm.Feed(OnCondition.Idle);
            }

            if (timerSkill > cooldownSequence && Vector3.Distance(target.transform.position, transform.position) < 3.5f)
                fsm.Feed(OnCondition.ExplotionSkill);
            else if (timerSkill >= cooldownSequence && meteoritesCount < 3)
            {
                fsm.Feed(OnCondition.Meteorites);
            }
        };

        meteorSkill.OnEnter += () =>
        {
            Patrol();
            timerSkill = 0;
            timerAttack = 0;
            anim.speed = 2;
        };
        meteorSkill.OnUpdate += () =>
        {
            timerSecuence += Time.deltaTime;
            timerAttack += Time.deltaTime;

            if (timerAttack <= timeToAttack - 1)
            {
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                transform.forward = Vector3.Lerp(transform.forward, dirToGo, 0.8f);
            }

            if (timerAttack > timeToAttack)
                fsm.Feed(OnCondition.Idle);
        };
        meteorSkill.OnExit += () =>
        {
            var met = Instantiate(meteorite, target.transform.position + Vector3.up * 10, transform.rotation);
            met.GetComponent<Bomb>().dir = (target.transform.position - met.transform.position).normalized;
            met.GetComponent<Bomb>().speed = 0.05f;
            met.GetComponent<Bomb>().myLayer = 10;
            meteoritesCount++;
            cooldownSequence *= 2;
            timerSkill = 0;
            timerAttack = 0;
            anim.speed = 1;
        };

        explotionSkill.OnEnter += () =>
        {
            Patrol();
            cirlceZone.SetActive(true);
        };
        explotionSkill.OnUpdate += () =>
        {
            timerToIdle += Time.deltaTime;
            if (timerToIdle >= 1f)
                fsm.Feed(OnCondition.Idle);
        };
        explotionSkill.OnExit += () =>
        {
            cirlceZone.SetActive(false);
            var fire = Instantiate(explotionFire, new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);
            fire.transform.Rotate(90, 0, 0);
            fire.GetComponent<MakeAreaDamage>().myLayer = 10;
            Shake.instance.shake = 0.1f;
            Shake.instance.shakeAmount = 0.1f;
            timerToIdle = 0;
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
            Instantiate(deadParticle, transform.position, transform.rotation);
            GetComponent<Collider>().enabled = false;
            Destroy(this.gameObject, 3f);
        };

        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.Meteorites, meteorSkill);
        idle.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Meteorites, meteorSkill);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.ExplotionSkill, explotionSkill);
        attack.AddTransition(OnCondition.Meteorites, meteorSkill);
        attack.AddTransition(OnCondition.Die, die);

        meteorSkill.AddTransition(OnCondition.Idle, idle);
        meteorSkill.AddTransition(OnCondition.Die, die);
        meteorSkill.AddTransition(OnCondition.ExplotionSkill, explotionSkill);

        explotionSkill.AddTransition(OnCondition.Idle, idle);
        explotionSkill.AddTransition(OnCondition.Die, die);

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
        Meteorites,
        ExplotionSkill,
        Die
    }

    public override void Attack()
    {

    }

    public override void MyAttack()
    {
        weapon.Attack(80, transform.position + transform.forward.normalized, transform.rotation, 10, target);
    }

    /*
    private void SpearAttack(Vector3 posToAttack)
    {
        zoneAttack.SetActive(false);
        var bomb = Instantiate(this.bomb, transform.position + transform.forward * 1.5f - Vector3.up, transform.rotation);
        bomb.speed = 0.05f;
        float dist = Vector3.Distance(target.position, transform.position);
        bomb.speed += dist * 0.01f;
        bomb.dir = ((target.transform.position - transform.position) / 3 + Vector3.up * 2);
        bomb.myLayer = 10;
    }*/

    public void Heal(float amount)
    {
        Vector3 vectRot = new Vector3(-90, 0, 0);
        vectRot += transform.rotation.eulerAngles;
        Quaternion rot = Quaternion.Euler(vectRot);
        Instantiate(healParticles, transform.position, rot);
        life += amount;

        if (life >= maxLife)
            life = maxLife;
    }

    public override void TakeDamage(float dmg)
    {
        sight = alertedSight;
        healthBar.gameObject.SetActive(true);

        life -= dmg;

        if (life >= maxLife)
            life = maxLife;

        if (life <= 0)
            fsm.Feed(OnCondition.Die);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 13)
            fsm.Feed(OnCondition.ExplotionSkill);
    }
}
