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
    private PlayerModel player;
    private AudioSource audioSource;
    public AudioClip lvlUpClip;

    public float startMaxExp;
    public float expFactor;
    private float maxExp;
    private float exp;
    private float timeToFade;

    //texto q aprace cuando agarras la exp
    public Text expGainText;
    //contador de la corrutina
    private float howMuchExp;

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


        expGainText.gameObject.SetActive(false);
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
        ///expGainText.gameObject.SetActive(true);
        ///expGainText.text = "+ " + exp.ToString() + " EXP";
        //esto lo uso para el contador de la corrutina para que vaya reduciendose el numero en el texto
        howMuchExp += exp;
        this.exp += exp;
        StaticData.exp = this.exp;
        CheckLevelUp();
        UpdateXpBar();
        StopAllCoroutines();
        StartCoroutine(TextExp());
        ///StartCoroutine(StopExpText());
    }
    //corrutina para el texto de la experiencia.
    IEnumerator TextExp()
    {
        expGainText.gameObject.SetActive(true);
        while(howMuchExp>0)
        {
            expGainText.text = " + " + (int)howMuchExp + " EXP";
            howMuchExp -= Time.deltaTime*40;
            yield return null;
        }
        expGainText.gameObject.SetActive(false);
    }
    //IEnumerator StopExpText()
    //{
    //    yield return new WaitForSeconds(1f);
    //    expGainText.gameObject.SetActive(false);
    //}

    private void InitialLevelUp(int level)
    {
        var changeweapon = player;
        for (int i = 0; i < level; i++)
        {
            lifeStat.stat += 10;
            strStat.stat += 10;
            dexStat.stat += 10;

            //player.UpdateMaxLife(player.maxLife + 10 * 25f);
            changeweapon.str += 5;
            changeweapon.dex += 5;
            maxExp *= expFactor;
        }
    }

    public void LevelUp()
    {
        GetExp(maxExp);
        howMuchExp = 0;
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

            //player.UpdateMaxLife(player.maxLife + 10 * 25f);
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
