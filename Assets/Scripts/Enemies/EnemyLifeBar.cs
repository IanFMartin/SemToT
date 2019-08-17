using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyLifeBar : MonoBehaviour
{
    public Image healthBar;
    private Enemy myBehaviour;
    private float maxLife;

    private void Start()
    {
        myBehaviour = GetComponentInParent<Enemy>();
        maxLife = myBehaviour.maxLife;
    }

    private void Update()
    {
        healthBar.fillAmount = myBehaviour.life / maxLife;

        if (myBehaviour.life <= 0)
            healthBar.GetComponentInParent<Canvas>().gameObject.SetActive(false);
    }
}
