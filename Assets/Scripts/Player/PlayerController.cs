using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera cam;
    public Quaternion targetRotation;
    private float angleFwdRotation;
    private float angleRightRotation;
    public float rotationSpeed;
    public float speed;
    private float hSpeed;
    internal Vector3 dir;
    public bool canMove = true;
    private PlayerLife myLife;

    public Animator anim;
    private float inputH;
    private float inputV;

    void Start()
    {
        anim = GetComponent<Animator>();
        myLife = GetComponent<PlayerLife>();
    }

    void Update()
    {
        if(!myLife.dead)
        {
            MouseRotation();
            if (canMove)
            {
                Movement();
            }
        }
    }

    public void ToIdle()
    {
        inputV = 0;
        inputH = 0;
        anim.SetFloat("InputH", inputH);
        anim.SetFloat("InputV", inputV);
    }

    private void Movement()
    {
        inputV = 0;
        inputH = 0;

        transform.position += Direction() * speed * Time.deltaTime;

        anim.SetFloat("InputH", inputH);
        anim.SetFloat("InputV", inputV);
    }

    public Vector3 Direction()
    {
        if (dir.magnitude < 0.2f)
            dir = Vector3.zero;        

        if (Input.GetKey(KeyCode.W))
        {
            SetAnimPoseVertical(1, -1, -1, 1);
            dir += Vector3.forward * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            SetAnimPoseVertical(-1, 1, 1, -1);
            dir -= Vector3.forward * Time.deltaTime;
        }
        else
            dir = Vector3.Lerp(Vector3.forward, Vector3.zero, 1f);

        if (Input.GetKey(KeyCode.D))
        {
            SetAnimPoseHorizontal(1, -1, 1, -1);
            dir += Vector3.right * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            SetAnimPoseHorizontal(-1, 1, -1, 1);
            dir -= Vector3.right * Time.deltaTime;
        }
        else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            dir = Vector3.Lerp(Vector3.right, Vector3.zero, 1f);

        return dir.normalized;
    }

    private void SetAnimPoseVertical(int vRight, int vLeft, int hUp, int hDown)
    {
        if (angleFwdRotation < 45)
            inputV = vRight;
        else if (angleFwdRotation > 135)
            inputV = vLeft;
        if (angleRightRotation < 45)
            inputH = hUp;
        else if (angleRightRotation > 135)
            inputH = hDown;
    }

    private void SetAnimPoseHorizontal(int hRight, int hLeft, int vUp, int vDown)
    {
        if (angleFwdRotation < 45)
            inputH = hRight;
        else if (angleFwdRotation > 135)
            inputH = hLeft;
        if (angleRightRotation < 45)
            inputV = vUp;
        else if (angleRightRotation > 135)
            inputV = vDown;
    }

    private void MouseRotation()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float hitDist = 0f;

        if (playerPlane.Raycast(ray, out hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            angleFwdRotation = Vector3.Angle(Vector3.forward, targetPoint - transform.position);
            angleRightRotation = Vector3.Angle(Vector3.right, targetPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //Cursor.visible = false;
        }
    }
}