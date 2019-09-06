using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerView))]
public class PlayerModel : MonoBehaviour
{
    public float maxHealth;
    float _health;
    public float speed;
    public float rotationSpeed;
    Rigidbody _rb;

    public float maxCurseTime;
    float _currentCurseTime;
    public float curseDamage;
    bool _isDead;    

    #region Skills var
    public float dashForce;
    bool _canDash;
    bool _canJump;
    bool _canMove = true;
    public float dashCooldown;
    public float jumpCooldown;
    float _currentDashCooldown;
    float _currentJumpCooldown;
    #endregion

    #region MVC vars
    PlayerView _myView;
    IController _keyboardController;
    IController _currentController;

    #endregion

    #region events
    //public event Action<bool> OnMove = delegate {};
    public event Action<int> OnAttack = delegate {};
    public event Action OnRangeAttack = delegate {};
    public event Action OnDash = delegate {};
    public event Action OnJump = delegate {};
    public event Action OnHit = delegate {};
    public event Action OnDeath = delegate {};
    public event Action OnSpecial = delegate {};
    public event Action OnIdle = delegate {};

    #endregion

    #region input vars
    [HideInInspector]
    public bool dashInput;
    [HideInInspector]
    public bool jumpInput;
    #endregion

    #region attack vars
    //cambiar todo esto
    public Weapon currentWeapon;
    public Weapon weapon1;
    public Weapon weapon2;
    public Transform weaponSocket;
    public WeaponsUI weaponUI;
    public float str;
    public float dex;
    float damage;
    float chargeValue;
    #endregion

    //remove l8er
    public Camera cam;

    void Start()
    {
        _isDead = false;
        _health = maxHealth;
        _rb = GetComponent<Rigidbody>();
        _keyboardController = new KeyboardController(this, GetComponent<PlayerView>());
        _currentController = _keyboardController;

        //weapon start, dps vemos q hacemos
        if (weapon1 != null)
            currentWeapon = weapon1;
        if (weapon2 != null)
            weapon2.gameObject.SetActive(false);

        //LoadWeapons();
        //fbClass.ChangeFeedback(currentWeapon);
    }    

    void Update()
    {
        DashCooldown();
        JumpCooldown();
        _currentController.OnUpdate();

        //pausa?
    }

    void FixedUpdate()
    {

        _currentController.OnFixedUpdate();
    }

    #region movement

