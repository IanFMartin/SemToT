using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IGrabbable
{
    public float damage;
    public float originalDmg;
    public float TopSpecialDamage;
    public float specialDamage;
    public float rateOfFire;
    private float timeToFire;
    public bool allowAttack;
    public bool dropped;
    public bool hasSpecialAttack;
    public int iD;
    public GameObject trail;
    private GameObject myTrailInstance;
    private AudioSource audioSource;
    public AudioClip normalClip;
    public AudioClip specialClip;

    private void Start()
    {
        if (GetComponent<AudioSource>() != null)
            audioSource = GetComponent<AudioSource>();
        timeToFire = rateOfFire;
        originalDmg = damage;
    }

    private void Update()
    {
        timeToFire += Time.deltaTime;
        if (rateOfFire <= timeToFire)
            allowAttack = true;
    }

    public void Attack(Vector3 offset, Quaternion rot, int layer, Transform target = null)
    {
        if (allowAttack)
        {
            WeaponAttack(offset, rot, layer);
            timeToFire = 0;
            allowAttack = false;
        }
    }

    public void Attack(float extraDmg, Vector3 offset, Quaternion rot, int layer, Transform target = null)
    {
        if (allowAttack)
        {
            damage = this.originalDmg + extraDmg;
            WeaponAttack(offset, rot, layer);
            timeToFire = 0;
            allowAttack = false;
        }
    }

    public void StartAttack()
    {
        if (trail != null)
        {
            myTrailInstance = Instantiate(trail, trail.transform.position, trail.transform.rotation);
            myTrailInstance.transform.parent = this.transform;
            myTrailInstance.SetActive(true);
        }
    }

    public void EndAttack()
    {
        if (myTrailInstance != null)
            Destroy(myTrailInstance, 0.1f);
    }

    public void StartSpecialAttack(float extraDmg)
    {
        if (allowAttack)
        {
            specialDamage += Time.deltaTime * damage * (StaticData.level + 3);
            if (specialDamage >= TopSpecialDamage + extraDmg + 1)
            {
                specialDamage = TopSpecialDamage + extraDmg + 1;
            }
        }
    }

    public void EndSpecialAttack(Vector3 offset, Quaternion rot, int layer, Transform target = null)
    {
        if (allowAttack)
        {
            WeaponSpecialAttack(offset, rot, layer);
            timeToFire = 0;
            specialDamage = 0;
            allowAttack = false;
        }
    }

    public virtual void WeaponSpecialAttack(Vector3 offset, Quaternion rot, int layer)
    {
        if (specialClip != null && audioSource != null)
        {
            audioSource.clip = specialClip;
            audioSource.Play();
        }
    }

    public virtual void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        if (specialClip != null && audioSource != null)
        {
            audioSource.clip = normalClip;
            audioSource.Play();
        }
    }

    public void Grab(LootSystem looter)
    {
        looter.player.GetComponent<ChangeWeapon>().NewWeapon(this);
    }
}
