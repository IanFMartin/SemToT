using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionalPlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 dir;
    public float speed;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        Movement();

        //Temporalmente el dash esta en esta clase
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rb.AddForce(Direction() * speed * 4, ForceMode.Impulse);
            StartCoroutine("VelocityToZero");
        }
    }

    IEnumerator VelocityToZero()
    {
        yield return new WaitForSeconds(0.14f);
        _rb.velocity = Vector3.zero;
    }

    private void Movement()
    {
        transform.forward = Vector3.Lerp(transform.forward, Direction(), 0.4f);
        transform.position += Direction() * speed * Time.deltaTime;
        _anim.SetFloat("InputV", Direction().magnitude);
    }

    private Vector3 Direction()
    {
        if (dir.magnitude < 0.2f)
            dir = Vector3.zero;


        if (Input.GetKey(KeyCode.W))
            dir += Vector3.forward * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            dir -= Vector3.forward * Time.deltaTime;
        else
            dir = Vector3.Lerp(dir, Vector3.zero, 1f);

        if (Input.GetKey(KeyCode.D))
            dir += Vector3.right * Time.deltaTime;
        else if (Input.GetKey(KeyCode.A))
            dir -= Vector3.right * Time.deltaTime;
        else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            dir = Vector3.Lerp(dir, Vector3.zero, 1f);

        return dir.normalized;
    }
}
