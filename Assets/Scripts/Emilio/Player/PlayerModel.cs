using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerView))]
public class PlayerModel : MonoBehaviour
{
    public float maxHealth;
    internal float _health;
    public float speed;
    public float rotationSpeed;
    Rigidbody _rb;

    public float maxCurseTime;
    float _currentCurseTime;
    public float curseDamage;
    bool _isDead;    

    #region Skills var
    public float dashForce;
    public float jumpForce;
    bool _canDash;
    bool _canJump;
    bool _canMove = true;
    public float dashCooldown;
    public float jumpCooldown;
    float _currentDashCooldown;
    float _currentJumpCooldown;
    bool jumping;
    public float jumpPower;
    public float jumpRadius;
    #endregion

    #region MVC vars
    PlayerView _myView;
    IController _keyboardController;
    IController _currentController;

    #endregion

    #region events
    public event Action<bool, bool> OnMove = delegate {};
    public event Action<int> OnAttack = delegate {};
    public event Action OnRangeAttack = delegate {};
    public event Action OnDash = delegate {};
    public event Action OnJump = delegate {};
    public event Action OnHit = delegate {};
    public event Action OnDeath = delegate {};
    public event Action OnSpecial = delegate {};
    public event Action<bool> OnIdle = delegate {};

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
    public float comboSlashTimer;
    float _currentComboTimer;
    bool _nextSlash;
    int _slashCounter;
    bool _isCharging;

    ClassChangeFeedback fbClass;
    #endregion

    //remove l8er
    public Camera cam;
    public UILife ui;
    public PlayerSpawner spawner;
    public GameObject damageParticle;
    public ParticleSystem healingParticle;
    bool _isPaused;
    public Text pauseText;
    public Image imageToFill;
    public bool DontHasTofill;
    public GameObject dashEffect;
    public GameObject smoke;
    public GameObject expansion;
    public GameObject crack;

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

        fbClass = GetComponent<ClassChangeFeedback>();
        LoadWeapons();
        fbClass.ChangeFeedback(currentWeapon);

        //delete l8er
        _isPaused = false;
        pauseText.gameObject.SetActive(false);
        healingParticle.gameObject.SetActive(false);
    }    

    void Update()
    {
        DashCooldown();
        JumpCooldown();
        ComboTimerUpdate();
        _currentController.OnUpdate();

        //pausa
        LifeUpdate();
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

            OnMove(_rb.velocity != Vector3.zero, Vector3.Angle(_rb.velocity, transform.forward) < 180);
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
                Shake.instance.shake = 0.1f;
                Shake.instance.shakeAmount = 0.1f;
                Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
            }

            _health -= dmg;
            ui.ShowLife(_health);
            if (_health > maxHealth)
                _health = maxHealth;
            if (_health <= 0)
                Death();
        }
    }

    void Death()
    {
        _isDead = true;
        OnDeath();
        this.gameObject.layer = 13;
        spawner.Respawn();
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

    void LifeUpdate()
    {
        Curse();
        if (imageToFill.gameObject.activeSelf && !DontHasTofill)
        {
            imageToFill.fillAmount = 1 - _currentCurseTime;
        }


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

    #endregion

    #region skills

    public void Dash()
    {
        if (_canDash)
        {
            _canDash = false;
            //xq speed? investigar cuando termine player
            _rb.AddForce(transform.forward * speed * dashForce, ForceMode.Impulse);
            //cambiar
            var dash = Instantiate(dashEffect, transform.position + transform.up.normalized, transform.rotation);
            dash.SetActive(true);
            dash.transform.parent = this.transform;
            OnDash();
            StartCoroutine(VelocityToZero());
        }

        dashInput = false;
    }

    public void Jump()
    {
        if (_canJump)
        {
            _canJump = false;
            _canMove = false;
            jumping = true;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Instantiate(smoke, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.rotation);

            OnJump();
        }

        jumpInput = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9 && jumping)
        {
            Instantiate(expansion, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.rotation);
            Instantiate(crack, new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z), transform.rotation);
            jumping = false;
            Shake.instance.shake = 0.2f;
            Shake.instance.shakeAmount = 0.2f;
            //_rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _canMove = true;
            Landed();
        }
    }

    public void Landed()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, jumpRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody colliderRb = hit.GetComponent<Rigidbody>();

            if (colliderRb != null && hit.gameObject.layer == 10)
            {
                colliderRb.AddExplosionForce(jumpPower, transform.position, jumpRadius);
                colliderRb.velocity = Vector3.zero;
            }
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

    public void Attack()
    {
        if (currentWeapon.allowAttack)
        {
            if (_nextSlash)
                _slashCounter++;

            _nextSlash = true;
            damage = str;

            Vector3 vectRot;
            Quaternion rot;
            //cambiar
            switch (_slashCounter)
            {
                case 0:
                    Shake.instance.shake = 0.06f;
                    Shake.instance.shakeAmount = 0.06f;
                    vectRot = new Vector3(0, 0, 30);
                    vectRot += transform.rotation.eulerAngles;
                    rot = Quaternion.Euler(vectRot);
                    currentWeapon.Attack(damage, transform.position + transform.forward.normalized / 1.2f + Vector3.up / 2, rot, 12);
                    break;
                case 1:
                    Shake.instance.shake = 0.07f;
                    Shake.instance.shakeAmount = 0.07f;
                    vectRot = new Vector3(0, 0, 180);
                    vectRot += transform.rotation.eulerAngles;
                    rot = Quaternion.Euler(vectRot);
                    currentWeapon.Attack(damage, transform.position + transform.forward.normalized * 1.2f + Vector3.up * 0.8f, rot, 12);
                    break;
                case 2:
                    Shake.instance.shake = 0.09f;
                    Shake.instance.shakeAmount = 0.09f;
                    vectRot = new Vector3(0, 0, 20);
                    vectRot += transform.rotation.eulerAngles;
                    rot = Quaternion.Euler(vectRot);
                    currentWeapon.Attack(damage, transform.position + transform.forward.normalized * 1.2f + Vector3.up, rot, 12);
                    break;
            }

            OnAttack(_slashCounter);
        }
    }

    void ComboTimerUpdate()
    {
        if (_nextSlash)
        {
            _currentComboTimer += Time.deltaTime;
            if (_currentComboTimer >= comboSlashTimer)
            {
                _nextSlash = false;
                _currentComboTimer = 0;
            }
        }else if(_slashCounter > 0)
        {
            _slashCounter = 0;
            StartCoroutine(ToIdle());
        }
        
    }

    public void StartSpecial()
    {
        if(currentWeapon.hasSpecialAttack && currentWeapon.allowAttack)
        {
            _isCharging = true;
            _canMove = false;
            OnIdle(false);//praticula
            chargeValue -= 0.02f;
            currentWeapon.StartSpecialAttack(str);

            if (currentWeapon.specialDamage >= currentWeapon.TopSpecialDamage + str)
            {
                EndSpecial();
            }
        }        
    }

    public void EndSpecial()
    {
        if (_isCharging)
        {
            _isCharging = false;
            chargeValue = 1;
            _canMove = true;
            OnSpecial();
            SpecialAttack();
        }
    }

    IEnumerator ToIdle()
    {
        yield return new WaitForSeconds(0.1f);
        OnIdle(true);
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

        fbClass.ChangeFeedback(currentWeapon);
        weaponUI.ReceiveWeapons(weapon1, weapon2, currentWeapon);
    }

    public void SwitchWeapon()
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
        OnIdle(false);
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
