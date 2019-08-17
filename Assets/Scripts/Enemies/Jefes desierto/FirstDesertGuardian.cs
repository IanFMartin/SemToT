using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstDesertGuardian : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;

    public Bomb bomb;
    public GameObject zoneAttack;
    public List<GameObject> doorList = new List<GameObject>();

    public float cooldownSequence;
    private float timeToSequence;
    public float timeToAttack;
    public float timeSkill;
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
        var skill = new State<OnCondition>("Skill");
        var die = new State<OnCondition>("Dead");

        idle.OnUpdate += () =>
        {
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
            timeToAttack = 0;
            attacked = false;
            Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
            transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
        };
        attack.OnUpdate += () =>
        {
            timeToSequence += Time.deltaTime;
            timeToAttack += Time.deltaTime;
            timeSkill += Time.deltaTime;
            if (timeToAttack >= 1f && !attacked)
            {
                attacked = true;
                zoneAttack.gameObject.SetActive(false);
                anim.SetBool("Run", false);
                anim.SetTrigger("Attack");
            }
            if (timeToAttack > 1.5f)
            {
                fsm.Feed(OnCondition.Idle);
            }
            if (timeSkill >= 3)
            {
                //fsm.Feed(OnCondition.SkillReady); por ahora no
            }
            else
            {

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
                //anim.SetFloat("Attack", 1);
                //anim.SetBool("Run", false);
            }
            if (timeToAttack > 4f)
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
            Instantiate(deadParticle, transform.position, transform.rotation);
            GetComponent<Collider>().enabled = false;
            Destroy(this.gameObject, 3f);
        };

        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Attack, attack);
        idle.AddTransition(OnCondition.SkillReady, skill);
        idle.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.SkillReady, skill);
        persuit.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.SkillReady, skill);
        attack.AddTransition(OnCondition.Die, die);

        skill.AddTransition(OnCondition.Idle, idle);
        skill.AddTransition(OnCondition.Die, die);

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
        Die
    }

    public override void MyAttack()
    {
        BombAttack(target.position);
    }

    private void BombAttack(Vector3 posToAttack)
    {
        var zone = Instantiate(zoneAttack, new Vector3(posToAttack.x, 0.64f, posToAttack.z) + transform.forward, transform.rotation);
        zone.transform.Rotate(90, 0, 0);
        zone.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        zone.SetActive(true);
        var bomb = Instantiate(this.bomb, transform.position + transform.forward * 1.5f, transform.rotation);
        bomb.zone = zone;
        bomb.speed = 0.05f;
        float dist = Vector3.Distance(target.position, transform.position);
        bomb.speed += dist * 0.005f;
        bomb.dir = ((target.transform.position - transform.position) / 3 + Vector3.up * 2);
        bomb.myLayer = 10;
        bomb.dmg = 200;
    }

    public override void TakeDamage(float dmg)
    {
        Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
        StartCoroutine(DamageColor());
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

    IEnumerator DamageColor()
    {
        var rends = GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Color> myColor = new List<Color>();
        foreach (var rend in rends)
        {
            myColor.Add(rend.material.color);
            rend.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.15f);
        foreach (var color in myColor)
        {
            if (color == Color.red)
                continue;
            foreach (var rend in rends)
            {
                rend.material.color = color;
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
