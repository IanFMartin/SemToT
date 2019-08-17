using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Jump : MonoBehaviour
{
    public GameObject smoke;
    public GameObject expansion;
    public GameObject crack;
    public Dash dash;
    private Rigidbody _rb;
    public bool canJump;
    private bool jumping;
    public int jumpForce;
    public int fallMultiplier;
    private float timer;
    private int floorLayer;
    public float cooldown;
    public float radius = 5.0F;
    public float power = 10.0F;

    public MyFloatEvent setHabilityCooldown;
    public MyFloatEvent setCooldown;
    public UnityEvent skillNotReady;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        canJump = false;
        setHabilityCooldown.Invoke(cooldown);
        _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        floorLayer = 1 << 9;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !dash.isDashing)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Jumping();
        }
        else if (!canJump && Input.GetKeyDown(KeyCode.Space) && !jumping)
            skillNotReady.Invoke();

        if (_rb.velocity.y <= 0.1f && jumping)
        {
            _rb.velocity += new Vector3(0, 1 * Physics.gravity.y, 0) * (fallMultiplier) * Time.deltaTime;

        }
        if (!Physics.Raycast(transform.position, new Vector3(0, -1, 0), 10, floorLayer))
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void Update()
    {
        if (timer >= cooldown)
            canJump = true;
        else
            timer++;
        setCooldown.Invoke(timer);
    }


    public void Jumping()
    {
        anim.SetTrigger("jump");
        timer = 0;
        jumping = true;
        canJump = false;
        StartCoroutine(JumpAfter(0.2f));
    }

    IEnumerator JumpAfter(float t)
    {
        yield return new WaitForSeconds(t);
        _rb.AddForce(new Vector3(0, jumpForce * _rb.mass, 0), ForceMode.Impulse);
        Instantiate(smoke, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.rotation);
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
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Landed();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 9 && canJump)
        {
            canJump = false;
        }
    }

    public void Landed()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody colliderRb = hit.GetComponent<Rigidbody>();

            if (colliderRb != null && hit.gameObject.layer == 10)
            {
                colliderRb.AddExplosionForce(power, transform.position, radius);
                colliderRb.velocity = Vector3.zero;
            }
        }
    }
}
