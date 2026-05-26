using UnityEngine;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("[ 매니저 참조 ]")]
    public GameManager gameManager;
    public BoardManager boardManager;

    [Header("[ 인트로 UI 요소 ]")]
    public GameObject introPanel;              
    public TextMeshProUGUI countdownText;      
    public CanvasGroup startButtonCanvasGroup; 

    private bool isStartClicked = false;

    private void Start()
    {
        if (countdownText != null) countdownText.text = "";
        if (introPanel != null) introPanel.SetActive(true);

        if (gameManager != null && boardManager != null && gameManager.allStages.Length > 0)
        {
            StageData introPreviewStage = gameManager.allStages[0];
            boardManager.SetupBoard(introPreviewStage);
            boardManager.SpawnCharacters(introPreviewStage);
        }
    }

    private void Update()
    {
        if (!isStartClicked && startButtonCanvasGroup != null)
        {
            startButtonCanvasGroup.alpha = Mathf.PingPong(Time.time * 1.5f, 0.8f) + 0.2f;
        }
    }

    public void ClickStartGameButton()
    {
        if (isStartClicked) return;
        isStartClicked = true;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        StartCoroutine(IntroSequenceRoutine());
    }

    private IEnumerator IntroSequenceRoutine()
    {
        // 🎯 [핵심 패치] 부모인 introPanel이 꺼져도 글자가 보이도록 countdownText 오브젝트만 쏙 켜둡니다.
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        if (introPanel != null) introPanel.SetActive(false);

        // 프리뷰 캐릭터 즉시 청소
        CharacterPiece[] activePieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (CharacterPiece piece in activePieces)
        {
            if (piece != null && piece.gameObject != null) Destroy(piece.gameObject);
        }

        if (boardManager != null)
        {
            if (boardManager.mainLines != null)
            {
                foreach (var line in boardManager.mainLines)
                {
                    if (line != null && line.slots != null)
                    {
                        foreach (var slot in line.slots) if (slot != null) slot.ClearSlot();
                    }
                }
            }
            if (boardManager.waitingLine != null && boardManager.waitingLine.slots != null)
            {
                foreach (var slot in boardManager.waitingLine.slots) if (slot != null) slot.ClearSlot();
            }
        }

        // 메모리가 완전히 비워질 때까지 리얼 타임 대기 (0.3초 숨고르기)
        yield return new WaitForSecondsRealtime(0.3f);

        // 🎯 텅 빈 보드판 상태에서 무조건 3, 2, 1 카운트다운을 화면에 강제 출력합니다!
        if (countdownText != null)
        {
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1.0f);
            }
            countdownText.text = "GO!";
        }

        // 'GO!' 지점에서 캐릭터가 스폰되도록 바통 터치
        if (gameManager != null)
        {
            gameManager.LoadStageDirectFromIntro(0); 
        }

        yield return new WaitForSecondsRealtime(0.8f);
        if (countdownText != null) countdownText.text = "";
        
        Destroy(gameObject);
    }
}