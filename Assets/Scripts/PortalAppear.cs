using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalAppear : MonoBehaviour
{
    public GameObject portal;

    public void SummonPortal()
    {
        portal.SetActive(true);
    }
}
