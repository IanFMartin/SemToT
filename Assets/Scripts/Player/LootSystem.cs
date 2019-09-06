using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public PlayerModel player;
    internal List<Collider> grabbables = new List<Collider>();
    public Canvas grabCanvas;
    private Transform posGrab;

    private void Start()
    {
        player = FindObjectOfType<PlayerModel>();
    }

    private void Update()
    {
        if (player != null && player.gameObject.activeInHierarchy)
        {
            transform.position = player.transform.position;
            Grab();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ILooteable>() != null)
        {
            other.gameObject.GetComponent<Item>().Loot(player.transform);
        }
    }

    public void Grab()
    {
        var grabObject = GetCloserGrabbable();
        if (grabObject != null)
        {
            grabCanvas.transform.position = posGrab.position + Vector3.up/2;
            grabCanvas.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                grabObject.Grab(this);
            }
        }
        else
            grabCanvas.gameObject.SetActive(false);
    }

    public IGrabbable GetCloserGrabbable()
    {
        grabbables.Clear();
        grabbables.AddRange(Physics.OverlapSphere(transform.position, 3));
        if (grabbables.Where(x => x.GetComponent<Weapon>()).Any())
        {
            grabbables = grabbables.Where(x => x.GetComponent<Weapon>())
                                                .Where(x => x.GetComponent<Weapon>().dropped == true)
                                                    .ToList();
        }
        grabbables = grabbables.Where(x => x.GetComponent<IGrabbable>() != null).ToList();
        if (grabbables.Count > 0)
        {
            var grabObj = grabbables.OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
                                .First();
            posGrab = grabObj.transform;
            return grabObj.GetComponent<IGrabbable>();
        }
        else
            return null;
    }
}