using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("[ Audio Sources ]")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("[ Audio Clips ]")]
    public AudioClip bgmClip;
    public AudioClip clickClip;
    public AudioClip startClip;
    public AudioClip clearClip;
    public AudioClip failClip;

    [Header("[ Debug ]")]
    public bool showMissingAudioWarnings = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null)
        {
            if (showMissingAudioWarnings)
            {
                Debug.LogWarning("[SoundManager] BGM source or clip is not assigned.");
            }

            return;
        }

        if (bgmSource.isPlaying && bgmSource.clip == bgmClip)
        {
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            if (showMissingAudioWarnings)
            {
                Debug.LogWarning("[SoundManager] SFX source or clip is not assigned.");
            }

            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
