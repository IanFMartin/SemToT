using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkToGuide : MonoBehaviour
{
    public Guide guide;
    public Image interactionImg;
    public int rangeOfTalking;
    private DialogueManager DM;
    private void Start()
    {
        DM = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (guide != null)
        {
            var distanceToTarget = Vector3.Distance(transform.position, guide.transform.position);
            if (distanceToTarget < rangeOfTalking)
            {
                if (!DM.IsTalking)
                {
                    interactionImg.gameObject.SetActive(true);
                    GetComponent<ChangeWeapon>().enabled = true;
                }
                else
                {
                    interactionImg.gameObject.SetActive(false);
                    GetComponent<ChangeWeapon>().enabled = false;
                }
                if (Input.GetKeyDown(KeyCode.E) && !DM.IsTalking)
                {
                    guide.Talking();
                }
            }
            else
            {
                interactionImg.gameObject.SetActive(false);
                DM.EndDialogue();
            }
        }
        else
            TalkToGuideNow();

    }

    private void TalkToGuideNow()
    {
        var listGuide = new List<Collider>();
        listGuide.Clear();
        listGuide.AddRange(Physics.OverlapSphere(transform.position, rangeOfTalking));
        foreach (var item in listGuide)
        {
            if (item.GetComponent<Guide>())
                guide = item.GetComponent<Guide>();
        }

        /*
        if (listGuide.Count > 0 && listGuide.gameObject.GetComponent<Guide>())
        {
            guide = listGuide.
            guide = listGuide[0].gameObject.GetComponent<Guide>();
        }
                else
        {
            guide = null;
        }
        */
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangeOfTalking);
    }
}
