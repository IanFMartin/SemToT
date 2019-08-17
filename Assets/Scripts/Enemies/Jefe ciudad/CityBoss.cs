using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBoss : Enemy, IBoss
{
    internal StateMachine<OnCondition> fsm;
    public string stateName;

    public GameObject zoneRunAttack;
    public GameObject trialRun;
    public List<GameObject> doorList = new List<GameObject>();

    public int runsPerSequence;
    private int runsQuantity;
    public float cooldownSequence;
    public float timeToAttack;
    private float timerSecuence;
    private float timerAttack;
    private float timerSkill;
    private float timerToRun;

    private bool inRage;
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
        xpPool = FindObjectOfType<ExpParticlesPool>();
        expController = FindObjectOfType<ExpController>();
        zoneRunAttack.SetActive(false);
        trialRun.SetActive(false);

        var idle = new State<OnCondition>("Idle");
        var persuit = new State<OnCondition>("Persuit");
        var attack = new State<OnCondition>("Attack");
        var skill = new State<OnCondition>("Skill");
        var resetRun = new State<OnCondition>("ResetSkill");
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
            if (timerAttack <= timeToAttack - 1)
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
            if (timerSkill >= cooldownSequence)
            {
                fsm.Feed(OnCondition.SkillReady);
            }
        };

        skill.OnEnter += () =>
        {
            Patrol();
            timerSkill = 0;
            timerAttack = 0;
            anim.speed = 5;
            trialRun.SetActive(true);
        };
        skill.OnUpdate += () =>
        {
            timerSecuence += Time.deltaTime;
            timerAttack += Time.deltaTime;

            if (timerAttack <= timeToAttack - 1)
            {
                Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                transform.forward = Vector3.Lerp(transform.forward, dirToGo, 0.8f);
            }
            if (timerAttack >= timeToAttack - 1 && timerAttack < timeToAttack)
            {
                zoneRunAttack.SetActive(true);
            }

            if (timerAttack >= timeToAttack)
            {
                zoneRunAttack.SetActive(false);
                anim.SetBool("Run", false);
                anim.SetTrigger("Attack");
                transform.position += transform.forward * 30 * Time.deltaTime;
                inRage = true;
            }
            //anim.SetFloat("Attack", 1);
            //anim.SetBool("Run", false);
            if (timerAttack > timeToAttack + 1f)
                fsm.Feed(OnCondition.ResetRun);
        };
        skill.OnExit += () =>
        {
            zoneRunAttack.SetActive(false);
            trialRun.SetActive(false);
            timerSkill = 0;
            timerAttack = 0;
            anim.speed = 1;
            inRage = false;
        };

        resetRun.OnEnter += () =>
        {
            runsQuantity++;
            Patrol();
        };
        resetRun.OnUpdate += () =>
        {
            timerToRun += Time.deltaTime;
            if (runsQuantity > runsPerSequence)
            {
                fsm.Feed(OnCondition.Idle);
            }
            else if (timerToRun > 0.5f)
                fsm.Feed(OnCondition.SkillReady);
        };
        resetRun.OnExit += () =>
        {

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
        skill.AddTransition(OnCondition.ResetRun, resetRun);

        resetRun.AddTransition(OnCondition.Idle, idle);
        resetRun.AddTransition(OnCondition.SkillReady, skill);
        resetRun.AddTransition(OnCondition.Die, die);

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
        ResetRun,
        Die
    }

    public override void Attack()
    {

    }

    public override void MyAttack()
    {
        Vector3 vectRot = new Vector3(0, 180, 0);
        vectRot += transform.rotation.eulerAngles;
        Quaternion rot = Quaternion.Euler(vectRot);
        weapon.Attack(400, transform.position + transform.forward.normalized * 3, rot, 10, target);
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

    public override void TakeDamage(float dmg)
    {
        Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
        sight = alertedSight;
        healthBar.gameObject.SetActive(true);
        StartCoroutine(DamageColor());
        life -= dmg;

        if (life >= maxLife)
            life = maxLife;

        if (life <= 0)
            fsm.Feed(OnCondition.Die);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (inRage && collision.gameObject.layer == 13)
            fsm.Feed(OnCondition.ResetRun);
        if (inRage && collision.gameObject.layer == 12)
        {
            var player = collision.gameObject.GetComponent<PlayerLife>();
            player.TakeDamage(800);
            fsm.Feed(OnCondition.Idle);
        }
    }
}
