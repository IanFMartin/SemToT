using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public PlayerLife playerLife;
    public GameObject deadSoul;
    public GameObject deadParticle;
    public Animator anim;

    private void Start()
    {
        StartCoroutine("PlayerSpawn");
    }

    IEnumerator PlayerSpawn()
    {
        playerLife.gameObject.SetActive(false);
        yield return new WaitForSeconds(1 + Camera.main.GetComponent<CameraControl>().timeToFollow * 2.5f);
        var a = Instantiate(deadSoul, playerLife.transform.position + Vector3.up, Quaternion.identity);
        yield return new WaitForSeconds(1);
        playerLife.gameObject.SetActive(true);
        Destroy(a);
    }

    public void Respawn()
    {
        StartCoroutine("PlayerRespawn");
    }

    IEnumerator PlayerRespawn()
    {
        Shake.instance.shake = 0.1f;
        Shake.instance.shakeAmount = 0.1f;
        yield return new WaitForSeconds(0.2f);
        Instantiate(deadSoul, playerLife.transform.position + Vector3.up, Quaternion.identity);
        Instantiate(deadParticle, playerLife.transform.position + Vector3.up, Quaternion.identity);
        yield return new WaitForSeconds(3);
        anim.SetTrigger("FadeOutTrigger");
    }

}
