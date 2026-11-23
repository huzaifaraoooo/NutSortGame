using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip moveSound;
    public AudioClip winSound;
    public AudioClip selectSound; // Naya Select Sound

    void Awake()
    {
        instance = this;
    }

    public void PlayMoveSound()
    {
        if (sfxSource != null && moveSound != null)
        {
            sfxSource.PlayOneShot(moveSound);
        }
    }

    public void PlayWinSound()
    {
        if (sfxSource != null && winSound != null)
        {
            sfxSource.PlayOneShot(winSound);
        }
    }

    public void PlaySelectSound()
    {
        if (sfxSource != null && selectSound != null)
        {
            sfxSource.PlayOneShot(selectSound);
        }
    }
}