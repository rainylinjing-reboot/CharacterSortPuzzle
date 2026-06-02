using UnityEngine;

public class LuckyRunSoundManager : MonoBehaviour
{
    public static LuckyRunSoundManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmAudioSource;
    public AudioSource runAudioSource;
    public AudioSource sfxAudioSource;

    [Header("BGM")]
    public AudioClip bgmClip;
    public float bgmVolume = 0.35f;

    [Header("Run")]
    public AudioClip runClip;
    public float runVolume = 0.18f;

    [Header("SFX")]
    public AudioClip doorOpenClip;
    public AudioClip hitClip;
    public AudioClip retryClip;

    public float doorOpenVolume = 0.8f;
    public float hitVolume = 1.0f;
    public float retryVolume = 0.7f;

    [Header("Setting")]
    public bool playBgmOnStart = true;
    public bool playRunSoundOnStart = true;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetupAudioSources();

        if (playBgmOnStart == true)
        {
            PlayBGM();
        }

        if (playRunSoundOnStart == true)
        {
            PlayRunSound();
        }
    }

    void SetupAudioSources()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.clip = bgmClip;
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.volume = bgmVolume;
        }

        if (runAudioSource != null)
        {
            runAudioSource.clip = runClip;
            runAudioSource.loop = true;
            runAudioSource.playOnAwake = false;
            runAudioSource.volume = runVolume;
        }

        if (sfxAudioSource != null)
        {
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }
    }

    public void PlayBGM()
    {
        if (bgmAudioSource == null || bgmClip == null)
            return;

        if (bgmAudioSource.isPlaying == true)
            return;

        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    public void StopBGM()
    {
        if (bgmAudioSource == null)
            return;

        bgmAudioSource.Stop();
    }

    public void PlayRunSound()
    {
        if (runAudioSource == null || runClip == null)
            return;

        if (runAudioSource.isPlaying == true)
            return;

        runAudioSource.clip = runClip;
        runAudioSource.volume = runVolume;
        runAudioSource.loop = true;
        runAudioSource.Play();
    }

    public void StopRunSound()
    {
        if (runAudioSource == null)
            return;

        runAudioSource.Stop();
    }

    public void PlayDoorOpenSound()
    {
        PlayOneShot(doorOpenClip, doorOpenVolume);
    }

    public void PlayHitSound()
    {
        PlayOneShot(hitClip, hitVolume);
    }

    public void PlayRetrySound()
    {
        PlayOneShot(retryClip, retryVolume);
    }

    void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfxAudioSource == null || clip == null)
            return;

        sfxAudioSource.PlayOneShot(clip, volume);
    }
}