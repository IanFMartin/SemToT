using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MobSpawner : MonoBehaviour
{
    public Transform startPosX;
    public Transform endPosX;
    public Transform startPosZ;
    public Transform endPosZ;
    public List<Enemy> typeOfEnemyInTheRoom;
    public int enemyPerRoom;
    private EnemyWeaponDrop dropWeapon;
    public WeaponTable wT;
    private SpawnWeapon SW;
    private Vector3 whereEnemyDie;
    private ExpParticlesPool xpPool;
    private ExpController expController;

    public void Awake()
    {
        SpawnMobs();
        dropWeapon = new EnemyWeaponDrop();
        SW = new SpawnWeapon();
        wT = FindObjectOfType<WeaponTable>();
        xpPool = FindObjectOfType<ExpParticlesPool>();
        expController = FindObjectOfType<ExpController>();
    }

    private void SpawnMobs()
    {
        for (int i = 0; i < enemyPerRoom; i++)
        {
            var tempEnemy = Instantiate(typeOfEnemyInTheRoom[Random.Range(0, typeOfEnemyInTheRoom.Count)],
                         new Vector3(Random.Range(startPosX.position.x, endPosX.position.x), 1, Random.Range(startPosZ.position.z, endPosZ.position.z)),
                         Quaternion.identity);
            tempEnemy.transform.parent = this.transform;
            tempEnemy.OnDeath += GetWeapon;
            tempEnemy.OnDeathExp += EnemyDie;
            tempEnemy.transformOnDeath += getTranform;
        }
    }

    private void EnemyDie(float amountExp)
    {
        for (int i = 0; i < 5; i++)
        {
            var p = xpPool.pool.GetObject();
            p.Spawn(whereEnemyDie);
            p.expController = this.expController;
            p.exp = amountExp;
        }
    }

    private void GetWeapon()
    {
        if (wT.dicWeaponPrefab.Count <= 0) return;
        int exp = Random.Range(0, 100);
        if (exp <= 3)
        {
            int index = dropWeapon.GetWeapon(wT.DicToUseInDrop);
            if (exp >= 0)
            {
                GameObject weaponPrefab = wT.dicWeaponPrefab[index];
                SW.Spawn(weaponPrefab, whereEnemyDie);
            }
        }

    }

    private void getTranform(Vector3 t)
    {
        whereEnemyDie = t;
    }
}
