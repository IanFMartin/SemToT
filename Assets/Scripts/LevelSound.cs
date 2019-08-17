using UnityEngine;

public class LevelSound : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play(1);
        }
    }
}
