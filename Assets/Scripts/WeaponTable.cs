using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponTable : MonoBehaviour
{
    public static WeaponTable WeaponTableSingleton = null;
    public List<int> Weaponid;
    public List<GameObject> WeaponModel;
    internal List<int> weaponiDSpawn = new List<int>();
    internal List<GameObject> weaponModelSpawn = new List<GameObject>();
    public Dictionary<int, GameObject> dicWeaponPrefab = new Dictionary<int, GameObject>();
    public Dictionary<int, float> DicToUseInDrop = new Dictionary<int, float>();

    internal int weapon1ID = -1;
    internal int weapon2ID = -1;
    internal int currentWeaponSaved;

    private void Awake()
    {
        if (WeaponTableSingleton == null)
            WeaponTableSingleton = this;
        else if (WeaponTableSingleton != this)
            Destroy(gameObject);
    }

    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void UpdateWeaponList()
    {
        dicWeaponPrefab.Clear();
        DicToUseInDrop.Clear();
        for (int i = 0; i < weaponiDSpawn.Count; i++)
        {
           dicWeaponPrefab.Add(weaponiDSpawn[i], weaponModelSpawn[i]);
            DicToUseInDrop.Add(weaponiDSpawn[i], 10);
        }
    }

    public GameObject GetSavedWeapon(int id)
    {
        if (id == -1)
            return null;

        for (int i = 0; i < WeaponModel.Count; i++)
        {
            if (WeaponModel[i].GetComponent<Weapon>().iD == id)
                return WeaponModel[i];
        }

        return null;
    }
}
