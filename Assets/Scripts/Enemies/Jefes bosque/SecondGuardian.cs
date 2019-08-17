using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondGuardian : Enemy, IBoss
{
    public GameObject whirlwindEffect;
    public GameObject redZone;
    public GameObject archerSummon;
    public GameObject smokeEffect;
    public List<GameObject> doorList = new List<GameObject>();
    public float cdWhirlWindSpell;
    private float spellTime;
    public float cdSummon;
    private float summonTime;
    public Transform startPosX;
    public Transform endPosX;
    public Transform startPosZ;
    public Transform endPosZ;
    private bool spelling;
    private bool invulnerable;
    private EnemyWeaponDrop dropWeapon;
    public WeaponTable wT;
    private SpawnWeapon SW;
    public Shader dissolve;
    private bool dead;

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
    }
    public override void OpenAttack()
    {
        base.OpenAttack();
        spellTime = cdWhirlWindSpell;
    }

    public override void Attack()
    {
        if(!dead)
        {
            spellTime += Time.deltaTime;
            summonTime += Time.deltaTime;
            if (summonTime >= cdSummon)
            {
                Summon();
            }
            if (spellTime >= cdWhirlWindSpell && distanceToTarget < range)
            {
                StartCoroutine(AttackSecuence());
            }
            else if (!spelling)
                base.Attack();
        }

    }

    private void Summon()
    {
        summonTime = 0;
        var pos = rndPos();
        var smoke = Instantiate(smokeEffect, pos, transform.rotation);
        var mob = Instantiate(archerSummon, pos, transform.rotation);
        mob.GetComponent<Enemy>().target = this.target;
    }

    private Vector3 rndPos()
    {
        return new Vector3(UnityEngine.Random.Range(startPosX.position.x, endPosX.position.x), 1.5f,
                                                                UnityEngine.Random.Range(startPosZ.position.z, endPosZ.position.z));
    }

    public override void TakeDamage(float dmg)
    {
        if (!invulnerable)
            base.TakeDamage(dmg);
        if (life <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        dead = true;
        for (int i = 0; i < 35; i++)
        {
            var p = xpPool.pool.GetObject();
            p.Spawn(new Vector3(transform.position.x, 0, transform.position.z));
            p.expController = this.expController;
        }
        Vector3 posToSpawn = new Vector3(transform.position.x, 0.5f, transform.position.z);
        Instantiate(itemToSpawn, posToSpawn, transform.rotation);
        DeactivateDoors(doorList);
        GetWeapon();
        Die();
        anim.speed = 0;
        var rends = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>() != null)
            GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>().enabled = false;
        GetComponent<Collider>().enabled = false;
        foreach (var rend in rends)
        {
            rend.material.shader = dissolve;
        }
        Destroy(this.gameObject, 3f);
    }

    IEnumerator AttackSecuence()
    {
        invulnerable = true;
        spelling = true;
        anim.SetBool("Run", false);
        spellTime = 0;
        var redAlert = Instantiate(redZone, new Vector3(transform.position.x, 0.8f, transform.position.z), Quaternion.Euler(90, 0, 0));
        yield return new WaitForSeconds(1f);
        var spell = Instantiate(whirlwindEffect, transform.position - Vector3.up, transform.rotation);
        spell.GetComponent<MakeAreaDamage>().myLayer = this.gameObject.layer;
        yield return new WaitForSeconds(4f);
        spelling = false;
        invulnerable = false;
        Destroy(spell);
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
