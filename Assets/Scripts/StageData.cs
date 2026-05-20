using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "PuzzleGame/StageData")]
public class StageData : ScriptableObject
{
    [Header("[ 스테이지 기본 설정 ]")]
    public int stageNumber;          // 스테이지 번호
    public int activeLines = 4;      // 사용할 메인 라인의 개수 (Stage 1 = 4, Stage 2 = 5)
    public int characterTypeCount = 4;// 사용할 캐릭터 종류 수 (Stage 1 = 4, Stage 2 = 5)
    public float timeLimit = 90f;    // 제한 시간 (Stage 1 = 90s, Stage 2 = 120s)

    [Header("[ 대기열 설정 ]")]
    public int waitingSlotCount = 5; // 대기열 슬롯 수 (기본 5개)
}