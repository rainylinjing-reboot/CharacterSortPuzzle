using UnityEngine;
using UnityEngine.Serialization;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("[ Audio Sources ]")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("[ BGM Clips ]")]
    [FormerlySerializedAs("bgmClip")]
    public AudioClip introBgmClip;
    public AudioClip stage1BgmClip;
    public AudioClip stage2BgmClip;
    public AudioClip rankingBgmClip;

    [Header("[ SFX Clips ]")]
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
            EnsureAudioSources();
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        PlayIntroBGM();
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmSource == null)
        {
            bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null || sfxSource == bgmSource)
        {
            foreach (AudioSource source in sources)
            {
                if (source != null && source != bgmSource)
                {
                    sfxSource = source;
                    break;
                }
            }

            if (sfxSource == null || sfxSource == bgmSource)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    public void PlayIntroBGM()
    {
        PlayBGM(introBgmClip);
    }

    public void PlayStageBGM(int stageNumber)
    {
        AudioClip clip = ResolveStageBgmClip(stageNumber);
        PlayBGM(clip);
    }

    public void PlayRankingBGM()
    {
        PlayBGM(rankingBgmClip, true);
    }

    public void PlayBGM()
    {
        PlayIntroBGM();
    }

    private AudioClip ResolveStageBgmClip(int stageNumber)
    {
        if (stageNumber <= 1)
        {
            return stage1BgmClip != null ? stage1BgmClip : introBgmClip;
        }

        if (stageNumber == 2)
        {
            return stage2BgmClip != null ? stage2BgmClip : stage1BgmClip;
        }

        return stage2BgmClip != null ? stage2BgmClip : stage1BgmClip;
    }

    private void PlayBGM(AudioClip clip, bool stopIfMissing = false)
    {
        EnsureAudioSources();

        if (bgmSource == null || clip == null)
        {
            if (stopIfMissing)
            {
                StopBGM();
            }

            if (showMissingAudioWarnings)
            {
                Debug.LogWarning("[SoundManager] BGM source or clip is not assigned.");
            }

            return;
        }

        if (bgmSource.isPlaying && bgmSource.clip == clip)
        {
            return;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
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
        EnsureAudioSources();

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