    public void Move(float hAxis, float vAxis)
    {
        if (_canMove)
        {
            Vector3 moveHorizontal = Vector3.right * hAxis;
            Vector3 moveVertical = Vector3.forward * vAxis;

            var velocity = (moveHorizontal + moveVertical).normalized * speed;
            _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);

            //animator
        }
    }

    void MouseMovement()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float hitDist = 0f;

        if(playerPlane.Raycast(ray, out hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            var targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            //angleFwdRotation = Vector3.Angle(Vector3.forward, targetPoint - transform.position);
            //angleRightRotation = Vector3.Angle(Vector3.right, targetPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region curse + take damage

    void Curse()
    {
        _currentCurseTime += Time.deltaTime;
        if(_currentCurseTime >= maxCurseTime)
        {
            _currentCurseTime = 0;
            if (!_isDead)
                TakeDamage(curseDamage, true);
        }
    }

    public void TakeDamage(float dmg, bool isCurseDmg)
    {
        if (!_isDead)
        {
            if (!isCurseDmg)
            {
                //shake + particula
            }

            _health -= dmg;
            //updeatear el view para la ui
            if (_health > maxHealth)
                _health = maxHealth;
            if (_health <= 0)
                Death();
        }
    }

    void Death()
    {
        _isDead = true;
        //update animator
    }

    #endregion

    #region skills

    public void Dash()
    {
        if (_canDash)
        {
            _canDash = false;
            //xq speed? investigar cuando termine player
            _rb.AddForce(transform.forward * speed * dashForce, ForceMode.Impulse);
            //cambiar corutina dps
            StartCoroutine(VelocityToZero());
            //animator + particula + skillnotready ui
        }

        dashInput = false;
    }

    public void Jump()
    {
        if (_canJump)
        {
            _canJump = false;
            //ver eventos de animacion
        }
    }

    //delete l8er
    IEnumerator VelocityToZero()
    {
        yield return new WaitForSeconds(0.14f);
        _rb.velocity = Vector3.zero;
    }

    void DashCooldown()
    {
        if (!_canDash)
        {
            _currentDashCooldown += Time.deltaTime;
            if(_currentDashCooldown >= dashCooldown)
            {
                _canDash = true;
                _currentDashCooldown = 0;
            }
        }
    }

    void JumpCooldown()
    {
        if (!_canJump)
        {
            _currentJumpCooldown += Time.deltaTime;
            if (_currentJumpCooldown >= jumpCooldown)
            {
                _canJump = true;
                _currentJumpCooldown = 0;
            }
        }
    }

    #endregion

    //rehacer weapons
    #region attack

    private void StartSpecial()
    {
        _canMove = false;
        OnIdle();//praticula
        chargeValue -= 0.02f;
        currentWeapon.StartSpecialAttack(str);

        if (currentWeapon.specialDamage >= currentWeapon.TopSpecialDamage + str)
        {
            EndSpecial();
        }
    }

    private void EndSpecial()
    {
        chargeValue = 1;
        _canMove = true;
        OnSpecial();
        StartCoroutine(ToIdle());
    }

    IEnumerator ToIdle()
    {
        yield return new WaitForSeconds(0.1f);
        OnIdle();
    }

    public void SpecialAttack()
    {
        currentWeapon.EndSpecialAttack(transform.position + transform.forward.normalized * 1.2f + Vector3.up, transform.rotation, 12);
        StartCoroutine(ToIdle());
        Shake.instance.shake = 0.12f;
        Shake.instance.shakeAmount = 0.14f;
    }


    //xq esto?
    public void BowAttack()
    {
        OnRangeAttack();
        damage = dex;
    }

    //xq esto?
    public void StartAttack()
    {
        currentWeapon.StartAttack();
    }

    //xq esto?
    public void EndAttack()
    {
        currentWeapon.EndAttack();
    }

    void LoadWeapons()
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
        if (weaponUI != null)
            weaponUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }

    Weapon LoadWeapon(Weapon newWeapon, Weapon slotWeapon)
    {
        newWeapon.transform.position = weaponSocket.position;
        newWeapon.transform.parent = weaponSocket;
        newWeapon.transform.rotation = currentWeapon.transform.rotation;
        currentWeapon.gameObject.SetActive(false);
        newWeapon.dropped = false;
        if (slotWeapon != null)
            Destroy(slotWeapon.gameObject);
        slotWeapon = newWeapon;
        currentWeapon = slotWeapon;

        weaponUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
        return currentWeapon;
    }

    public void NewWeapon(Weapon newWeapon)
    {
        var posOnFloor = newWeapon.transform.position;
        
        newWeapon.transform.position = weaponSocket.position;
        newWeapon.transform.parent = weaponSocket;

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

        //fbClass.ChangeFeedback(currentWeapon);
        weaponUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
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
            //fbClass.ChangeFeedback(currentWeapon);
        }
        if (weaponUI != null)
            weaponUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }


    #endregion

    private void OnTriggerEnter(Collider c)
    {
        #region activatedoors
        if (c.gameObject.GetComponent<BarrierActivation>())
        {
            _canDash = false;
            _canJump = false;
            _canMove = false;

            Shake.instance.shake = 0.3f;
            Shake.instance.shakeAmount = 0.7f;
            //cambiar corutina l8er
            StartCoroutine(ActivateDoors(c.gameObject));
        }
        #endregion
    }

    //delete l8er
    IEnumerator ActivateDoors(GameObject go)
    {
        //volver a idle
        yield return new WaitForSeconds(1);
        if (go)
        {
            go.GetComponent<BarrierActivation>().ActivateDoors();
            Destroy(go);
        }
        _canDash = true;
        _canJump = true;
        _canMove = true;
    }

}
