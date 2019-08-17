using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILife : MonoBehaviour
{
    public PlayerLife playerLife;
    public Image topHealthBar;
    public Image curseClock;
    public Text txtClock;

    public void ShowLife(float life)
    {
        topHealthBar.fillAmount = playerLife.life / playerLife.maxLife;

        /*
        if (healthBar.fillAmount >= 0.66f)
            healthBar.color = Color.green;
        else if (healthBar.fillAmount >= 0.33f)
            healthBar.color = Color.yellow;
        else if (healthBar.fillAmount >= 0f)
             healthBar.color = Color.red;
        */
    }
    public void CurseTimeUpdate()
    {
        var curse = playerLife.curseTime;
        curseClock.fillAmount = curse / playerLife.maxCurseTime;
        float segs = curse % 60;
        int minutes = Mathf.RoundToInt((curse - segs) / 60);
        string txtSegs = segs.ToString();
        if (segs < 10)
            txtSegs = 0.ToString() + segs.ToString();
        txtClock.text = minutes.ToString() + ":" + txtSegs;
    }
}

[System.Serializable]
public class MyFloatEvent : UnityEvent<float>
{
}
