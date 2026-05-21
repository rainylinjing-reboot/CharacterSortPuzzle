using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("[ TMP 텍스트 컴포넌트 연결 ]")]
    public TextMeshProUGUI stageText;       
    public TextMeshProUGUI timerText;       
    public TextMeshProUGUI resultStatusText;

    [Header("[ 1단계: 항복 팝업 패널 ]")]
    public GameObject giveUpPopupPanel;     

    [Header("[ 3단계: 명예의 전당 UI 패널 ]")]
    public GameObject leaderboardPanel;     // 랭킹창 팝업 전체 부모
    public TextMeshProUGUI titleText;       // 💡 [신규] 겹침 방지를 위해 타이틀 텍스트 컴포넌트 연결 추가!
    public TMP_InputField nameInputField;   // 닉네임 인풋필드
    public Button saveButton;               
    public TextMeshProUGUI leaderboardContentText; // 10명의 순위 출력 텍스트
    public Button restartButton;            

    private LeaderboardManager leaderboardManager;
    private GameManager gameManager;        
    private int cachedStage = 1;
    private float cachedTime = 0f;

    private void Awake()
    {
        if (resultStatusText != null) resultStatusText.text = "";
        
        leaderboardManager = GetComponent<LeaderboardManager>();
        if (leaderboardManager == null) leaderboardManager = gameObject.AddComponent<LeaderboardManager>();

        gameManager = FindAnyObjectByType<GameManager>();

        SetGiveUpPopupActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void UpdateStageText(int stageNum)
    {
        if (stageText != null) stageText.text = $"STAGE {stageNum}";
    }

    public void UpdateTimerText(float time)
    {
        if (timerText != null) timerText.text = $"TIME: {time:F1}s";
    }

    public void ShowResultText(string message)
    {
        if (resultStatusText != null) resultStatusText.text = message;
    }

    public void SetGiveUpPopupActive(bool isActive)
    {
        if (giveUpPopupPanel != null)
        {
            giveUpPopupPanel.SetActive(isActive);
        }
    }

    // 🎯 [시퀀스 1단계] 게임 종료 시 입력창 모드 활성화
    public void OpenLeaderboardInput(int finalStage, float takenTime)
    {
        cachedStage = finalStage;
        cachedTime = takenTime;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);

            // 축하 타이틀 및 입력 양식 ON
            if (titleText != null) titleText.gameObject.SetActive(true);
            if (nameInputField != null)
            {
                nameInputField.gameObject.SetActive(true);
                nameInputField.text = "";
                nameInputField.ActivateInputField(); 
            }
            if (saveButton != null) saveButton.gameObject.SetActive(true);

            // 결과판과 리스타트 버튼은 숨김
            if (leaderboardContentText != null) leaderboardContentText.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(false);
        }
    }

    // 🎯 [시퀀스 2단계] Save 버튼 연동 (겹침 현상 해결)
    public void OnClickSubmitRecord()
    {
        if (nameInputField == null || leaderboardManager == null) return;

        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) return; 

        // 1. 데이터 저장
        leaderboardManager.AddNewRecord(playerName, cachedStage, cachedTime);

        // 2. 💡 [핵심] 겹침 방지를 위해 타이틀 문구와 입력 UI를 모두 함께 꺼줍니다!
        if (titleText != null) titleText.gameObject.SetActive(false);
        nameInputField.gameObject.SetActive(false);
        if (saveButton != null) saveButton.gameObject.SetActive(false);

        // 3. 가독성 스킨이 입혀진 랭킹 데이터 출력 및 리스타트 활성화
        DisplayLeaderboard();
        if (leaderboardContentText != null) leaderboardContentText.gameObject.SetActive(true);
        if (restartButton != null) restartButton.gameObject.SetActive(true);
    }

    public void OnClickLeaderboardRestart()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (gameManager != null) gameManager.LoadStage(0); 
    }

    // 🎯 게임 테마에 맞춘 가독성 만점 녹색 계열 텍스트 가공
    // 🎯 게임 테마에 맞춘 [가독성 강화] 진한 녹색 계열 텍스트 가공
// 🎯 [색상 커스텀 & 라인 칼정렬] 버전 명예의 전당 텍스트 출력 함수
    private void DisplayLeaderboard()
    {
        if (leaderboardContentText == null || leaderboardManager == null) return;

        List<LeaderboardEntry> list = leaderboardManager.GetLeaderboard();

        // 💡 [헤더 정렬] <pos> 태그로 RANK, NAME, STAGE, TIME이 시작될 절대적인 가로 위치를 픽셀 단위로 고정합니다.
        // 게임에 어울리는 명시성 높은 딥 민트 그린(#12D371) 컬러 유지
        string displayText = "<color=#12D371>RANK<pos=18%>NAME<pos=55%>STAGE<pos=78%>TIME</color>\n";
        displayText += "<color=#3D664C>------------------------------------------------</color>\n";

        for (int i = 0; i < 10; i++)
        {
            if (i < list.Count)
            {
                // 🟢 [요청사항 반영 1] 등수(RANK)는 묵직하고 선명한 '진한 연두색(#52B30C)' 적용
                displayText += $"<color=#52B30C>{i + 1}</color>";

                // 🟢 [요청사항 반영 2] 이름, 스테이지, 시간 데이터는 흰색을 완전히 배제하고 '에메랄드 국방색(#3A7F5A)' 일괄 세팅
                // 데이터들 역시 헤더와 소수점 위치까지 완벽하게 수직 라인이 일치하도록 <pos> 좌표를 매칭합니다.
                displayText += $"<pos=18%><color=#3A7F5A>{list[i].playerName}</color>";
                displayText += $"<pos=55%><color=#3A7F5A>{list[i].finalStage}</color>";
                displayText += $"<pos=78%><color=#3A7F5A>{list[i].clearTime:F1}s</color>\n";
            }
            else
            {
                // 🟢 [요청사항 반영 3] 빈 슬롯 데이터(-) 역시 흰색 없이 차분하게 묻히는 '톤다운된 에메랄드 국방색(#2F5E43)' 처리
                displayText += $"<color=#2F5E43>{i + 1}</color>";
                displayText += $"<pos=18%><color=#2F5E43>-</color>";
                displayText += $"<pos=55%><color=#2F5E43>-</color>";
                displayText += $"<pos=78%><color=#2F5E43>-</color>\n";
            }
        }

        leaderboardContentText.text = displayText;
    }
}