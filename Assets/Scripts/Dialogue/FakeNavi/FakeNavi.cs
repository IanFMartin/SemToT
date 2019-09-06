using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeNavi : MonoBehaviour
{
    public Text fakeNaviText;
    //public Image fade;
    public GameObject player;
    public ParticleSystem fakeNavi;
    public Dialogue whatToSay;
    public DialogueManager DM;
    public Button[] naviButton;
    public int currentButton;

    //Fake Navi Talk
    public void FNTalk()
    {
        currentButton = 0;
        fakeNaviText.gameObject.SetActive(true);
        naviButton[0].gameObject.SetActive(true);
        //  fade.gameObject.SetActive(true);
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<ChangeWeapon>().enabled = false;
        fakeNavi.Play();
        DM.StartDialogue(whatToSay);
    }
    public void NextSentence()
    {
        naviButton[currentButton + 1].gameObject.SetActive(true);
        naviButton[currentButton].gameObject.SetActive(false);
        DM.DisplayNextSentence();
        currentButton += 1;
    }
    public void StopTalking()
    {
        DM.EndDialogue();
        foreach (var item in naviButton)
        {
        item.gameObject.SetActive(false);
        }
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<ChangeWeapon>().enabled = true;
        fakeNavi.Stop();
    }


}
