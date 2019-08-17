
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDoors : MonoBehaviour
{
    public GameObject player;
    private void OnTriggerEnter(Collider o)
    {
        if(o.gameObject.GetComponent<BarrierActivation>()!=null)
        {
            player.GetComponent<Dash>().canDash = false;
            player.GetComponent<PlayerController>().canMove = false;
            Shake.instance.shake = 0.3f;
            Shake.instance.shakeAmount = 0.7f;
            StartCoroutine(ActivateDoor(o.gameObject));
        }
    }

    IEnumerator ActivateDoor(GameObject go)
    {
        player.GetComponent<PlayerController>().ToIdle();
        yield return new WaitForSeconds(1);
        if(go != null)
        {
            go.GetComponent<BarrierActivation>().ActivateDoors();
            Destroy(go);
        }
        player.GetComponent<Dash>().canDash = true;
        player.GetComponent<PlayerController>().canMove = true;
    }
}
