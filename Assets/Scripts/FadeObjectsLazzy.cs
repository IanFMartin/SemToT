using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FadeObjectsLazzy : MonoBehaviour
{
    public Transform target;
    public LayerMask obstaclesLayer;
    public Shader transpShader;
    private GameObject savedGO;
    private Shader savedShader;
    private bool shaderSaved;
    public float radius;
    private HashSet<Collider> transpColliders = new HashSet<Collider>();
    private HashSet<Collider> normalColliders = new HashSet<Collider>();
    private List<Collider> colliders = new List<Collider>();

    void Update()
    {
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(transform.position, radius, target.transform.position - transform.position - Vector3.up / 4, Mathf.Infinity, obstaclesLayer);
        //savedColliders = hits.Select(x => x.collider).ToList();
        foreach (var hit in hits)
        {
            colliders = hits.Select(x => x.collider).ToList();

            foreach (var item in transpColliders)
            {
                if (!colliders.Contains(item))
                {
                    if (item.GetComponent<MeshRenderer>())
                    {
                        var allMats = item.GetComponent<MeshRenderer>().materials;
                        for (int i = 0; i < allMats.Length; i++)
                        {
                            allMats[i].shader = savedShader;
                        }
                        normalColliders.Add(item);
                    }
                    if (item.GetComponentInChildren<MeshRenderer>())
                    {
                        var allMats = item.GetComponentInChildren<MeshRenderer>().materials;
                        for (int i = 0; i < allMats.Length; i++)
                        {
                            allMats[i].shader = savedShader;
                        }
                        normalColliders.Add(item);
                    }
                }
            }

            ///////////////////////////////

            if (hit.collider.gameObject.layer == 13 && hit.collider.GetComponentInChildren<MeshRenderer>())
            {
                var allMats = hit.collider.GetComponentInChildren<MeshRenderer>().materials;
                for (int i = 0; i < allMats.Length; i++)
                {
                    if (!transpColliders.Contains(hit.collider))
                    {
                        savedShader = allMats[i].shader;
                        transpColliders.Add(hit.collider);
                        if (normalColliders.Contains(hit.collider))
                            normalColliders.Remove(hit.collider);
                    }
                    allMats[i].shader = transpShader;
                }
            }
            if (hit.collider.gameObject.layer == 13 && hit.collider.GetComponent<MeshRenderer>())
            {
                var allMats = hit.collider.GetComponent<MeshRenderer>().materials;
                for (int i = 0; i < allMats.Length; i++)
                {
                    if (!transpColliders.Contains(hit.collider))
                    {
                        savedShader = allMats[i].shader;
                        transpColliders.Add(hit.collider);
                        if (normalColliders.Contains(hit.collider))
                            normalColliders.Remove(hit.collider);
                    }
                    allMats[i].shader = transpShader;
                }
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, target.transform.position - transform.position);
    }
}
