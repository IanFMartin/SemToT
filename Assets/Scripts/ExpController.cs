using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpController : MonoBehaviour
{
    public Image expBar;
    public Text txtLevel;
    public GameObject lvlUpSpell;
    public GameObject levlUpPanel;
    public AddNumberStat lifeStat;
    public AddNumberStat strStat;
    public AddNumberStat dexStat;
    private PlayerLife player;
    private AudioSource audioSource;
    public AudioClip lvlUpClip;

    public float startMaxExp;
    public float expFactor;
    private float maxExp;
    private float exp;
    private float timeToFade;

    private void Start()
    {
        if (GetComponent<AudioSource>() != null)
            audioSource = GetComponent<AudioSource>();
        player = GetComponent<UILife>().playerLife;
        exp = 0;
        maxExp = startMaxExp;
        InitialLevelUp(StaticData.level);
        exp = StaticData.exp;
        UpdateXpBar();
    }

    private void Update()
    {
        FadePanel();
        txtLevel.text = StaticData.level.ToString();

        if(Input.GetKey(KeyCode.Tab))
        {
            if(Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                LevelUp();
            }
        }
    }

    private void FadePanel()
    {
        if (levlUpPanel.activeInHierarchy && lifeStat.timer <= 0)
        {
            timeToFade += Time.deltaTime;
            if (timeToFade >= 3f)
            {
                levlUpPanel.SetActive(false);
            }
        }
    }

    public void GetExp(float exp)
    {
        this.exp += exp;
        StaticData.exp = this.exp;
        CheckLevelUp();
        UpdateXpBar();
    }

    private void InitialLevelUp(int level)
    {
        var changeweapon = player.GetComponent<ChangeWeapon>();
        for (int i = 0; i < level; i++)
        {
            lifeStat.stat += 10;
            strStat.stat += 10;
            dexStat.stat += 10;

            player.UpdateMaxLife(player.maxLife + 10 * 25f);
            changeweapon.str += 5;
            changeweapon.dex += 5;
            maxExp *= expFactor;
        }
    }

    public void LevelUp()
    {
        GetExp(maxExp);
    }

    public void CheckLevelUp()
    {
        if (exp >= maxExp)
        {
            if(audioSource != null && lvlUpClip != null)
            {
                audioSource.clip = lvlUpClip;
                audioSource.Play();
            }
            float tempExp = exp - maxExp;
            exp = tempExp;
            maxExp *= expFactor;

            Instantiate(lvlUpSpell, player.transform.position, Quaternion.identity);

            levlUpPanel.SetActive(true);
            timeToFade = 0;

            player.UpdateMaxLife(player.maxLife + 10 * 100f);
            player.GetComponent<ChangeWeapon>().str += 10;
            player.GetComponent<ChangeWeapon>().dex += 10;
            StaticData.level++;

            lifeStat.statToAdd += 10;
            strStat.statToAdd += 10;
            dexStat.statToAdd += 10;

            Shake.instance.shake = 0.1f;
            Shake.instance.shakeAmount = 0.5f;
        }
    }

    private void UpdateXpBar()
    {
        expBar.fillAmount = exp / maxExp;
    }
}
