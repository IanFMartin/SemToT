using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeWeapon : MonoBehaviour
{
    public Weapon currentWeapon;
    public Weapon weapon1;
    public Weapon weapon2;
    public GameObject specialCharge;
    private float chargeValue = 1;
    public float animValue;
    private float combotime;
    private float timerAnim;
    private Animator anim;
    public GameObject leftArm;
    public GameObject rightArm;
    private PlayerController playerController;
    private ClassChangeFeedback fbClass;
    private PlayerLife myLife;
    public float str;
    public float dex;
    private float damage;
    public WeaponsUI weaponsUI;

    void Start()
    {
        myLife = GetComponent<PlayerLife>();
        playerController = GetComponent<PlayerController>();
        fbClass = GetComponent<ClassChangeFeedback>();
        anim = GetComponent<Animator>();

        if (weapon1 != null)
            currentWeapon = weapon1;
        if (weapon2 != null)
            weapon2.gameObject.SetActive(false);

        LoadWeapons();
        fbClass.ChangeFeedback(currentWeapon);
    }

    void Update()
    {
        if (!myLife.dead)
        {
            if (currentWeapon.GetType() == typeof(Bow) && Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (currentWeapon.allowAttack)
                    BowAttack();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0) && currentWeapon.allowAttack && timerAnim <= 0)
                AnimationAttack();

            if (Input.GetKey(KeyCode.Mouse1) && currentWeapon.hasSpecialAttack && currentWeapon.allowAttack)
                StartSpecial();
            if (Input.GetKeyUp(KeyCode.Mouse1) && currentWeapon.hasSpecialAttack)
                EndSpecial();
            if (Input.GetKeyDown(KeyCode.Q))
                SwitchWeapon();
        }

        CombosTimer();
        TimerAnim();
    }

    internal void LoadWeapons()
    {
        if (WeaponTable.WeaponTableSingleton.weapon1ID != -1)
        {
            var weaponSaved = Instantiate(WeaponTable.WeaponTableSingleton.GetSavedWeapon(WeaponTable.WeaponTableSingleton.weapon1ID));
            weapon1 = LoadWeapon(weaponSaved.GetComponent<Weapon>(), weapon1);
        }
        if (WeaponTable.WeaponTableSingleton.weapon2ID != -1)
        {
            var weaponSaved = Instantiate(WeaponTable.WeaponTableSingleton.GetSavedWeapon(WeaponTable.WeaponTableSingleton.weapon2ID));
            weapon2 = LoadWeapon(weaponSaved.GetComponent<Weapon>(), weapon2);
        }
        if (WeaponTable.WeaponTableSingleton.currentWeaponSaved == 1)
            currentWeapon = weapon1;
        if (WeaponTable.WeaponTableSingleton.currentWeaponSaved == 2 && WeaponTable.WeaponTableSingleton.weapon2ID != -1)
            currentWeapon = weapon2;
        SwitchWeapon();
        SwitchWeapon();
        if (weaponsUI != null)
            weaponsUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }

    public void BowAttack()
    {
        anim.SetFloat("InputAttack", 50);
        damage = dex;
    }

    private void CombosTimer()
    {
        if (combotime > 0) combotime -= Time.deltaTime;
        else animValue = 0;
    }

    private void TimerAnim()
    {
        if (timerAnim > 0) timerAnim -= Time.deltaTime;
    }

    private void AnimationAttack()
    {
        timerAnim = 0.15f;
        animValue++;
        if (animValue == 1)
            anim.SetTrigger("FirstAttack");
        if (animValue == 2)
            anim.SetTrigger("SecondAttack");
        if (animValue == 3)
            anim.SetTrigger("ThirdAttack");
        //anim.SetFloat("InputAttack", animValue);
        damage = str;
    }

    private void StartSpecial()
    {
        playerController.canMove = false;
        playerController.ToIdle();
        var main = specialCharge.GetComponent<ParticleSystem>().main;
        chargeValue -= 0.02f;
        Color colorCharge = new Color(1, chargeValue, chargeValue);
        main.startColor = new ParticleSystem.MinMaxGradient(colorCharge);
        specialCharge.gameObject.SetActive(true);
        currentWeapon.StartSpecialAttack(str);
        if (currentWeapon.specialDamage >= currentWeapon.TopSpecialDamage + str)
        {
            EndSpecial();
        }
    }

    private void EndSpecial()
    {
        chargeValue = 1;
        playerController.canMove = true;
        specialCharge.gameObject.SetActive(false);
        anim.SetFloat("InputAttack", 101);
        StartCoroutine(ToIdle());
    }

    IEnumerator ToIdle()
    {
        yield return new WaitForSeconds(0.1f);
        anim.SetFloat("InputAttack", 0);
    }

    public void SpecialAttack()
    {
        currentWeapon.EndSpecialAttack(transform.position + transform.forward.normalized * 1.2f + Vector3.up, transform.rotation, 12);
        StartCoroutine(ToIdle());
        Shake.instance.shake = 0.12f;
        Shake.instance.shakeAmount = 0.14f;
    }

    public void StartAttack()
    {
        currentWeapon.StartAttack();
    }

    public void EndAttack()
    {
        currentWeapon.EndAttack();
    }

    public void Attack()
    {
        combotime = 0.5f;
        Shake.instance.shake = 0.06f;
        Shake.instance.shakeAmount = 0.06f;
        Vector3 vectRot = new Vector3(0, 0, 30);
        vectRot += transform.rotation.eulerAngles;
        Quaternion rot = Quaternion.Euler(vectRot);
        currentWeapon.Attack(damage, transform.position + transform.forward.normalized / 1.2f + Vector3.up / 2, rot, 12);
        StartCoroutine(ToIdle());
    }

    public void SecondAttack()
    {
        combotime = 0.4f;
        Shake.instance.shake = 0.07f;
        Shake.instance.shakeAmount = 0.07f;
        Vector3 vectRot = new Vector3(0, 0, 180);
        vectRot += transform.rotation.eulerAngles;
        Quaternion rot = Quaternion.Euler(vectRot);
        currentWeapon.Attack(damage, transform.position + transform.forward.normalized * 1.2f + Vector3.up * 0.8f, rot, 12);
        StartCoroutine(ToIdle());
    }

    public void ThirdAttack()
    {
        combotime = 0f;
        Shake.instance.shake = 0.09f;
        Shake.instance.shakeAmount = 0.09f;
        Vector3 vectRot = new Vector3(0, 0, 20);
        vectRot += transform.rotation.eulerAngles;
        Quaternion rot = Quaternion.Euler(vectRot);
        //Vector3 nextPos = transform.position + transform.forward.normalized;
        //transform.position = Vector3.Lerp(transform.position, nextPos, 0.5f);
        currentWeapon.Attack(damage, transform.position + transform.forward.normalized * 1.2f + Vector3.up, rot, 12);
        StartCoroutine(ToIdle());
    }

    private Weapon LoadWeapon(Weapon newWeapon, Weapon slotWeapon)
    {
        if (newWeapon.GetType() == typeof(Bow))
        {
            newWeapon.transform.position = leftArm.transform.position;
            newWeapon.transform.parent = leftArm.transform;
        }
        else
        {
            newWeapon.transform.position = rightArm.transform.position;
            newWeapon.transform.parent = rightArm.transform;
        }
        newWeapon.transform.rotation = currentWeapon.transform.rotation;
        currentWeapon.gameObject.SetActive(false);
        newWeapon.dropped = false;
        if (slotWeapon != null)
            Destroy(slotWeapon.gameObject);
        slotWeapon = newWeapon;
        currentWeapon = slotWeapon;

        weaponsUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
        return currentWeapon;
    }

    public void NewWeapon(Weapon newWeapon)
    {
        var posOnFloor = newWeapon.transform.position;
        if (newWeapon.GetType() == typeof(Bow))
        {
            newWeapon.transform.position = leftArm.transform.position;
            newWeapon.transform.parent = leftArm.transform;
        }
        else
        {
            newWeapon.transform.position = rightArm.transform.position;
            newWeapon.transform.parent = rightArm.transform;
        }

        newWeapon.transform.rotation = currentWeapon.transform.rotation;

        if (weapon1 == null)
        {
            currentWeapon.gameObject.SetActive(false);
            newWeapon.dropped = false;
            weapon1 = newWeapon;
            currentWeapon = weapon1;
        }
        else if (weapon2 == null)
        {
            currentWeapon.gameObject.SetActive(false);
            newWeapon.dropped = false;
            weapon2 = newWeapon;
            currentWeapon = weapon2;
        }
        else
        {
            currentWeapon.transform.parent = null;
            currentWeapon.transform.position = posOnFloor;
            if (currentWeapon == weapon1)
                weapon1 = newWeapon;
            else if (currentWeapon == weapon2)
                weapon2 = newWeapon;

            currentWeapon.dropped = true;
            newWeapon.dropped = false;
            currentWeapon = newWeapon;
        }

        fbClass.ChangeFeedback(currentWeapon);
        weaponsUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }

    void SwitchWeapon()
    {
        if (currentWeapon != null && weapon2 != null)
        {
            if (currentWeapon == weapon1)
            {
                weapon2.gameObject.SetActive(true);
                weapon1.gameObject.SetActive(false);
                currentWeapon = weapon2;
            }
            else if (currentWeapon == weapon2)
            {
                weapon1.gameObject.SetActive(true);
                weapon2.gameObject.SetActive(false);
                currentWeapon = weapon1;
            }
            fbClass.ChangeFeedback(currentWeapon);
        }
        if (weaponsUI != null)
            weaponsUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }
}
