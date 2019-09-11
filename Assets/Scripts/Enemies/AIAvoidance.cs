using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAvoidance : MonoBehaviour
{
    public float radFlock;
    public float radObst;
    public float avoidWeight;
    public float separationWeight;
    private Collider closerObstacle;
    public Vector3 vectSeparacion;
    public Vector3 vectAvoidance;
    public List<Collider> friends;
    public List<Collider> obstacles;
    public List<Collider> hero;
    private LayerMask enemieLayer;
    private LayerMask wallsLayer;
    private LayerMask playerLayer;
    private Enemy myBehavoiur;

    private void Start()
    {
        myBehavoiur = GetComponent<Enemy>();
        enemieLayer = 1 << 10;
        wallsLayer = 1 << 13;
        playerLayer = 1 << 12;
    }

    private void Update()
    {
        GetHero();
    }

    public void CalculateVectors()
    {
        GetFriendsAndObstacles();
        closerObstacle = GetCloserOb();
        vectSeparacion = new Vector3(GetSep().x * separationWeight, 0, GetSep().z * separationWeight);
        vectAvoidance = GetObstacleAvoidance() * avoidWeight;
    }

    private void GetHero()
    {
        hero.Clear();
        hero.AddRange(Physics.OverlapSphere(transform.position, myBehavoiur.sight, playerLayer));
        if (hero.Count > 0)
        {
            myBehavoiur.target = FindObjectOfType<PlayerLife>().transform;
        }
        else
        {
            myBehavoiur.target = null;
        }
    }


    public void GetFriendsAndObstacles()
    {
        friends.Clear();
        friends.AddRange(Physics.OverlapSphere(transform.position, radFlock, enemieLayer)); //Layer 10 Enemigos
        obstacles.Clear();
        obstacles.AddRange(Physics.OverlapSphere(transform.position, radObst, wallsLayer)); //Layer 13 Paredes
    }

    public Vector3 GetSep()
    {
        Vector3 sep = new Vector3();
        foreach (var item in friends)
        {
            Vector3 f = new Vector3();
            f = transform.position - item.transform.position;
            float mag = radFlock - f.magnitude;
            f.Normalize();
            f *= mag;
            sep += f;
        }
        return sep /= friends.Count();
    }

    public Collider GetCloserOb()
    {
        if (obstacles.Count > 0)
            return obstacles.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        else
            return null;
    }

    public Vector3 GetObstacleAvoidance()
    {
        if (closerObstacle)
            return new Vector3(transform.position.x - closerObstacle.transform.position.x, 0, transform.position.z - closerObstacle.transform.position.z);
        else return Vector3.zero;
    }

}
