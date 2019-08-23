using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FirstGuardian : Enemy, IBoss
{
    public Transform startPosX;
    public Transform endPosX;
    public Transform startPosZ;
    public Transform endPosZ;
    public float rangeOFDisseapear;
    public GameObject smoke;
    public List<GameObject> doorList = new List<GameObject>();
    public int timer;
    public List<Weapon> dropList = new List<Weapon>();
    public List<float> dropChanceList = new List<float>();
    private Dictionary<int, float> DicToUseInDrop = new Dictionary<int, float>();
    private EnemyWeaponDrop dropWeapon;
    public WeaponTable wT;
    private SpawnWeapon SW;
    public Shader dissolve;
    private bool dead;

    private ExpParticlesPool xpPool;
    private ExpController expController;

    public void Start()
    {
        dropWeapon = new EnemyWeaponDrop();
        SW = new SpawnWeapon();
        var weaponTable = GameObject.Find("Weapon Table");
        wT = FindObjectOfType<WeaponTable>();

        xpPool = FindObjectOfType<ExpParticlesPool>();
        expController = FindObjectOfType<ExpController>();
        for (int i = 0; i < dropList.Count; i++)
        {
            DicToUseInDrop.Add(dropList[i].iD, dropChanceList[i]);
        }
    }

    private void Update()
    {
      if (target != null)
        {
            var distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget < rangeOFDisseapear && !dead)
            {
                if (timer <= 0)
                {
                    Dissapear();
                    timer = 100;
                }
                else
                    timer--;
            }
        }
    }

    public override void Attack()
    {
        if (!dead)
            base.Attack();
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        if (!alerted)
            AlertFriends();
        sight = alertedSight;
        healthBar.gameObject.SetActive(true);
        life -= dmg;

        if (life <= 0)
        {
            Dead();
        }
        else
            StartCoroutine(DamageColor());
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

    private void Dead()
    {
        for (int i = 0; i < 25; i++)
        {
            var p = xpPool.pool.GetObject();
            p.Spawn(new Vector3(transform.position.x, 0, transform.position.z));
            p.expController = this.expController;
        }
        Vector3 posToSpawn = new Vector3(transform.position.x, 0.5f, transform.position.z);
        Instantiate(itemToSpawn, posToSpawn, transform.rotation);
        dead = true;
        DeactivateDoors(doorList);
        GetWeapon();
        anim.speed = 0;
        if (GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>() != null)
            GetComponentInChildren<Weapon>().gameObject.GetComponent<Collider>().enabled = false;
        GetComponent<Collider>().enabled = false;
        var rends = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var rend in rends)
        {
            rend.material.shader = dissolve;
        }
        Destroy(this.gameObject, 3f);
    }

    public void Dissapear()
    {
        var tempPos = NewPosition();
        var exp = Instantiate(smoke, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), Quaternion.identity);
        exp.transform.forward = Vector3.up;
        transform.position = new Vector3(1000, 1000, 1000);
        StartCoroutine(TimerDissapearance());
        Instantiate(exp, tempPos, Quaternion.identity);
        transform.position = tempPos;
    }

    IEnumerator TimerDissapearance()
    {
        yield return new WaitForSeconds(0.3f);
    }

    private Vector3 NewPosition()
    {
        var tempPos = new Vector3(Random.Range(startPosX.position.x, endPosX.position.x), transform.position.y, Random.Range(startPosZ.position.z, endPosZ.position.z));
        while (Vector3.Distance(tempPos, target.position) < rangeOFDisseapear)
            tempPos = new Vector3(Random.Range(startPosX.position.x, endPosX.position.x), transform.position.y, Random.Range(startPosZ.position.z, endPosZ.position.z));

        return tempPos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangeOFDisseapear);
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
