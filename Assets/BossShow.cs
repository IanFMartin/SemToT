using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossShow : MonoBehaviour
{
    public GameObject trigger;
    public Image bossHealth;
    public SkeletonBoss boss;
    public CameraControl cam;
    public GameObject target;


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            target = other.gameObject;
            target.GetComponent<PlayerController>().anim.Play("idle");
            other.GetComponent<PlayerController>().enabled = false;
            other.GetComponent<Dash>().enabled = false;
            other.GetComponent<Jump>().enabled = false;
            cam.targetTransform = boss.transform;
            StartCoroutine(MoveCamera());

        }
    }

    IEnumerator MoveCamera()
    {
        yield return new WaitForSeconds(1);
        boss.fsm.Feed(SkeletonBoss.OnCondition.Show);
        bossHealth.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        cam.targetTransform = target.transform;
        target.GetComponent<PlayerController>().enabled = true;
        target.GetComponent<Dash>().enabled = true;
        target.GetComponent<Jump>().enabled = true;
        boss.fsm.Feed(SkeletonBoss.OnCondition.Idle);

        Destroy(gameObject);
    }
}
