using UnityEngine;
using TMPro; // TextMeshProUGUI 사용을 위해 필수

public class UIManager : MonoBehaviour
{
    [Header("[ TMP 텍스트 컴포넌트 연결 ]")]
    public TextMeshProUGUI stageText;       // 상단 스테이지 표시용 Text (예: STAGE 1)
    public TextMeshProUGUI timerText;       // 남은 시간 표시용 Text (예: TIME: 90.0s)
    public TextMeshProUGUI resultStatusText;// 게임 오버 / 클리어 팝업 텍스트

    [Header("[ 1단계: 항복 팝업 패널 ]")]
    public GameObject giveUpPopupPanel;     // 평소엔 꺼져 있을 항복 경고창 패널 오브젝트

    private void Awake()
    {
        // 게임 시작 시 결과창 텍스트는 깨끗하게 비워둡니다.
        if (resultStatusText != null)
        {
            resultStatusText.text = "";
        }

        // 시작할 때 혹시 켜져 있을지 모를 항복 팝업창을 안전하게 끄고 시작합니다.
        SetGiveUpPopupActive(false);
    }

    /// <summary>
    /// 상단 스테이지 텍스트를 업데이트합니다.
    /// </summary>
    public void UpdateStageText(int stageNum)
    {
        if (stageText != null)
        {
            stageText.text = $"STAGE {stageNum}";
        }
    }

    /// <summary>
    /// 남은 시간 텍스트를 소수점 첫째 자리까지 포맷팅하여 업데이트합니다.
    /// </summary>
    public void UpdateTimerText(float time)
    {
        if (timerText != null)
        {
            timerText.text = $"TIME: {time:F1}s";
        }
    }

    /// <summary>
    /// 화면 중앙에 게임 결과(STAGE CLEAR / GAME OVER 등)를 띄웁니다.
    /// </summary>
    public void ShowResultText(string message)
    {
        if (resultStatusText != null)
        {
            resultStatusText.text = message;
        }
    }

    /// <summary>
    /// 🚨 [1단계 추가] 항복 팝업창(패널)을 껐다 켜는 제어 함수입니다.
    /// </summary>
    public void SetGiveUpPopupActive(bool isActive)
    {
        if (giveUpPopupPanel != null)
        {
            giveUpPopupPanel.SetActive(isActive);
        }
    }
}