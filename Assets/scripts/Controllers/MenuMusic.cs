using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip menuMusic;

    void Awake()
    {
        if (FindObjectsOfType<MenuMusic>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        audioSource.clip = menuMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}
