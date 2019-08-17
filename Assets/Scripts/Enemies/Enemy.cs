using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public delegate void DropExpOrbHandler(float amountExpPerOrb);
public delegate void DropWeaponHandler();
public delegate void PositionDeathHandler(Vector3 t);
public class Enemy : MonoBehaviour, IDamageable
{
    public Transform target;
    public float maxLife;
    internal float life;
    public float lerpSpeed;
    public int sight;
    public int alertedSight;
    public bool alerted;
    public float range;
    public float amountExpPerOrb;
    internal float distanceToTarget;
    internal Weapon weapon;
    public Canvas healthBar;
    protected AIAvoidance aiAvoidance;
    protected NavMeshAgent navMeshAgent;
    protected Animator anim;
    public GameObject damageParticle;
    //Temporalmente acá
    public GameObject itemToSpawn;
    public event DropExpOrbHandler OnDeathExp = delegate { };
    public event DropWeaponHandler OnDeath = delegate { };
    public event PositionDeathHandler transformOnDeath = delegate { };

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = range - 0.2f;
        weapon = GetComponentInChildren<Weapon>();
        aiAvoidance = GetComponent<AIAvoidance>();
        anim = GetComponentInChildren<Animator>();
        life = maxLife;
        alertedSight = sight * 2;
    }

    public void Patrol()
    {
        anim.SetBool("Run", false);
    }

    protected void MoveToAgent()
    {
        if(target != null)
        navMeshAgent.SetDestination(target.position);
    }

    public virtual void OpenAttack()
    {

    }

    public virtual void Attack()
    {
        if (target != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            Vector3 dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
            if (distanceToTarget < sight && distanceToTarget > range)
            {
                MoveToAgent();
                anim.SetBool("Run", true);
            }
            else if (dirToGo.magnitude <= range)
            {
                dirToGo = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
                transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
                anim.SetBool("Run", false);
                anim.SetTrigger("Attack");
            }
            transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
        }
    }

    IEnumerator BeforeAttack(Vector3 dirToGo)
    {
        yield return new WaitForSeconds(1f);
    }

    public virtual void MyAttack()
    {
        weapon.Attack(transform.position + transform.forward.normalized, transform.rotation, 10, target);
        anim.SetBool("Run", false);
    }

    public virtual void TakeDamage(float dmg)
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
            Die();
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

    public virtual void Die()
    {
        if (weapon.GetComponent<Collider>())
            weapon.GetComponent<Collider>().enabled = false;
        SpawnWeaponUnlocked();
    }

    protected void SpawnWeaponUnlocked()
    {
        transformOnDeath(transform.position);
        OnDeath();
        OnDeathExp(amountExpPerOrb);
    }

    protected void AlertFriends()
    {
        LayerMask enemieLayer = 1 << 10;
        alerted = true;
        var myNearFriends = Physics.OverlapSphere(transform.position, 5, enemieLayer).Select(x => x.GetComponent<Enemy>());
        foreach (var friend in myNearFriends)
        {
            friend.sight = friend.alertedSight;
            friend.alerted = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sight);
    }
}

