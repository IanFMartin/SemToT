using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillHUDController : MonoBehaviour
{
    public Image cooldownImg;
    public Image habilityReady;
    private float habilityCD;
    
    public void SetHabilityCD(float maxCD)
    {
        habilityCD = maxCD;
    }

    public void SetCooldown(float cd)
    {
        cooldownImg.fillAmount = cd / habilityCD;

        if(cd == habilityCD)
            habilityReady.gameObject.SetActive(true);
        else
            habilityReady.gameObject.SetActive(false);
    }
}
