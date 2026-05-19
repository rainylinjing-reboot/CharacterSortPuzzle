using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("[ 매니저 참조 ]")]
    public BoardManager boardManager;
    public UIManager uiManager;

    [Header("[ 데이터 설정 ]")]
    public StageData currentStageData; // 현재 실행할 스테이지 데이터

    private float timeRemaining;
    private bool isGameActive = false;

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        if (currentStageData == null || boardManager == null || uiManager == null)
        {
            Debug.LogError("GameManager에 필수 오브젝트들이 연결되지 않았습니다.");
            return;
        }

        // 1차 단계: 보드 레이아웃 활성화
        boardManager.SetupBoard(currentStageData);

        // 타이머 및 UI 초기화
        timeRemaining = currentStageData.timeLimit;
        uiManager.UpdateStageText(currentStageData.stageNumber);
        uiManager.UpdateTimerText(timeRemaining);

        isGameActive = true;
    }

    private void Update()
    {
        if (!isGameActive) return;

        // 타이머 차감 계산
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            uiManager.UpdateTimerText(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            uiManager.UpdateTimerText(timeRemaining);
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        uiManager.ShowResultText("GAME OVER");
        Debug.Log("제한 시간 초과: 게임 오버");
    }
}