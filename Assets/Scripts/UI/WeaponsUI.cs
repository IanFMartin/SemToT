using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    private Weapon weapon1;
    private Weapon weapon2;
    private Weapon currentWeapon;

    public Sprite stickSprite;
    public Sprite bowSprite;
    public Sprite swordSprite;
    public Sprite staffSprite;
    public Sprite staffElectricSprite;
    public Sprite daggerSprite;

    public Image slot1;
    public Image slot2;
    public GameObject enabled1;
    public GameObject enabled2;

    public void ReceiveWeapons(Weapon w1, Weapon w2, Weapon cW)
    {
        weapon1 = w1;
        weapon2 = w2;
        currentWeapon = cW;
        if (weapon1 != null)
            slot1.sprite = GetWeaponSprite(w1);
        if (weapon2 != null)
            slot2.sprite = GetWeaponSprite(w2);

        if (currentWeapon == weapon1)
        {
            enabled1.SetActive(true);
            enabled2.SetActive(false);
        }
        if (currentWeapon == weapon2)
        {
            enabled1.SetActive(false);
            enabled2.SetActive(true);
        }
    }

    private Sprite GetWeaponSprite(Weapon w)
    {
        if (w.iD == 1)
            return stickSprite;
        if (w.iD == 2)
            return bowSprite;
        if (w.iD == 3)
            return swordSprite;
        if (w.iD == 4)
            return staffSprite;
        if (w.iD == 5)
            return staffElectricSprite;
        if (w.iD == 6)
            return daggerSprite;
        else
            return null;
    }
}
