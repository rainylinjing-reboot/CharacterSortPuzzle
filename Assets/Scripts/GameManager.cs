using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("[ 매니저 참조 ]")]
    public BoardManager boardManager;
    public UIManager uiManager;

    [Header("[ 스테이지 레벨 데이터 관리 ]")]
    public StageData[] allStages;       // 스테이지 데이터들을 순서대로 넣어둘 배열
    private int currentStageIndex = 0;   // 현재 진행 중인 스테이지 배열 인덱스 (0부터 시작)

    [Header("[ 조작 상태 변수 ]")]
    public CharacterPiece selectedCharacter = null;

    private StageData currentStageData; // 현재 활성화된 스테이지 실제 데이터
    private float timeRemaining;
    private bool isGameActive = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // 첫 번째 스테이지로 인덱스 설정 후 시작
        currentStageIndex = 0;
        LoadStage(currentStageIndex);
    }

    // 특정 인덱스의 스테이지 데이터를 로드하는 함수
    public void LoadStage(int index)
    {
        if (allStages == null || allStages.Length == 0)
        {
            Debug.LogError("🚨 GameManager에 StageData가 등록되지 않았습니다! 인스펙터를 확인해 주세요.");
            return;
        }

        if (index < allStages.Length)
        {
            currentStageData = allStages[index];
            InitializeGame();
        }
        else
        {
            // 배열 범위를 벗어났다면 준비된 모든 스테이지를 다 정복한 상태!
            AllStageClear();
        }
    }

    private void InitializeGame()
    {
        if (currentStageData == null || boardManager == null || uiManager == null) return;

        ClearSelectedCharacter();

        // [중요] 다음 스테이지 소환 전, 보드 위에 남아있던 이전 판 캐릭터들을 완벽히 청소합니다.
        ClearExistingCharacters();

        // 새 스테이지 세팅 및 스폰
        boardManager.SetupBoard(currentStageData);
        boardManager.SpawnCharacters(currentStageData);

        // 타이머 및 UI 세팅
        timeRemaining = currentStageData.timeLimit;
        uiManager.UpdateStageText(currentStageData.stageNumber);
        uiManager.UpdateTimerText(timeRemaining);
        uiManager.ShowResultText("");

        selectedCharacter = null;
        isGameActive = true;
        Debug.Log($"🎮 [스테이지 시작] Stage {currentStageData.stageNumber} 가 활성화되었습니다!");
    }

    private void Update()
    {
        if (!isGameActive) return;

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

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (mainCamera == null) return;

        Vector2 mousePosition = Pointer.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;

            CharacterPiece clickedCharacter = hitObject.GetComponentInParent<CharacterPiece>();
            if (clickedCharacter != null)
            {
                ProcessCharacterClick(clickedCharacter);
                return;
            }

            Slot clickedSlot = hitObject.GetComponentInParent<Slot>();
            if (clickedSlot != null)
            {
                ProcessSlotClick(clickedSlot);
                return;
            }
        }
    }

    private void ProcessCharacterClick(CharacterPiece clickedPiece)
    {
        if (clickedPiece.IsMoving) return;
        if (clickedPiece.currentSlot == null) return;

        LineController line = clickedPiece.currentSlot.ownerLine;
        if (line == null) return;

        CharacterPiece topPiece = line.GetTopCharacter();

        if (clickedPiece == topPiece)
        {
            SetSelectedCharacter(clickedPiece);
            Debug.Log($"✅ [선택 완료] '{clickedPiece.characterType}' 달릴 준비 완료!");
        }
    }

    private void ProcessSlotClick(Slot clickedSlot)
    {
        if (selectedCharacter == null || !clickedSlot.IsEmpty) return;

        LineController startLine = selectedCharacter.currentSlot.ownerLine;
        LineController targetLine = clickedSlot.ownerLine;

        if (startLine == targetLine)
        {
            Debug.LogWarning("⚠️ [조작 거부] 같은 줄 내부로는 이동할 수 없습니다!");
            return;
        }

        Slot realTargetSlot = targetLine.GetFirstEmptySlot();

        if (realTargetSlot != null)
        {
            List<Slot> calculatedPath = new List<Slot>();

            int exitGateIndex = 4;
            if (startLine.slots[4] != null && !startLine.slots[4].IsEmpty) exitGateIndex = 5;

            int startIdx = selectedCharacter.currentSlot.slotIndex;
            for (int i = startIdx + 1; i <= exitGateIndex; i++)
            {
                if (startLine.slots[i] != null) calculatedPath.Add(startLine.slots[i]);
            }

            int enterGateIndex = 4;
            if (targetLine.slots[4] != null && !targetLine.slots[4].IsEmpty) enterGateIndex = 5;

            if (exitGateIndex != enterGateIndex)
            {
                if (exitGateIndex == 4 && startLine.slots[5] != null) calculatedPath.Add(startLine.slots[5]);
                if (enterGateIndex == 4 && targetLine.slots[5] != null) calculatedPath.Add(targetLine.slots[5]);
            }

            if (targetLine.slots[enterGateIndex] != null) calculatedPath.Add(targetLine.slots[enterGateIndex]);

            int targetIdx = realTargetSlot.slotIndex;
            for (int i = enterGateIndex - 1; i >= targetIdx; i--)
            {
                if (targetLine.slots[i] != null) calculatedPath.Add(targetLine.slots[i]);
            }

            selectedCharacter.MoveAlongPath(calculatedPath);
            ClearSelectedCharacter();

            CancelInvoke("CheckStageClearCondition");
            Invoke("CheckStageClearCondition", 1.2f);
        }
    }

    private void CheckStageClearCondition()
    {
        if (!isGameActive) return;

        if (AnyCharacterMoving())
        {
            Invoke("CheckStageClearCondition", 0.5f);
            return;
        }

        if (boardManager.waitingLine != null)
        {
            foreach (Slot slot in boardManager.waitingLine.slots)
            {
                if (slot.gameObject.activeSelf && !slot.IsEmpty) return;
            }
        }

        for (int i = 0; i < currentStageData.activeLines; i++)
        {
            LineController line = boardManager.mainLines[i];
            if (line == null) continue;

            if (line.slots[0].IsEmpty) return;
            CharacterType targetType = line.slots[0].GetCharacter().characterType;

            for (int j = 0; j < 4; j++)
            {
                if (line.slots[j].IsEmpty || line.slots[j].GetCharacter().characterType != targetType) return;
            }
        }

        // 통과 시 코루틴을 활용한 스테이지 클리어 연출 및 로드 진행
        StartCoroutine(StageClearRoutine());
    }

    // [핵심 추가] 스테이지 클리어 연출 및 자동 다음 스테이지 전환 코루틴
    private IEnumerator StageClearRoutine()
    {
        isGameActive = false; // 조작 및 타이머 일시 정지

        if (uiManager != null) uiManager.ShowResultText("STAGE CLEAR!");
        Debug.Log("🎉 스테이지 클리어! 잠시 후 다음 스테이지로 전환됩니다.");

        // 플레이어가 승리의 기쁨을 만끽하고 UI를 볼 수 있도록 2.5초간 대기합니다.
        yield return new WaitForSeconds(2.5f);

        // 다음 스테이지로 인덱스 1 증가
        currentStageIndex++;

        // 다음 스테이지 로드 호출
        LoadStage(currentStageIndex);
    }

    // 보드 위의 구형 캐릭터들을 삭제해 주는 청소 함수
    private void ClearExistingCharacters()
    {
        CharacterPiece[] existingPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (CharacterPiece piece in existingPieces)
        {
            Destroy(piece.gameObject);
        }

        // 모든 슬롯의 연결 데이터도 깨끗하게 리셋
        if (boardManager != null)
        {
            foreach (var line in boardManager.mainLines)
            {
                if (line != null)
                {
                    foreach (var slot in line.slots) if (slot != null) slot.ClearSlot();
                }
            }
            if (boardManager.waitingLine != null)
            {
                foreach (var slot in boardManager.waitingLine.slots) if (slot != null) slot.ClearSlot();
            }
        }
    }

    private void AllStageClear()
    {
        isGameActive = false;
        ClearSelectedCharacter();
        if (uiManager != null) uiManager.ShowResultText("ALL STAGES CLEAR!");
        Debug.Log("🏆🏆🏆 대단합니다! 준비된 모든 스테이지를 완벽하게 클리어하셨습니다! 최종 승리! 🏆🏆🏆");
    }

    private bool AnyCharacterMoving()
    {
        CharacterPiece[] allPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece.IsMoving) return true;
        }
        return false;
    }

    private void GameOver()
    {
        isGameActive = false;
        ClearSelectedCharacter();
        if (uiManager != null) uiManager.ShowResultText("GAME OVER");
    }

    private void SetSelectedCharacter(CharacterPiece character)
    {
        if (selectedCharacter == character)
        {
            selectedCharacter.SetSelectedOutline(true);
            return;
        }

        ClearSelectedCharacter();

        selectedCharacter = character;
        if (selectedCharacter != null)
        {
            selectedCharacter.SetSelectedOutline(true);
        }
    }

    private void ClearSelectedCharacter()
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.SetSelectedOutline(false);
        }

        selectedCharacter = null;
    }
}
