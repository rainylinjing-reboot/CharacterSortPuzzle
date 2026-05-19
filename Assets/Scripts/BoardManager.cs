using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("[ 라인 컴포넌트 등록 ]")]
    public LineController[] mainLines;       // 메인 보드의 최대 5개 라인
    public LineController waitingLine;       // 대기열 라인 1개

    // 현재 스테이지 조건에 맞춰 보드 판의 라인들을 활성화/비활성화
    public void SetupBoard(StageData stageData)
    {
        if (stageData == null)
        {
            Debug.LogError("StageData가 보드매니저에 입력되지 않았습니다.");
            return;
        }

        // 1. 메인 라인 세팅 (설정된 개수만큼만 활성화)
        for (int i = 0; i < mainLines.Length; i++)
        {
            if (mainLines[i] != null)
            {
                // 스테이지 라인 수보다 작은 인덱스만 켜고, 나머지는 끎
                bool isLineActive = i < stageData.activeLines;
                mainLines[i].gameObject.SetActive(isLineActive);
                mainLines[i].lineIndex = i;
            }
        }

        // 2. 대기열 라인 세팅 (항상 활성화하되 슬롯 수 조건 방어)
        if (waitingLine != null)
        {
            waitingLine.gameObject.SetActive(true);
            waitingLine.isWaitingLine = true;

            // 대기열 내부 슬롯 개수 세팅 방어 코드
            for (int i = 0; i < waitingLine.slots.Length; i++)
            {
                if (waitingLine.slots[i] != null)
                {
                    bool isSlotActive = i < stageData.waitingSlotCount;
                    waitingLine.slots[i].gameObject.SetActive(isSlotActive);
                }
            }
        }

        Debug.Log($"Stage {stageData.stageNumber} 보드 구성 완료. 활성 라인: {stageData.activeLines}");
    }
}