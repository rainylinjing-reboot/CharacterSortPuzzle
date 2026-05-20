using UnityEngine;
using TMPro; // TextMeshPro 사용 필수

public class UIManager : MonoBehaviour
{
    [Header("[ TMP 텍스트 컴포넌트 연결 ]")]
    public TextMeshProUGUI stageText;       // 상단 스테이지 표시용 Text
    public TextMeshProUGUI timerText;       // 남은 시간 표시용 Text
    public TextMeshProUGUI resultStatusText;// 게임 오버 / 클리어 팝업용 Text

    private void Awake()
    {
        // 게임 시작 시 결과창 텍스트 비워두기
        if (resultStatusText != null)
            resultStatusText.text = "";
    }

    public void UpdateStageText(int stageNum)
    {
        if (stageText != null)
            stageText.text = $"STAGE {stageNum}";
    }

    public void UpdateTimerText(float time)
    {
        if (timerText != null)
        {
            // 소수점 첫째 자리까지만 포맷팅하여 노출 (예: 90.0s)
            timerText.text = $"TIME: {time:F1}s";
        }
    }

    public void ShowResultText(string message)
    {
        if (resultStatusText != null)
        {
            resultStatusText.text = message;
        }
    }
}