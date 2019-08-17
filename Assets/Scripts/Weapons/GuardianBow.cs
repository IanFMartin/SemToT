using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianBow : Bow
{
    private int counter;
    public int rainRadius;
    public Enemy enemy;
    public GameObject zoneOfEffect;

    public override void WeaponAttack(Vector3 offset, Quaternion rot, int layer)
    {
        if (counter <= 0)
        {
            RainOfArrows(layer);
            counter = 5;
        }
        else
        {
            bulletPrefab.GetComponent<Bullet>().myLayer = layer;
            bulletPrefab.GetComponent<Bullet>().dmg = damage;
            var bullet = Instantiate(bulletPrefab, offset, rot);
            counter--;
        }
    }

    public void RainOfArrows(int layer)
    {
        if (enemy.target != null)
        {
            StartCoroutine(TimerBetweenArrows(layer));
        }
        }

    IEnumerator TimerBetweenArrows(int layer)
    {
        var centerPoint = enemy.target.gameObject.transform.position;
        var redZone = Instantiate(zoneOfEffect, centerPoint, Quaternion.identity);
        redZone.transform.Rotate(90, 0, 0);
        var originPoint = centerPoint;
        originPoint.y += 10;

        for (int i = 0; i < 80; i++)
        {
            originPoint.x = centerPoint.x + Random.Range(-rainRadius, rainRadius);
            originPoint.z = centerPoint.z + Random.Range(-rainRadius, rainRadius);
            bulletPrefab.GetComponent<Bullet>().myLayer = layer;
            bulletPrefab.GetComponent<Bullet>().dmg = damage;
            var bullet = Instantiate(bulletPrefab, originPoint, Quaternion.identity);
            bullet.transform.forward = Vector3.down;
           bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(redZone);
    }
    void OnDrawGizmos()
    {
        if(enemy.target != null)
        {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemy.target.gameObject.transform.position, rainRadius);
        }
    }
}
