using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControl : MonoBehaviour
{
    public Transform targetTransform;
    public Vector3 offset;
    public int distanceToPlayer;
    public float timeToFollow;

    private void Awake()
    {
        WindowsHelperAccessibilityKeys.AllowAccessibilityShortcutKeys(false);
    }

    private void Update()
    {
        timeToFollow -= Time.deltaTime;
        if (targetTransform != null && timeToFollow <= 0)
        {
            Vector3 expectedPosition = targetTransform.position + offset;
            if (expectedPosition.magnitude < distanceToPlayer)
                transform.position = Vector3.Lerp(transform.position, expectedPosition, 0.02f);
            else if (expectedPosition.y > distanceToPlayer)
                transform.position = Vector3.Lerp(transform.position, expectedPosition, 0.1f);
            else
                transform.position = Vector3.Lerp(transform.position, expectedPosition, 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.L)) SceneManager.LoadScene(3);
    }
}
