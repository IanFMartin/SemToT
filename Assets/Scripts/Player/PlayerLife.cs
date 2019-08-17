﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour, IDamageable
{
    public float maxLife;
    internal float life;
    private Vector3 initialPos;
    public MyFloatEvent lifeEvent;
    public UnityEvent CurseClockUpdate;
    public UnityEvent deadEvent;
    public float curseDamage;
    public float curseTime;
    internal float maxCurseTime;
    internal float elapsedCurseTime;
    private Animator anim;
    internal bool dead;
    public GameObject damageParticle;

    private void Start()
    {
        anim = GetComponent<Animator>();
        maxCurseTime = curseTime;
        life = maxLife;
        lifeEvent.Invoke(life);
        initialPos = transform.position;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.L))
            Dead();

        Curse();
    }

    public void UpdateMaxLife(float newMax)
    {
        maxLife = newMax;
        lifeEvent.Invoke(life);
    }

    private void Curse()
    {
        elapsedCurseTime += Time.deltaTime;
        CurseClockUpdate.Invoke();
        if (elapsedCurseTime >= 1)
        {
            elapsedCurseTime = 0;
            if (!dead)
                TakeCurseDamage(curseDamage);
        }
    }

    public void TakeCurseDamage(float dmg)
    {
        curseTime -= dmg;
        if (curseTime > maxCurseTime)
            curseTime = maxCurseTime;
        //lifeEvent.Invoke(life);
        if (curseTime <= 0)
        {
            Dead();
        }
    }

    public void TakeDamage(float dmg)
    {
        if (!dead)
        {
            Shake.instance.shake = 0.05f;
            Shake.instance.shakeAmount = 0.05f;
            Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
            life -= dmg;
            lifeEvent.Invoke(life);
            if (life > maxLife)
                life = maxLife;
            if (life <= 0)
                Dead();
        }
    }

    public void Dead()
    {
        dead = true;
        anim.SetTrigger("die");
        this.gameObject.layer = 13;
        deadEvent.Invoke();
        WeaponTable.WeaponTableSingleton.weapon1ID = 1;
        WeaponTable.WeaponTableSingleton.weapon2ID = -1;
        GetComponent<Rigidbody>().drag = 50;
    }
}
