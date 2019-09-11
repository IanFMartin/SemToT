using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerEntryUI : MonoBehaviour
{

    public ParticleSystem ImplosionCurseParticle;
    public PlayerController _target;
    public Image curseClock;

    private void Start()
    {
       curseClock.fillAmount = 0;
        curseClock.gameObject.SetActive(false);
    }
    public void Cursing()
    {
        curseClock.gameObject.SetActive(true);
        Instantiate(ImplosionCurseParticle, _target.transform.position, _target.transform.rotation);
        _target.canMove = false;
        _target.ToIdle();
        StartCoroutine(CursingCorrutine());
    }
    IEnumerator CursingCorrutine()
    {
        yield return new WaitForSeconds(1);
        var watchdog = 0;
        while (curseClock.fillAmount < 1 && watchdog < 10000)
        {
            curseClock.fillAmount = Time.deltaTime * 80;
            watchdog++;
        }
        _target.canMove = true;


    }
}
