using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dash : MonoBehaviour
{
    public GameObject dashEffect;
    private PlayerController pController;
    private Rigidbody _rb;
    public float power;
    public bool canDash;
    [HideInInspector]
    public bool isDashing;
    public float cooldown;
    private float timer;

    public MyFloatEvent setHabilityCooldown;
    public MyFloatEvent setCooldown;
    public UnityEvent skillNotReady;

    void Start()
    {
        pController = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody>();
        setHabilityCooldown.Invoke(cooldown);
    }

    void FixedUpdate()
    {
        if (timer >= cooldown) canDash = true;
        else timer++;
        setCooldown.Invoke(timer);

        if (Input.GetKeyDown(KeyCode.LeftShift) && !canDash)
            skillNotReady.Invoke();
        else if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            isDashing = true;
            OnDash();
        }
    }

    public void OnDash()
    {
        canDash = false;
        timer = 0;
        var dash = Instantiate(dashEffect, transform.position + transform.up.normalized, transform.rotation);
        dash.SetActive(true);
        dash.transform.parent = this.transform;
        if (pController.Direction().magnitude == 0)
            _rb.AddForce(transform.forward.normalized * pController.speed * _rb.mass * power, ForceMode.Impulse);
        else
            _rb.AddForce(pController.Direction() * pController.speed * _rb.mass * power, ForceMode.Impulse);
        StartCoroutine("VelocityToZero");
    }

    IEnumerator VelocityToZero()
    {
        yield return new WaitForSeconds(0.14f);
        _rb.velocity = Vector3.zero;
        isDashing = false;
        StopAllCoroutines();
    }
}
