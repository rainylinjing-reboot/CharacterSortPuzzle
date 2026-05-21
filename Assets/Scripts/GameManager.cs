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
    public StageData[] allStages;       
    private int currentStageIndex = 0;   

    [Header("[ 조작 상태 변수 ]")]
    public CharacterPiece selectedCharacter = null;

    private StageData currentStageData; 
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isGiveUpConfirming = false; // [1단계] 현재 항복 확인 창이 켜져 있는지 여부
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        currentStageIndex = 0;
        LoadStage(currentStageIndex);
    }

    public void LoadStage(int index)
    {
        if (allStages == null || allStages.Length == 0) return;

        if (index < allStages.Length)
        {
            currentStageData = allStages[index];
            InitializeGame();
        }
        else
        {
            AllStageClear();
        }
    }

    private void InitializeGame()
    {
        if (currentStageData == null || boardManager == null || uiManager == null) return;

        // 게임 일시정지 상태가 있을 수 있으므로 시간 축 정상화
        Time.timeScale = 1f;
        isGiveUpConfirming = false;
        if (uiManager != null) uiManager.SetGiveUpPopupActive(false);

        ClearSelectedCharacter();
        ClearExistingCharacters();

        boardManager.SetupBoard(currentStageData);
        boardManager.SpawnCharacters(currentStageData);

        timeRemaining = currentStageData.timeLimit;
        uiManager.UpdateStageText(currentStageData.stageNumber);
        uiManager.UpdateTimerText(timeRemaining);
        uiManager.ShowResultText("");

        isGameActive = true;

        // 🔊 [사운드] 게임 시작 효과음 발동 (안전지대 탑재)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.startClip);
            SoundManager.Instance.PlayBGM(); // 혹시 꺼졌을지 모를 BGM 재개
        }
    }

    private void Update()
    {
        // 🚨 [1단계 추가] 항복 확인 상태에서 키보드 'Y' 누르면 즉시 리스타트 발동!
        if (isGiveUpConfirming)
        {
            if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
            {
                ConfirmGiveUpAndRestart();
                return;
            }
        }

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

    // 🚨 [1단계 추가] UI 항복 버튼을 누르면 실행되는 함수
    public void ClickGiveUpButton()
    {
        if (!isGameActive || isGiveUpConfirming) return;

        // 🔊 [사운드] 클릭 효과음
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        isGiveUpConfirming = true;
        Time.timeScale = 0f; // 게임 속도를 0으로 만들어 타이머와 조작을 일시 중지!
        
        if (uiManager != null)
        {
            uiManager.SetGiveUpPopupActive(true); // 항복 알림 팝업창 켜기
        }
        Debug.Log("🏳️ 항복하시겠습니까? 확인 창 활성화. [Y] 키를 누르면 리스타트됩니다.");
    }

    // 🚨 [1단계 추가] 팝업창에서 취소(창 닫기)를 눌렀을 때
    public void CancelGiveUp()
    {
        // 🔊 [사운드] 클릭 효과음
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        isGiveUpConfirming = false;
        Time.timeScale = 1f; // 게임 속도를 다시 원래대로 복구!
        
        if (uiManager != null) uiManager.SetGiveUpPopupActive(false);
    }

    // 🚨 [1단계 추가] 항복 수락 (Y키 또는 확인 버튼 클릭 시) 실제 초기화 작동
    public void ConfirmGiveUpAndRestart()
    {
        Debug.Log("🔄 항복 수락됨. 현재 스테이지를 처음부터 다시 시작합니다.");
        // 현재 진행 중이던 스테이지 인덱스를 그대로 넘겨 판을 리셋합니다.
        LoadStage(currentStageIndex);
    }

    private void HandleMouseClick()
    {
        if (mainCamera == null || isGiveUpConfirming) return;

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
            // 🔊 [사운드] 캐릭터 선택 성공 클릭음 재생
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

            SetSelectedCharacter(clickedPiece);
            Debug.Log($"✅ [선택 완료] '{clickedPiece.characterType}' 달릴 준비 완료!");
        }
    }

    private void ProcessSlotClick(Slot clickedSlot)
    {
        if (selectedCharacter == null || !clickedSlot.IsEmpty) return;

        LineController startLine = selectedCharacter.currentSlot.ownerLine;
        LineController targetLine = clickedSlot.ownerLine;

        if (startLine == targetLine) return;

        Slot realTargetSlot = targetLine.GetFirstEmptySlot();

        if (realTargetSlot != null)
        {
            // 🔊 [사운드] 목적지 빈 슬롯 클릭 성공음 재생
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

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

            // 도착 프레임 직접 검증 방식으로 6차 변경 완료했으므로 Invoke 줄은 삭제 유지!
        }
    }

    // 캐릭터들이 목적지에 안착했을 때 직접 찔러주는 무결점 성공 판정 함수
    public void CheckStageClearConditionDirect()
    {
        if (!isGameActive) return;

        if (AnyCharacterMoving()) return;

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

        StartCoroutine(StageClearRoutine());
    }

    private IEnumerator StageClearRoutine()
    {
        isGameActive = false; 

        // 🔊 [사운드] 스테이지 클리어 대성공 사운드 재생 + BGM 잠시 뮤트
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clearClip);
        }

        if (uiManager != null) uiManager.ShowResultText("STAGE CLEAR!");
        
        yield return new WaitForSeconds(2.5f);

        currentStageIndex++;
        LoadStage(currentStageIndex);
    }

    private void ClearExistingCharacters()
    {
        CharacterPiece[] existingPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (CharacterPiece piece in existingPieces)
        {
            Destroy(piece.gameObject);
        }

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
        
        // 🔊 [사운드] 최종 올 클리어 시에도 축하 효과음 발동
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clearClip);

        if (uiManager != null) uiManager.ShowResultText("ALL STAGES CLEAR!");
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

        // 🔊 [사운드] 게임 오버 실패 사운드 재생 및 BGM 정지
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(SoundManager.Instance.failClip);
        }

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
        if (selectedCharacter != null) selectedCharacter.SetSelectedOutline(true);
    }

    private void ClearSelectedCharacter()
    {
        if (selectedCharacter != null) selectedCharacter.SetSelectedOutline(false);
        selectedCharacter = null;
    }
}