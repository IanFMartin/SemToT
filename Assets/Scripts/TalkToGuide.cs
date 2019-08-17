using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkToGuide : MonoBehaviour
{
    public Guide guide;
    public Image interactionImg;
    public int rangeOfTalking;
    private bool _canTalk = true;

    void Update()
    {
        if (guide != null)
        {
            var distanceToTarget = Vector3.Distance(transform.position, guide.transform.position);
            if (distanceToTarget < rangeOfTalking)
            {
                if (_canTalk == true)
                    interactionImg.gameObject.SetActive(true);
                else
                    interactionImg.gameObject.SetActive(false);
                if (Input.GetKeyDown(KeyCode.E) && _canTalk)
                {
                    guide.Talking();
                    _canTalk = false;
                }
            }
            else
                interactionImg.gameObject.SetActive(false);
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
