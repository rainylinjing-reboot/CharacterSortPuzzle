using UnityEngine;
using System.Collections.Generic;

// 하나의 랭킹 정보를 담을 단위 구조체
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;  // 이름 (NAME)
    public int finalStage;     // 최종 스테이지 (STAGE)
    public float clearTime;    // 기록 초수 (TIME)

    public LeaderboardEntry(string name, int stage, float time)
    {
        this.playerName = name;
        this.finalStage = stage;
        this.clearTime = time;
    }
}

// 💡 [에러 해결 핵심] 중복 이름 충돌을 피하기 위해 클래스명을 유니크하게 변경했습니다.
[System.Serializable]
public class PuzzleLeaderboardWrapper
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    private const string SAVE_KEY = "PuzzleGame_Leaderboard";
    private const int MAX_RECORD_COUNT = 10; // 최대 10명까지 기록 제한

    // 랭킹 리스트 로드하기
    public List<LeaderboardEntry> GetLeaderboard()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            return new List<LeaderboardEntry>();
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<LeaderboardEntry>();
        }

        // 💡 여기도 바뀐 클래스명으로 정렬 변환합니다.
        PuzzleLeaderboardWrapper data = JsonUtility.FromJson<PuzzleLeaderboardWrapper>(json);
        if (data == null || data.entries == null)
        {
            return new List<LeaderboardEntry>();
        }

        return data.entries;
    }

    // 새 기록 추가 및 상위 10명 정렬 저장 함수
    public void AddNewRecord(string name, int stage, float time)
    {
        if (string.IsNullOrEmpty(name)) name = "UNKNOWN";

        List<LeaderboardEntry> currentList = GetLeaderboard();

        // 1. 새 기록 추가
        currentList.Add(new LeaderboardEntry(name, stage, time));

        // 2. 랭킹 정렬 알고리즘 
        currentList.Sort((a, b) =>
        {
            if (a.finalStage != b.finalStage)
            {
                return b.finalStage.CompareTo(a.finalStage); // 스테이지 내림차순
            }
            return a.clearTime.CompareTo(b.clearTime); // 소요 시간 오름차순
        });

        // 3. 10명 커트라인 방어선
        if (currentList.Count > MAX_RECORD_COUNT)
        {
            currentList.RemoveRange(MAX_RECORD_COUNT, currentList.Count - MAX_RECORD_COUNT);
        }

        // 4. 로컬 기기에 JSON 데이터로 안전하게 박제 저장
        // 💡 바뀐 클래스명으로 래핑하여 세이브 진행
        PuzzleLeaderboardWrapper dataToSave = new PuzzleLeaderboardWrapper { entries = currentList };
        string json = JsonUtility.ToJson(dataToSave);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"💾 명예의 전당 갱신 완료! 등록자: {name} (Stage {stage} / {time:F1}s)");
    }

    // 명예의 전당 기록을 초기화하고 싶을 때 쓰는 함수
    public void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("🗑️ 명예의 전당 데이터가 깨끗하게 초기화되었습니다.");
    }
}
