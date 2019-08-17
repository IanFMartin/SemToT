using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cheats : MonoBehaviour
{
    public List<Weapon> weapons;

    private void Update()
    {
        if(Input.GetKey(KeyCode.Tab))
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                GetComponent<PlayerLife>().life = GetComponent<PlayerLife>().maxLife;
                GetComponent<PlayerLife>().lifeEvent.Invoke(GetComponent<PlayerLife>().life);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Instantiate(weapons[0].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Instantiate(weapons[1].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Instantiate(weapons[2].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Instantiate(weapons[3].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Instantiate(weapons[4].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Instantiate(weapons[5].gameObject, transform.position, transform.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Instantiate(weapons[6].gameObject, transform.position, transform.rotation);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                SceneManager.LoadScene(0);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                SceneManager.LoadScene(1);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                SceneManager.LoadScene(2);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                SceneManager.LoadScene(3);
            }

        }
    }
}
