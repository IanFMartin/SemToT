using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    //delete l8er
    bool _isPaused;
    public Text pauseText;
    public Image imageToFill;
    //booleano para que no se llene la imagen en el nivel de la entrada 
    public bool DontHasTofill;

    //healing particle, esto dps va a otro lado
    public ParticleSystem healingParticle;

    private void Start()
    {
        anim = GetComponent<Animator>();
        maxCurseTime = curseTime;
        life = maxLife;
        lifeEvent.Invoke(life);
        initialPos = transform.position;

        //delete l8er
        _isPaused = false;
        pauseText.gameObject.SetActive(false);
        healingParticle.gameObject.SetActive(false);
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.L)) Dead();

        Curse();
        //Para que se vaya llenando la imagen en base a la duracion del siguiente tick de la maldicion. 
        if (imageToFill.gameObject.activeSelf && !DontHasTofill)
        {
            imageToFill.fillAmount = 1 - elapsedCurseTime;
        }

        //delete l8er
        if (Input.GetKeyDown(KeyCode.P))
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Time.timeScale = 0;
                pauseText.gameObject.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                pauseText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateMaxLife(float newMax)
    {
        maxLife = newMax;
        lifeEvent.Invoke(life);
    }

    private void Curse()
    {
        elapsedCurseTime += Time.deltaTime;
        //CurseClockUpdate.Invoke();
        if (elapsedCurseTime >= maxCurseTime)
        {
            elapsedCurseTime = 0;
            if (!dead)
                TakeDamage(curseDamage, true);
        }
    }
    /*
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
    }*/

    public void TakeDamage(float dmg, bool isCurseDmg)
    {
        if (!dead)
        {
            if (!isCurseDmg)
            {
                Shake.instance.shake = 0.1f;
                Shake.instance.shakeAmount = 0.1f;
                Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
            }

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

    public void PlayHealingParticule()
    {
        healingParticle.gameObject.SetActive(true);
        healingParticle.Play();
        StartCoroutine(StopParticle());
    }

    IEnumerator StopParticle()
    {
        yield return new WaitForSeconds(1f);
        healingParticle.Stop();
        healingParticle.gameObject.SetActive(false);
    }
}
