using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private GameObject player;
    public GameObject center;
    public Animator fadeAnim;
    private Vector3 gravity;
    private bool isTouching;
    private ChangeWeapon cH;
    private void Start()
    {
        cH = FindObjectOfType<ChangeWeapon>();
    }

    void Update()
    {
        if (isTouching)
        {
            MoveToCenter();
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.GetComponent<PlayerController>() != null)
        {
            var pc = col.gameObject.GetComponent<PlayerController>();
            col.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            player = pc.gameObject;
            pc.canMove = false;
            pc.anim.SetFloat("InputH", 0);
            pc.anim.SetFloat("InputV", 0);
            isTouching = true;

            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                if (pc.GetComponent<ChangeWeapon>().currentWeapon == pc.GetComponent<ChangeWeapon>().weapon1)
                    WeaponTable.WeaponTableSingleton.currentWeaponSaved = 1;
                if (pc.GetComponent<ChangeWeapon>().currentWeapon == pc.GetComponent<ChangeWeapon>().weapon2)
                    WeaponTable.WeaponTableSingleton.currentWeaponSaved = 2;
                if (pc.GetComponent<ChangeWeapon>().weapon1 != null)
                    WeaponTable.WeaponTableSingleton.weapon1ID = pc.GetComponent<ChangeWeapon>().weapon1.iD;
                if (pc.GetComponent<ChangeWeapon>().weapon2 != null)
                    WeaponTable.WeaponTableSingleton.weapon2ID = pc.GetComponent<ChangeWeapon>().weapon2.iD;
            }

            StartCoroutine(EndLevelTimer());
        }
    }

    private void MoveToCenter()
    {
        player.transform.position = Vector3.Lerp(player.transform.position, center.transform.position, 0.01f + Time.deltaTime);

    }
    IEnumerator EndLevelTimer()
    {
        yield return new WaitForSeconds(1f);
        fadeAnim.SetTrigger("FadeOutTrigger");
    }
}
