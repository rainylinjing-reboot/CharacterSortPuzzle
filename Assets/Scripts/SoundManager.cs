using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 어디서나 편하게 사운드를 호출할 수 있도록 싱글톤(Singleton) 구조를 만듭니다.
    public static SoundManager Instance { get; private set; }

    [Header("[ 오디오 소스 등록 ]")]
    public AudioSource bgmSource;       // 배경음악 전용 스피커
    public AudioSource sfxSource;       // 효과음 전용 스피커

    [Header("[ 사운드 음원 파일(Clip) 등록 ]")]
    public AudioClip bgmClip;           // 기본 배경 음악
    public AudioClip clickClip;         // 캐릭터/슬롯 마우스 클릭 사운드
    public AudioClip startClip;         // 게임 카운트다운/시작 사운드
    public AudioClip clearClip;         // 스테이지 성공 사운드
    public AudioClip failClip;          // 게임 오버 / 실패 사운드

    private void Awake()
    {
        // 싱글톤 초기화 및 방어 코드
        if (Instance == null)
        {
            Instance = this;
            // 씬이 넘어가도 사운드가 끊기지 않고 유지되도록 설정 (필요시)
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 🎮 게임 시작 시 자동으로 배경음악 재생
        PlayBGM();
    }

    // 🎵 배경 음악 재생 (방어 코드 탑재)
    public void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null)
        {
            // 오디오 소스나 파일이 비어있어도 에러를 내지 않고 조용히 리턴합니다!
            Debug.LogWarning("💡 [사운드 가이드] bgmSource 또는 bgmClip이 비어 있어 배경음이 재생되지 않습니다.");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = true; // 배경음은 무한 반복
        bgmSource.Play();
    }

    // 🔊 배경 음악 정지
    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    // 🎯 효과음 단발성 재생 핵심 함수 (방어 코드 탑재)
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            // 슬롯에 사운드 파일을 안 넣었어도 에러 없이 부드럽게 우회합니다.
            return; 
        }

        // PlayOneShot은 여러 효과음이 겹쳐도 끊기지 않고 동시에 이쁘게 출력해 줍니다.
        sfxSource.PlayOneShot(clip);
    }
}