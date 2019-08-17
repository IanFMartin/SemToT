using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillNotReady : MonoBehaviour
{
    public Text cantDoTxt;
    private float alpha;
    private float timeToOff = 1f;
    private float showingTime;

    public void Update()
    {
        if (showingTime > 0)
            ExecuteShow();
        else
            EndShow();
    }

    private void EndShow()
    {
        showingTime = 0f;
        if (alpha > 0)
            alpha -= Time.deltaTime / 2;
        cantDoTxt.color = new Color(cantDoTxt.color.r, cantDoTxt.color.g, cantDoTxt.color.b, alpha);
    }

    private void ExecuteShow()
    {
        showingTime -= Time.deltaTime;
        if (alpha < 1)
            alpha += Time.deltaTime;
        cantDoTxt.color = new Color(cantDoTxt.color.r, cantDoTxt.color.g, cantDoTxt.color.b, alpha);
    }

    public void ShowText()
    {
        showingTime = timeToOff;
    }
}
