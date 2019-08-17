using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerriesBush : ShakePlants, IDamageable
{
    public GameObject berrylessBush;
    public GameObject enemy;
    public GameObject healthOrb;
    public Transform placeHolder;
    private EnemyWeaponDrop dropWeapon;
    private WeaponTable wT;
    private SpawnWeapon SW;

    private void Start()
    {
        dropWeapon = new EnemyWeaponDrop();
        SW = new SpawnWeapon();
        wT = FindObjectOfType<WeaponTable>();

    }
    private void GetWeapon()
    {
        if (wT.dicWeaponPrefab.Count <= 0) return;
        int index = dropWeapon.GetWeapon(wT.DicToUseInDrop);
        GameObject weaponPrefab = wT.dicWeaponPrefab[index];
        SW.Spawn(weaponPrefab, placeHolder.position + new Vector3(Random.Range(-0.5f, 0.8f), 0, Random.Range(-0.5f, 0.8f)));
    }
    void GetOrb()
    {
        int exp = Random.Range(1,6);
        for (int i = 0; i < (int)exp; i++)
        {
        Vector3 rndVector = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
            Instantiate(healthOrb, placeHolder.position, transform.rotation);
        }
    }

    void Summon()
    {
        int exp = Random.Range(1, 6);
        for (int i = 0; i < (int)exp; i++)
        {
            var tempEnemy = Instantiate(enemy, placeHolder.position, transform.rotation);
        }
    }
    private void Drop()
    {
        var exp = Random.Range(0f, 1f);
        if (exp < 0.2f) GetWeapon();
        else if (exp < 0.4) GetOrb();
        else Summon();
    }

    public override void TakeDamage(float damage)
    {
        Drop();
        Leaves.Play();
        Shake();
        Instantiate(berrylessBush, transform.position, transform.rotation);
        Destroy(gameObject);
    }

}
