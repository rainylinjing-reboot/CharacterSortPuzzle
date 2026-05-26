using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("[ 상단 인게임 UI 요소 ]")]
    public TextMeshProUGUI stageText;       
    public TextMeshProUGUI timerText;       
    public TextMeshProUGUI resultText;      

    [Header("[ 스테이지 시작 카운트다운 UI ]")]
    public TextMeshProUGUI countdownText;

    [Header("[ 포기(Give Up) 팝업 패널 ]")]
    public GameObject giveUpPopupPanel;     

    [Header("[ 명예의 전당(Leaderboard) 패널 ]")]
    public GameObject leaderboardPanel;     
    public TextMeshProUGUI congratulationText; 
    public TMP_InputField nameInputField;   
    public Button saveButton;               
    
    [Header("[ 랭킹 리스트 출력 설정 ]")]
    public TextMeshProUGUI leaderboardContentText; 

    [Header("[ 리트라이 버튼 오브젝트 제어 ]")]
    public GameObject reStartButtonObject; 

    private int cachedFinalStage = 1;
    private float cachedTakenTime = 0f;

    private void Awake()
    {
        if (resultText != null) resultText.text = "";

        if (countdownText == null)
        {
            GameObject countdownObject = GameObject.Find("CountdownText");
            if (countdownObject != null) countdownText = countdownObject.GetComponent<TextMeshProUGUI>();
        }

        ClearCountdownText();
    }

    private void Start()
    {
        SetGiveUpPopupActive(false);
        CloseLeaderboardInput();
    }

    public void UpdateStageText(int stageNumber)
    {
        if (stageText != null) stageText.text = $"STAGE {stageNumber}";
    }

    public void UpdateTimerText(float timeRemaining)
    {
        if (timerText != null) timerText.text = $"TIME: {timeRemaining:F1}s";
    }

    public void ShowResultText(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true); 
        }
    }

    public IEnumerator PlayCountdownRoutine()
    {
        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);

        string[] countdownSteps = { "3", "2", "1", "GO!" };
        foreach (string step in countdownSteps)
        {
            countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }

        ClearCountdownText();
    }

    public void ClearCountdownText()
    {
        if (countdownText == null) return;

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }

    public void SetGiveUpPopupActive(bool isActive)
    {
        if (giveUpPopupPanel != null) giveUpPopupPanel.SetActive(isActive);
        if (isActive && leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void OpenLeaderboardInput(int finalStage, float takenTime)
    {
        if (giveUpPopupPanel != null && giveUpPopupPanel.activeSelf) return;

        cachedFinalStage = finalStage;
        cachedTakenTime = takenTime;

        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (reStartButtonObject != null) reStartButtonObject.SetActive(false);

        if (congratulationText != null)
        {
            congratulationText.gameObject.SetActive(true);
            congratulationText.text = "CONGRATULATIONS!!"; 
        }

        if (leaderboardContentText != null) leaderboardContentText.text = ""; 

        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = ""; 
            nameInputField.ActivateInputField(); 
        }
        if (saveButton != null) saveButton.gameObject.SetActive(true);
    }

    public void CloseLeaderboardInput()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void OnClickSubmitScore()
    {
        if (nameInputField == null || string.IsNullOrEmpty(nameInputField.text)) return;

        string playerName = nameInputField.text.Trim();
        Debug.Log($"💾 명예의 전당 데이터 등록 시도: {playerName}");

        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (saveButton != null) saveButton.gameObject.SetActive(false);

        if (congratulationText != null)
        {
            congratulationText.text = "";
            congratulationText.gameObject.SetActive(false);
        }

        LeaderboardManager lm = FindFirstObjectByType<LeaderboardManager>();
        if (lm != null)
        {
            lm.AddNewRecord(playerName, cachedFinalStage, cachedTakenTime);
            UpdateLeaderboardDisplay(lm.GetLeaderboard());
        }
        else
        {
            List<LeaderboardEntry> tempList = new List<LeaderboardEntry> { new LeaderboardEntry(playerName, cachedFinalStage, cachedTakenTime) };
            UpdateLeaderboardDisplay(tempList);
        }

        if (reStartButtonObject != null) reStartButtonObject.SetActive(true);
    }

    public void OnClickRetryGame()
    {
        Time.timeScale = 1f;

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.LoadStage(0); 
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        CloseLeaderboardInput();
    }

    private void UpdateLeaderboardDisplay(List<LeaderboardEntry> list)
    {
        if (leaderboardContentText == null) return;

        if (list == null || list.Count == 0)
        {
            leaderboardContentText.text = "No Records Yet.";
            return;
        }

        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("RANK<pos=180>NAME<pos=450>STAGE<pos=700>TIME");
        sb.AppendLine("------------------------------------------------------------------------");

        for (int i = 0; i < list.Count; i++)
        {
            string rankPrefix = $"{i + 1}st";
            if (i == 1) rankPrefix = "2nd";
            if (i == 2) rankPrefix = "3rd";
            if (i >= 3) rankPrefix = $"{i + 1}th";

            sb.AppendLine($"<color=#00FF00>{rankPrefix}</color><pos=180><color=#50C878>{list[i].playerName}</color><pos=450><color=#50C878>STAGE {list[i].finalStage}</color><pos=700><color=#50C878>({list[i].clearTime:F1}s)</color>");
        }

        leaderboardContentText.text = sb.ToString();
        leaderboardContentText.gameObject.SetActive(true);
    }
}
